using Godot;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.stats;

namespace MPAutoChess.logic.core.unit;

public class Unit {
    
    public Stats Stats { get; private set; } = new Stats();
    
    public uint Level { get; private set; } = 1;
    
    public UnitType Type { get; private set; }
    
    public UnitContainer Container { get; set; }

    private UnitPool pool;

    private UnitInstance passiveInstance;
    
    public Unit(UnitType type, UnitPool pool) {
        Type = type;
        this.pool = pool;
        // TODO: Load stats from type
        Stats.GetCalculation(StatType.WIDTH).BaseValue = new ConstantValue(2f);
        Stats.GetCalculation(StatType.HEIGHT).BaseValue = new ConstantValue(2f);
    }
    
    ~Unit() {
        pool.ReturnUnit(Type);
        passiveInstance?.Dispose();
    }

    public float GetStatValue(StatType statType) {
        return Stats.GetValue(statType);
    }

    public Vector2 GetSize() {
        return new Vector2(Stats.GetValue(StatType.WIDTH), Stats.GetValue(StatType.HEIGHT));
    }

    public bool CanBePlacedAt(UnitContainer unitContainer, Vector2 position) {
        Unit? existingUnit = unitContainer.GetUnitAt(position);
        bool thisCanFit = unitContainer.CanFitAt(this, position, existingUnit);
        bool existingCanFit = existingUnit == null || Container.CanFitAt(existingUnit, Container.GetPlacement(this), this);
        return thisCanFit && existingCanFit;
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