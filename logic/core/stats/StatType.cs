using System.Collections.Generic;
using Godot;
using ProtoBuf;

namespace MPAutoChess.logic.core.stats;

[ProtoContract(Surrogate = typeof(StatTypeSurrogate))]
public class StatType {

    private static readonly Dictionary<string, StatType> STAT_TYPES = new Dictionary<string, StatType>();
    
    public readonly string Name;
    public readonly string Description;
    public readonly Texture2D Icon;

    private StatType() { } // for ProtoBuf serialization

    public StatType(string name, string description, Texture2D icon) {
        Name = name;
        Description = description;
        Icon = icon;
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

    public static readonly StatType MAX_HEALTH = new StatType("Max Health", "Maximum health of the unit.", null);
    public static readonly StatType MAX_MANA = new StatType("Max Mana", "The amount of Mana required to cast the spell.", null);
    public static readonly StatType STARTING_MANA = new StatType("Starting Mana", "The amount of Mana the unit starts combat with.", null);
    public static readonly StatType ARMOR = new StatType("Armor", "Reduces incoming physical damage.", null);
    public static readonly StatType AEGIS = new StatType("Aegis", "Reduces incoming magical damage.", null);
    public static readonly StatType STRENGTH = new StatType("Strength", "Damage dealt by the unit's attacks. Some spell cast are also improved by this.", null);
    public static readonly StatType MAGIC = new StatType("Magic", "Improves the units spell casts.", null);
    public static readonly StatType ATTACK_SPEED = new StatType("Attack Speed", "How often the unit attacks each second.", null);
    public static readonly StatType RANGE = new StatType("Range", "The distance at which the unit can attack.", null);
    public static readonly StatType WIDTH = new StatType("Width", "The width of the unit in grid cells.", null);
    public static readonly StatType HEIGHT = new StatType("Height", "The height of the unit in grid cells.", null);

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