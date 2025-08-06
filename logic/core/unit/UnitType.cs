using Godot;
using ProtoBuf;

namespace MPAutoChess.logic.core.unit;

[GlobalClass]
// resources do not need ProtoContract attribute, they are added all registered automatically (including Godot's built-in resources)
public partial class UnitType : Resource {
    
    // DO NOT CHANGE SETTERS TO PRIVATE! Godot silently fails in that case and leaves all the properties to their types default values.
    [Export] public string Name { get; set; }
    [Export] public int SlotsNeeded { get; set; } = 1;
    [Export] public Vector2 Size { get; set; } = new Vector2(2f, 2f);
    [Export] public int Cost { get; set; } = 1;
    [Export] public UnitRarity Rarity { get; set; } = UnitRarity.SPECIAL;
    [Export] public UnitRoleSet Roles { get; set; }
    [ExportCategory("Default Stats")]
    [Export] public float MaxHealth { get; set; } = 1000;
    [Export] public float MaxMana { get; set; } = 100;
    [Export] public float StartingMana { get; set; } = 100;
    [Export] public float Armor { get; set; } = 10;
    [Export] public float Aegis { get; set; } = 10;
    [Export] public float Strength { get; set; } = 50;
    [Export] public float AttackSpeed { get; set; } = 0.75f;
    [Export] public float Range { get; set; } = 0f;
    [Export] public float MovementSpeed { get; set; } = 2f;
    [ExportCategory("Assets")]
    [Export] public PackedScene UnitInstancePrefab { get; set; }
    [Export] public Texture2D Icon { get; set; }
    
}

public enum UnitRarity {
    SPECIAL = 0,
    COMMON = 1,
    Uncommon = 2,
    RARE = 3,
    EPIC = 4,
    Legendary = 5
}