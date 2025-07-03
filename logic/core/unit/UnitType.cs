using Godot;

namespace MPAutoChess.logic.core.unit;

[GlobalClass]
public partial class UnitType : Resource {
    
    [Export] public string Name { get; set; }
    [Export] public int SlotsNeeded { get; private set; } = 1;
    [Export] public int Size { get; private set; } = 2;
    [Export] public int Cost { get; private set; } = 1;
    [Export] public UnitRarity Rarity { get; private set; } = UnitRarity.Special;
    [ExportCategory("Default Stats")]
    [Export] public float MaxHealth { get; set; } = 1000;
    [Export] public float MaxMana { get; set; } = 100;
    [Export] public float StartingMana { get; set; } = 100;
    [Export] public float Armor { get; set; } = 10;
    [Export] public float Aegis { get; set; } = 10;
    [Export] public float Strength { get; set; } = 50;
    [Export] public float AttackSpeed { get; set; } = 0.75f;
    [Export] public float Range { get; set; } = 1f;
    [ExportCategory("Assets")]
    [Export] public PackedScene UnitInstancePrefab { get; set; }
    [Export] public Texture2D Icon { get; set; }
    
}

public enum UnitRarity {
    Special = 0,
    Common = 1,
    Uncommon = 2,
    Rare = 3,
    Epic = 4,
    Legendary = 5
}