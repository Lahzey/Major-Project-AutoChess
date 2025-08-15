using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit.role;
using ProtoBuf;

namespace MPAutoChess.logic.core.unit;

[ProtoContract]
public class Unit : IIdentifiable {
    private const string ITEM_STAT_NAME = "Item Stats";
    private const int DEFAULT_MAX_ITEM_COUNT = 3;
    private int unitInstanceCounter = 0;
    
    public string Id { get; set; }
    
    [ProtoMember(1)] public Stats Stats { get; private set; } = new Stats();
    
    [ProtoMember(2)] public uint Level { get; private set; } = 1;
    
    [ProtoMember(3)] public UnitType Type { get; private set; }
    
    public UnitContainer Container { get; set; } // will be set by its container, no need for ProtoBuf serialization
    
    [ProtoMember(4)] private List<Unit> mergedCopies = new List<Unit>(); // keeping a reference of merged copies to prevent them from destructing and returning to the pool

    [ProtoMember(5)] public List<Item> EquippedItems { get; private set; } = new List<Item>();

    private UnitPool pool;

    private UnitInstance passiveInstance;

    public Unit() { } // empty constructor for MessagePack serialization

    public Unit(UnitType type, UnitPool pool) {
        Type = type;
        this.pool = pool;
        SetBaseStatsFromType();
    }

    private void SetBaseStatsFromType() {
        float levelFactor = 1f + (Level - 1) * 0.5f; // TODO dynamic scaling factor for health and strength individually
        Stats.GetCalculation(StatType.WIDTH).BaseValue = Type.Size.X;
        Stats.GetCalculation(StatType.HEIGHT).BaseValue = Type.Size.Y;
        Stats.GetCalculation(StatType.MAX_HEALTH).BaseValue = Type.MaxHealth * levelFactor;
        Stats.GetCalculation(StatType.MAX_MANA).BaseValue = Type.MaxMana;
        Stats.GetCalculation(StatType.STARTING_MANA).BaseValue = Type.StartingMana;
        Stats.GetCalculation(StatType.ARMOR).BaseValue = Type.Armor;
        Stats.GetCalculation(StatType.AEGIS).BaseValue = Type.Aegis;
        Stats.GetCalculation(StatType.STRENGTH).BaseValue = Type.Strength * levelFactor;
        Stats.GetCalculation(StatType.ATTACK_SPEED).BaseValue = Type.AttackSpeed;
        Stats.GetCalculation(StatType.RANGE).BaseValue = Type.Range;
        Stats.GetCalculation(StatType.MOVEMENT_SPEED).BaseValue = Type.MovementSpeed;
        Stats.GetCalculation(StatType.MAGIC).BaseValue = 0;
    }

    private void ApplyItemStats() {
        Dictionary<StatType, float> itemStats = new Dictionary<StatType, float>();
        foreach (Item item in EquippedItems) {
            foreach (StatValue stat in item.Type.Stats) {
                if (itemStats.ContainsKey(stat.StatType)) {
                    itemStats[stat.StatType] += item.GetStat(stat.StatType);
                } else {
                    itemStats[stat.StatType] = item.GetStat(stat.StatType);
                }
            }
        }
        
        // clear previous item stats
        foreach (StatType statType in Stats.Types) {
            Stats.GetCalculation(statType).RemoveFlat(ITEM_STAT_NAME);
        }
        
        // add new item stats
        foreach ((StatType statType, float value) in itemStats) {
            if (value != 0) Stats.GetCalculation(statType).AddFlat(value, ITEM_STAT_NAME);
        }
    }

    public float GetStatValue(StatType statType) {
        return Stats.GetValue(statType);
    }

    public Vector2 GetSize() {
        return new Vector2(Stats.GetValue(StatType.WIDTH), Stats.GetValue(StatType.HEIGHT)); 
    }
    
    public int GetCopyCountForLevelUp() {
        return 3; // TODO make this modifiable with Event system
    }

    public void LevelUp(params Unit[] copies) {
        UnitLevelUpEvent levelUpEvent = new UnitLevelUpEvent(this, copies);
        EventManager.INSTANCE.NotifyBefore(levelUpEvent);
        List<Item> copyItems = new List<Item>();
        foreach (Unit copy in copies) {
            foreach (Item item in copy.EquippedItems.ToArray()) {
                if (EquipItem(item)) copy.RemoveItem(item, false);
            }
            copy.RemoveItems();
            copy.Container?.RemoveUnit(copy);
            
            // keep a reference to the merged copy to prevent it from being destructed (which returns it to the pool)
            mergedCopies.Add(copy);
            copy.passiveInstance?.QueueFree(); // dispose the passive instance here already (to save memory), since destructor will not be called until this unit is destructed
            copy.passiveInstance = null;
            // TODO instead of disposing immediately, play a merge animation and then dispose the instance

            foreach (StatType statType in copy.Stats.Types) {
                if (statType is StackableStatType stackableStatType) {
                    float currentStacks = Stats.GetCalculation(statType).BaseValue.Get();
                    float copyStacks = copy.Stats.GetCalculation(statType).BaseValue.Get();
                    float newStacks = stackableStatType.stackMode switch {
                        StackMode.LARGEST => Mathf.Max(currentStacks, copyStacks),
                        StackMode.SUM => currentStacks + copyStacks,
                        _ => throw new ArgumentOutOfRangeException(stackableStatType.stackMode + " is an unsupported stack mode")
                    };
                    Stats.GetCalculation(statType).BaseValue = newStacks;
                }
            }
        }

        Level++;
        SetBaseStatsFromType();
        GD.Print("Leveled up unit: " + Type.Name + " to level " + Level);
        EventManager.INSTANCE.NotifyAfter(levelUpEvent);
        ServerController.Instance.PublishChange(this);
    }
    
    public bool EquipItem(Item item) {
        if (EquippedItems.Count >= DEFAULT_MAX_ITEM_COUNT) {
            GD.PrintErr("Cannot equip more than " + DEFAULT_MAX_ITEM_COUNT + " items to a unit");
            return false;
        }
        EquippedItems.Add(item);
        item.Effect?.TryApply(item, GetOrCreatePassiveInstance());
        ApplyItemStats();
        ServerController.Instance.PublishChange(this);
        return true;
    }

    public void ReplaceItem(Item replacedItem, Item newItem) {
        int index = EquippedItems.IndexOf(replacedItem);
        if (index == -1) throw new ArgumentException("Item to replace not found in equipped items.", nameof(replacedItem));
        replacedItem.Effect?.TryRemove(replacedItem, GetOrCreatePassiveInstance());
        EquippedItems[index] = newItem;
        ApplyItemStats();
        newItem.Effect?.TryApply(newItem, GetOrCreatePassiveInstance());
        ServerController.Instance.PublishChange(this);
    }

    public void RemoveItem(Item item, bool addToInventory = true) {
        int index = EquippedItems.IndexOf(item);
        if (index == -1) throw new ArgumentException("Item to remove not found in equipped items.", nameof(item));
        
        if (addToInventory) {
            Player player = Container.GetPlayer();
            if (!player.Inventory.AddItem(item)) {
                GD.PrintErr($"Item was removed from unit {Type.Name} but could not be added to player inventory: {item.GetName()}");
            }
        }
        
        item.Effect?.TryRemove(item, GetOrCreatePassiveInstance());
        EquippedItems.RemoveAt(index);
        ApplyItemStats();
        ServerController.Instance.PublishChange(this);
    }

    public void RemoveItems(bool addToInventory = true) {
        Player player = Container.GetPlayer();
        foreach (Item item in EquippedItems) {
            // not calling RemoveItem repeatedly here to avoid multiple calls to ApplyItemStats and PublishChange
            if (addToInventory) {
                if (!player.Inventory.AddItem(item)) {
                    GD.PrintErr($"Item was removed from unit {Type.Name} but could not be added to player inventory: {item.GetName()}");
                }
            }
            item.Effect?.TryRemove(item, GetOrCreatePassiveInstance());
        }

        EquippedItems.Clear();
        ApplyItemStats();
        ServerController.Instance.PublishChange(this);
    }

    public UnitInstance CreateInstance(bool isCombatInstance, string name = null) {
        if (name == null && !ServerController.Instance.IsServer) throw new InvalidOperationException("Unnamed unit instances can only be created on the server.");

        string id = ((IIdentifiable)this).GetId(); // force ID generation so we can use it in the name (Id property is null if it has never been serialized before)
        UnitInstance instance = Type.UnitInstancePrefab.Instantiate<UnitInstance>();
        instance.Unit = this;
        instance.IsCombatInstance = isCombatInstance;
        instance.Name = $"{Type.Name}@{id}_Instance{name ?? (unitInstanceCounter++).ToString()}";

        if (isCombatInstance) {
            // SceneSafeMpSynchronizer synchronizer = new SceneSafeMpSynchronizer();
            // instance.AddChild(synchronizer);
            // synchronizer.RootPath = ".."; // parent which will be the UnitInstance itself
            // synchronizer.ReplicationConfig = new SceneReplicationConfig();
            // synchronizer.ReplicationConfig.AddProperty(".:" + Node2D.PropertyName.Position);
            // synchronizer.ReplicationConfig.AddProperty(".:" + Node2D.PropertyName.Rotation);
            // synchronizer.ReplicationConfig.AddProperty(".:" + Node2D.PropertyName.Scale);
        }
        
        return instance;
    }
    
    public UnitInstance GetOrCreatePassiveInstance() {
        if (passiveInstance == null) {
            passiveInstance = CreateInstance(false, "PassiveInstance");
        }
        return passiveInstance;
    }

    public Item? GetCraftingResultWith(Item item, out Item craftedFrom) {
        foreach (Item equippedItem in EquippedItems) {
            Item? resultingItem = GameSession.Instance.GetItemConfig().GetCraftingResult(item, equippedItem);
            if (resultingItem != null) {
                craftedFrom = equippedItem;
                return resultingItem;
            }
        }

        craftedFrom = null;
        return null;
    }
    
    public HashSet<UnitRole> GetRoles() {
        return Type.RoleSet?.Roles?? new HashSet<UnitRole>();
    }

    public bool HasRole(UnitRole role) {
        return Type.RoleSet.HasRole(role);
    }

    public void Dispose() {
        pool?.ReturnUnit(Type);
        passiveInstance?.QueueFree();
    }

    public int GetSellValue() {
        int level = (int)Level;
        return level * Type.Cost - (level - 1);
    }

    public void Sell() {
        RemoveItems();
        Container?.RemoveUnit(this);
        PlayerController.Current.Player.AddGold(GetSellValue());
        Dispose();
    }
}