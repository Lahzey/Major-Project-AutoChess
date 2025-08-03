using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using ProtoBuf;

namespace MPAutoChess.logic.core.stats;

[ProtoContract(Surrogate = typeof(StatTypeSurrogate))]
public class StatType {

    private static readonly Dictionary<string, StatType> STAT_TYPES = new Dictionary<string, StatType>();
    
    public readonly string Name;
    public readonly string Description;
    public readonly Texture2D Icon;
    public readonly uint DigitsAfterDecimal;
    public readonly bool IsPercentage;

    public StatType() { } // for ProtoBuf serialization

    public StatType(string name, string description, Texture2D icon, uint digitsAfterDecimal = 0, bool isPercentage = false) {
        Name = name;
        Description = description;
        Icon = icon;
        DigitsAfterDecimal = digitsAfterDecimal;
        IsPercentage = isPercentage;
        if (STAT_TYPES.ContainsKey(name))
            throw new System.ArgumentException($"StatType with name '{name}' already exists.");
        STAT_TYPES.Add(name, this);
    }
    
    public static StatType Parse(string name) {
        if (STAT_TYPES.TryGetValue(name, out StatType statType)) {
            return statType;
        }
        throw new System.ArgumentException($"StatType with name '{name}' does not exist.");
    }

    public static StatType[] GetAllValues() {
        return STAT_TYPES.Values.ToArray();
    }

    public static readonly StatType MAX_HEALTH = new StatType("Max Health", "Maximum health of the unit.", GD.Load<Texture2D>("res://assets/ui/max_health_icon.png"));
    public static readonly StatType MAX_MANA = new StatType("Max Mana", "The amount of Mana required to cast the spell.", GD.Load<Texture2D>("res://assets/ui/max_mana_icon.png"));
    public static readonly StatType HEALTH_REGEN = new StatType("Health Regeneration", "The amount of health recovered each second.", GD.Load<Texture2D>("res://assets/ui/health_regen_icon.png"));
    public static readonly StatType MANA_REGEN = new StatType("Mana Regeneration", "The amount of mana generated passively each second.", GD.Load<Texture2D>("res://assets/ui/mana_regen_icon.png"), 1);
    public static readonly StatType STARTING_MANA = new StatType("Starting Mana", "The amount of Mana the unit starts combat with.", GD.Load<Texture2D>("res://assets/ui/max_mana_icon.png"));
    public static readonly StatType ARMOR = new StatType("Armor", "Reduces incoming physical damage.", GD.Load<Texture2D>("res://assets/ui/armor_icon.png"));
    public static readonly StatType AEGIS = new StatType("Aegis", "Reduces incoming magical damage.", GD.Load<Texture2D>("res://assets/ui/aegis_icon.png"));
    public static readonly StatType TENACITY = new StatType("Tenacity", "Reduces the duration of incoming crowd control.", GD.Load<Texture2D>("res://assets/ui/tenacity_icon.png"), 0, true);
    public static readonly StatType STRENGTH = new StatType("Strength", "Damage dealt by the unit's attacks. Some spell cast are also improved by this.", GD.Load<Texture2D>("res://assets/ui/strength_icon.png"));
    public static readonly StatType MAGIC = new StatType("Magic", "Improves the units spell casts.", GD.Load<Texture2D>("res://assets/ui/magic_icon.png"));
    public static readonly StatType ATTACK_SPEED = new StatType("Attack Speed", "How often the unit attacks each second.", GD.Load<Texture2D>("res://assets/ui/attack_speed_icon.png"), 2);
    public static readonly StatType BONUS_ATTACK_SPEED = new StatType("Bonus Attack Speed", "Acts as a multiplier to attack speed.", GD.Load<Texture2D>("res://assets/ui/attack_speed_icon.png"), 0, true);
    public static readonly StatType CRIT_CHANCE = new StatType("Crit Chance", "The chance for physical attacks to critically strike.", GD.Load<Texture2D>("res://assets/ui/crit_chance_icon.png"), 0, true);
    public static readonly StatType CRIT_DAMAGE = new StatType("Crit Damage", "The amount of bonus damage dealt with a critical strike.", GD.Load<Texture2D>("res://assets/ui/crit_damage_icon.png"), 0, true);
    public static readonly StatType RANGE = new StatType("Range", "The distance at which the unit can attack.", GD.Load<Texture2D>("res://assets/ui/max_health_icon.png"));
    public static readonly StatType WIDTH = new StatType("Width", "The width of the unit in grid cells.", null);
    public static readonly StatType HEIGHT = new StatType("Height", "The height of the unit in grid cells.", null);
    public static readonly StatType MOVEMENT_SPEED = new StatType("Movement Speed", "The amount of cells the unit can traverse per second.", null);

    public string ToString(float statVal, int additionalDigits = 0) {
        int digitsAfterDecimal = (int) DigitsAfterDecimal + additionalDigits;
        if (IsPercentage) statVal *= 100f;
        if (digitsAfterDecimal == 0) return Mathf.RoundToInt(statVal) + (IsPercentage ? "%" : "");
        else return statVal.ToString("0." + new string('#', digitsAfterDecimal)) + (IsPercentage ? "%" : "");
    }
}

public class StackableStatType : StatType {

    public readonly bool persistent;
    public readonly StackMode stackMode;
    
    public StackableStatType(string name, string description, Texture2D icon, bool persistent, StackMode stackMode)
        : base(name, description, icon) {
        this.persistent = persistent;
        this.stackMode = stackMode;
    }
}

public enum StackMode {
    LARGEST, // keep the largest base value when combining stacks
    SUM // sum the base values when combining stacks
}

[ProtoContract]
public class StatTypeSurrogate {

    [ProtoMember(1)] public string name;
    
    public static implicit operator StatType(StatTypeSurrogate surrogate) {
        if (surrogate == null) return null;
        return StatType.Parse(surrogate.name);
    }
    
    public static implicit operator StatTypeSurrogate(StatType statType) {
        if (statType == null) return null;
        return new StatTypeSurrogate { name = statType.Name };
    }

}