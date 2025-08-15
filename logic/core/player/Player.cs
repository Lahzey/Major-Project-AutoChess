using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.environment;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.item.consumable;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.shop;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.player;

[ProtoContract]
public partial class Player : Node2D {
    
    private const int STARTING_HEALTH = 100;
    public const int COST_PER_XP = 1;
    public const int COST_PER_REROLL = 2;
    
    // some fields are exported so they can be synchronized with a MultiplayerSynchronizer node
    [ProtoMember(1)] public Account Account { get; private set; }
    [Export] [ProtoMember(2)] public int CurrentHealth { get; private set; }
    [Export] [ProtoMember(3)] public int MaxHealth { get; private set; } = STARTING_HEALTH;
    [Export] [ProtoMember(4)] public int Experience { get; private set; }
    [Export] [ProtoMember(5)] public int Level { get; private set; } = 1;
    [Export] [ProtoMember(6)] public int Gold { get; private set; }
    [Export] [ProtoMember(7)] public int FreeRerolls { get; set; }
    [ProtoMember(8)] public Shop Shop { get; private set; }

    public bool Dead => CurrentHealth <= 0;

    private Inventory inventory;

    [ProtoMember(9)]
    public Inventory Inventory {
        get => inventory;
        private set {
            inventory = value;
            inventory.Player = this;
        }
    }
    
    [ProtoMember(10)] private Dictionary<Consumable, uint> consumables = new Dictionary<Consumable, uint>();

    private Arena _arena;
    [ProtoMember(11)] public Arena Arena {
        get => _arena;
        private set {
            _arena = value;
            _arena.Player = this;
        }
    }

    public Board Board => Arena.Board;
    public Bench Bench => Arena.Bench;
    
    [ProtoMember(12)] public Calculation BoardSize { get; private set; } = new Calculation(1);

    public Player() {
        Shop = new Shop(this);
        Inventory = new Inventory(20);
        CurrentHealth = MaxHealth;
        
        foreach (Consumable consumable in Consumable.GetAll()) {
            consumables[consumable] = 0;
        }
        
        BoardSize.SetAutoSendChanges(true);
        
        // just for testing TODO: remove
        consumables[Consumable.Get<ItemRemover>()] = 5;
        consumables[Consumable.Get<ItemReroll>()] = 5;
        consumables[Consumable.Get<ItemUpgrade>()] = 5;
        consumables[Consumable.Get<UnitCloner>()] = 5;
    }

    public void SetAccount(Account account) {
        Account = account;
        Arena = ResourceLoader.Load<PackedScene>(Account.GetSelectedArenaType().GetPath()).Instantiate<Arena>();
        AddChild(Arena);
    }

    public override void _Ready() {
        if (!ServerController.Instance.IsServer) return;
        
        // do not call CheckForUnitLevelUp immediately as containers will update during the level up process (state might be incomplete during the level up)
        EventManager.INSTANCE.AddAfterListener((UnitContainerUpdateEvent e) => CallDeferred(MethodName.CheckForUnitLevelUp));
        EventManager.INSTANCE.AddAfterListener((PhaseChangeEvent e) => CallDeferred(MethodName.CheckForUnitLevelUp));
        EventManager.INSTANCE.AddAfterListener((UnitLevelUpEvent e) => CallDeferred(MethodName.CheckForUnitLevelUp));
    }

    public int GetXpForLevel(int level) {
        if (level <= 0) return -1;
         switch (level) {
            case 1: return 0;
            case 2: return 2;
            case 3: return 6;
            case 4: return 10;
            case 5: return 20;
            case 6: return 35;
            case 7: return 65;
            case 8: return 100;
            case 9: return 140;
            case 10: return 200; // Level 10 is the effective maximum level
            default: return 999 * (level - 10); // each level past level 10 requires so much xp its probably not doable
        }
    }

    private void OnExperienceGain() {
        while (Experience >= GetXpForLevel(Level + 1)) {
            Level++;
        }

        if (Level != Mathf.RoundToInt(BoardSize.BaseValue.Get())) {
            BoardSize.BaseValue = Level;
        }
    }
    
    public uint GetConsumableCount(Consumable consumable) {
        return consumables.TryGetValue(consumable, out uint count) ? count : 0;
    }
    
    public void SetConsumableCount(Consumable consumable, uint count) {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("SetConsumableCount can only be called on the server.");
        consumables[consumable] = count;
        ServerController.Instance.PublishChange(this);
    }

    private void CheckForUnitLevelUp() {
        Dictionary<Tuple<UnitType, uint>, List<Unit>> units = new Dictionary<Tuple<UnitType, uint>, List<Unit>>();

        if (!GameSession.Instance.IsInCombat(this)) {
            foreach (Unit unit in Board.GetUnits()) {
                Tuple<UnitType, uint> key = new Tuple<UnitType, uint>(unit.Type, unit.Level);
                if (!units.ContainsKey(key)) units.Add(key, new List<Unit>());
                units[key].Add(unit);
            }
        }
        
        foreach (SingleUnitSlot benchSlot in Bench.GetSlots()) {
            if (benchSlot.Unit != null) {
                Tuple<UnitType, uint> key = new Tuple<UnitType, uint>(benchSlot.Unit.Type, benchSlot.Unit.Level);
                if (!units.ContainsKey(key)) units.Add(key, new List<Unit>());
                units[key].Add(benchSlot.Unit);
            }
        }
        
        foreach (List<Unit> unitList in units.Values) {
            Unit firstUnit = unitList[0];
            int requiredForLevelUp = firstUnit.GetCopyCountForLevelUp();
            if (unitList.Count >= requiredForLevelUp) {
                Unit[] copies = unitList.GetRange(1, requiredForLevelUp - 1).ToArray();
                firstUnit.LevelUp(copies);
            }
        }
        
        
    }

    public bool TryPurchase(int cost, Action purchaseAction) {
        if (Gold >= cost) {
            Gold -= cost;
            purchaseAction();
            return true;
        } else {
            return false;
        }
    }
    
    public void MoveToTemporaryBench(Unit unit) {
        // TODO: Implement logic to move the unit to a temporary bench
        unit.Sell();
    }

    public void TakeDamage(float damage, Player? source) {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("TakeDamage can only be called on the server.");
        PlayerDamageEvent playerDamageEvent = new PlayerDamageEvent(source, this, damage);
        EventManager.INSTANCE.NotifyBefore(playerDamageEvent);
        CurrentHealth -= Mathf.RoundToInt(damage);
        if (CurrentHealth <= 0) Kill();
        EventManager.INSTANCE.NotifyAfter(playerDamageEvent);
    }

    public void Kill() {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("Kill can only be called on the server.");
        PlayerDeathEvent deathEvent = new PlayerDeathEvent(this);
        EventManager.INSTANCE.NotifyBefore(deathEvent);
        if (deathEvent.IsCancelled) {
            CurrentHealth = 1;
            return;
        }
        CurrentHealth = 0;
        
        foreach (Unit unit in Board.GetUnits().ToList()) {
            Board.RemoveUnit(unit);
            unit.Dispose();
        }
        
        foreach (SingleUnitSlot benchSlot in Bench.GetSlots()) {
            if (benchSlot.Unit != null) {
                Unit unit = benchSlot.Unit;
                benchSlot.RemoveUnit(unit);
                unit.Dispose();
            }
        }
        
        EventManager.INSTANCE.NotifyAfter(deathEvent);
        Rpc(MethodName.NotifyDeath);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void NotifyDeath() {
        CurrentHealth = 0;
        PlayerDeathEvent deathEvent = new PlayerDeathEvent(this);
        EventManager.INSTANCE.NotifyBefore(deathEvent);
        EventManager.INSTANCE.NotifyAfter(deathEvent);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void AddGold(int gold) {
        if (ServerController.Instance.IsServer) {
            Gold += gold;
            Rpc(MethodName.AddGold, gold);
        } else {
            // TODO gold gain animation
        }
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void AddExperience(int experience) {
        if (ServerController.Instance.IsServer) {
            Experience += experience;
            OnExperienceGain();
            Rpc(MethodName.AddExperience, experience);
        } else {
            // TODO experience gain animation
        }
    }

    public void AddInterest() {
        int interest = Mathf.FloorToInt(Gold * 0.1f);
        if (interest > 5) interest = 5;
        AddGold(interest);
    }
}