using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.stats;
using ProtoBuf;

namespace MPAutoChess.logic.core.unit;

[ProtoContract]
public class Unit : IIdentifiable {
    public string Id { get; set; }
    
    [ProtoMember(1)] public Stats Stats { get; private set; } = new Stats();
    
    [ProtoMember(2)] public uint Level { get; private set; } = 1;
    
    [ProtoMember(3)] public UnitType Type { get; private set; }
    
    public UnitContainer Container { get; set; } // will be set by its container, no need for ProtoBuf serialization
    
    [ProtoMember(4)] private List<Unit> mergedCopies = new List<Unit>(); // keeping a reference of merged copies to prevent them from destructing and returning to the pool

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
        Stats.GetCalculation(StatType.WIDTH).BaseValue = new ConstantValue(Type.Size.X);
        Stats.GetCalculation(StatType.HEIGHT).BaseValue = new ConstantValue(Type.Size.Y);
        Stats.GetCalculation(StatType.MAX_HEALTH).BaseValue = new ConstantValue(Type.MaxHealth * levelFactor);
        Stats.GetCalculation(StatType.MAX_MANA).BaseValue = new ConstantValue(Type.MaxMana);
        Stats.GetCalculation(StatType.STARTING_MANA).BaseValue = new ConstantValue(Type.StartingMana);
        Stats.GetCalculation(StatType.ARMOR).BaseValue = new ConstantValue(Type.Armor);
        Stats.GetCalculation(StatType.AEGIS).BaseValue = new ConstantValue(Type.Aegis);
        Stats.GetCalculation(StatType.STRENGTH).BaseValue = new ConstantValue(Type.Strength * levelFactor);
        Stats.GetCalculation(StatType.ATTACK_SPEED).BaseValue = new ConstantValue(Type.AttackSpeed);
        Stats.GetCalculation(StatType.RANGE).BaseValue = new ConstantValue(Type.Range);
        Stats.GetCalculation(StatType.MAGIC).BaseValue = new ConstantValue(0);
        GD.Print("Base health now " + Stats.GetValue(StatType.MAX_HEALTH) + " for unit type " + Type.Name + " at level " + Level);
    }
    
    ~Unit() {
        pool?.ReturnUnit(Type);
        passiveInstance?.Dispose();
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
        foreach (Unit copy in copies) {
            copy.Container?.RemoveUnit(copy);
            
            // keep a reference to the merged copy to prevent it from being destructed (which returns it to the pool)
            mergedCopies.Add(copy);
            copy.passiveInstance.Dispose(); // dispose the passive instance here already (to save memory), since destructor will not be called until this unit is destructed
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
                    Stats.GetCalculation(statType).BaseValue = new ConstantValue(newStacks);
                }
            }
        }

        Level++;
        SetBaseStatsFromType();
        GetOrCreatePassiveInstance().Modulate = new Color(1f + 0.2f * (Level - 1), 1f + 0.2f * (Level - 1), 1f);
        GD.Print("Leveled up unit: " + Type.Name + " to level " + Level);
        EventManager.INSTANCE.NotifyAfter(levelUpEvent);
    }

    public UnitInstance CreateInstance(bool isCombatInstance) {
        UnitInstance instance = Type.UnitInstancePrefab.Instantiate<UnitInstance>();
        instance.Unit = this;
        instance.IsCombatInstance = false;
        return instance;
    }
    
    public UnitInstance GetOrCreatePassiveInstance() {
        if (passiveInstance == null) {
            passiveInstance = CreateInstance(false);
        }
        return passiveInstance;
    }
}