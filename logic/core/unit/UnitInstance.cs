using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Godot;
using Godot.Collections;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.util;
using Color = Godot.Color;

namespace MPAutoChess.logic.core.unit;

[GlobalClass, Tool]
public partial class UnitInstance : CharacterBody2D {
    
    [Export] public PackedScene ResourceBarScene { get; set; } = ResourceLoader.Load<PackedScene>("res://prefabs/ResourceBar.tscn");
    
    public Unit Unit { get; set; }
    public Stats Stats { get; private set; } = new Stats();
    public float CurrentHealth { get; set; }
    public float CurrentMana { get; set; }
    
    public AnimatedSprite2D Sprite { get; set; }
    public Spell Spell { get; set; }
    
    public bool IsCombatInstance { get; set; }

    private Vector2 spritePosition;
    private Vector2 spriteScale;

    public override void _Ready() {
        if (Engine.IsEditorHint()) return;
        
        Array<Node> children = GetChildren();
        Sprite = children.FirstOrDefault(child => child is AnimatedSprite2D) as AnimatedSprite2D;
        Spell = children.FirstOrDefault(child => child is Spell) as Spell;
        spritePosition = Sprite.Position;
        spriteScale = Sprite.Scale;
        Stats = IsCombatInstance ? Unit.Stats.Clone() : Unit.Stats;
        SetFieldsFromStats();
        CreateResourceBars();
    }

    private void SetFieldsFromStats() {
        CurrentHealth = Stats.GetValue(StatType.MAX_HEALTH);
        CurrentMana = Stats.GetValue(StatType.STARTING_MANA);
        Scale = new Vector2(Stats.GetValue(StatType.WIDTH), Stats.GetValue(StatType.HEIGHT));
    }

    private void CreateResourceBars() {
        Node2D barsContainer = new Node2D();
        barsContainer.Name = "ResourceBars";
        // Due to local scaling (units having a size of 1x1 - 3x3), we need to scale the bar down.
        // Having sizes in the single or sub pixel range would cause issues like fuzzy rendering or minimum sizes, this way we can still set normal sizes (in the 100s of pixels).
        barsContainer.Scale = VectorExtensions.Uniform2D(0.001f);
        
        ResourceBar healthBar = ResourceBarScene.Instantiate<ResourceBar>();
        healthBar.Connect(this, instance => instance.Stats.GetValue(StatType.MAX_HEALTH), instance => instance.CurrentHealth);
        healthBar.SetGaps(ResourceBar.HEALTH_SMALL_GAP, ResourceBar.HEALTH_LARGE_GAP);
        healthBar.SetFillColor(ResourceBar.HEALTH_COLOR);
        healthBar.Size = new Vector2(1000, 150);
        healthBar.Position = new Vector2(-500, -750);
        barsContainer.AddChild(healthBar);
        
        int maxMana = (int) Stats.GetValue(StatType.MAX_MANA);
        if (maxMana > 0) { // max mana of 0 indicates no active ability or one that does not cost mana
            ResourceBar manaBar = ResourceBarScene.Instantiate<ResourceBar>();
            manaBar.Connect(this, instance => instance.Stats.GetValue(StatType.MAX_MANA), instance => instance.CurrentMana);
            manaBar.SetGaps(ResourceBar.NO_GAP, ResourceBar.NO_GAP);
            manaBar.SetFillColor(ResourceBar.MANA_COLOR);
            manaBar.Size = new Vector2(1000, 50);
            manaBar.Position = new Vector2(-500, -590); // just below the health bar (with a gap of 10)
            barsContainer.AddChild(manaBar);
        }
        AddChild(barsContainer);
    }

    public override void _Process(double delta) {
        if (Engine.IsEditorHint()) return;
        
        if (!IsCombatInstance) {
            SetFieldsFromStats();
        }
    }

    public void Heal(float amount) {
        if (amount <= 0) return;
        CurrentHealth = Math.Min(CurrentHealth + amount, Stats.GetValue(StatType.MAX_HEALTH));
    }

    public override string[] _GetConfigurationWarnings() {
        GD.Print("_GetConfigurationWarnings");
        
        List<string> warnings = new List<string>();
        Array<Node> children = GetChildren();
        if (!children.Any(child => child is AnimatedSprite2D)) {
            warnings.Add("UnitInstance must have an AnimatedSprite2D as a child.");
        }
        if (children.Count(child => child is Spell) != 1) {
            warnings.Add("UnitInstance must have exactly one Spell as a child.");
        }
        
        return warnings.ToArray();
    }

    public void SetHightlight(bool highlight) {
        Modulate = highlight ? new Color(2f, 2f, 2f) : Colors.White;
    }

    public void SetGlobal3DPostition(Vector3 globalPosition) {
        GlobalPosition = globalPosition.Xy();
        // fake 3D effect: higher z means moving sprite up a bit and making it larger and brighter
        Sprite.Position = spritePosition + Vector2.Up * globalPosition.Z * 0.2f;
        Sprite.Scale = spriteScale * Mathf.Pow(Mathf.E, globalPosition.Z * 0.2f);
        float modulate = globalPosition.Z * 0.2f + 1f;
        Sprite.Modulate = new Color(modulate, modulate, modulate);
    }
}