using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Godot;
using Godot.Collections;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.menu;
using MPAutoChess.logic.util;
using Color = Godot.Color;

namespace MPAutoChess.logic.core.unit;

[GlobalClass, Tool]
public partial class UnitInstance : CharacterBody2D {
    
    private const float ATTACK_ANIMATION_DURATION = 0.1f; // duration of the attack animation relative to the attack cooldown
    private const string IDLE_ANIMATION_NAME = "idle";
    private const string WALK_ANIMATION_NAME = "walk";
    private const string ATTACK_ANIMATION_NAME = "attack";
    private const string CAST_ANIMATION_NAME = "cast";
    private const string DEATH_ANIMATION_NAME = "death";
    
    
    private static readonly PackedScene OVERHEAD_UI_SCENE = ResourceLoader.Load<PackedScene>("res://ui/UnitOverlayUI.tscn");
    
    [Export(PropertyHint.Range, "0.0,1.0,0.01")] public float AttackTriggeredAt { get; set; } = 1f; // 0 = start of the animation, 1 = end of the animation
    [Export(PropertyHint.Range, "0.0,1.0,0.01")] public float SpellTriggeredAt { get; set; } = 1f; // 0 = start of the animation, 1 = end of the animation
    
    public Unit Unit { get; set; }
    public Stats Stats { get; private set; } = new Stats();
    public float CurrentHealth { get; set; }
    public float CurrentMana { get; set; }
    
    public AnimatedSprite2D Sprite { get; set; }
    public Spell Spell { get; set; }
    
    public bool IsCombatInstance { get; set; }

    private Vector2 spritePosition;
    private Vector2 spriteScale;
    
    private UnitOverlayUI overlayUi;
    public Combat CurrentCombat { get; set; }
    public bool IsInTeamA { get; set; } // true if this unit is part of team A, false if part of team B
    public UnitInstance? CurrentTarget { get; private set; }
    public PathFinder.Path? CurrentPath { get; private set; }
    public double AttackCooldown { get; set; } = 0.0;

    public override void _Ready() {
        if (Engine.IsEditorHint()) return;

        Array<Node> children = GetChildren();
        Sprite = children.FirstOrDefault(child => child is AnimatedSprite2D) as AnimatedSprite2D;
        Spell = children.FirstOrDefault(child => child is Spell) as Spell;
        spritePosition = Sprite.Position;
        spriteScale = Sprite.Scale;
        Sprite.Play(IDLE_ANIMATION_NAME);

        if (Engine.IsEditorHint()) {
            // arbitrary value for editor preview
            Stats = new Stats();
            Stats.GetCalculation(StatType.MAX_HEALTH).BaseValue = 1000f;
            Stats.GetCalculation(StatType.MAX_MANA).BaseValue = 100f;
            CurrentHealth = 800f;
            CurrentMana = 50f;
        } else {
            Stats = IsCombatInstance ? Unit.Stats.Clone() : Unit.Stats;
            SetFieldsFromStats();
        }
    }

    public override void _EnterTree() {
        if (Engine.IsEditorHint() || ServerController.Instance.IsServer) return;
        overlayUi = OVERHEAD_UI_SCENE.Instantiate<UnitOverlayUI>();
        overlayUi.UnitInstance = this;
        WorldControls.Instance.AddControl(overlayUi, new WorldControls.PositioningInfo() {
            attachedTo = this,
            attachedToBounds = new Rect2(-0.5f, -0.5f, 1f, 1f),
            attachmentPoint = AttachmentPoint.BOTTOM_CENTER,
            xGrowthDirection = GrowthDirection.BOTH,
            yGrowthDirection = GrowthDirection.NEGATIVE,
            size =  new Vector2(1f, 1.3f)
        });
    }

    public override void _ExitTree() {
        if (Engine.IsEditorHint() || ServerController.Instance.IsServer) return;
        WorldControls.Instance.RemoveControl(overlayUi);
    }

    private void SetFieldsFromStats() {
        CurrentHealth = Stats.GetValue(StatType.MAX_HEALTH);
        CurrentMana = Stats.GetValue(StatType.STARTING_MANA);
        Scale = new Vector2(Stats.GetValue(StatType.WIDTH), Stats.GetValue(StatType.HEIGHT));
    }

    public override void _Process(double delta) {
        if (Engine.IsEditorHint()) return;
        
        if (!IsCombatInstance) {
            SetFieldsFromStats();
        }
    }

    public void SetTarget(UnitInstance target, PathFinder.Path? path = null) {
        CurrentTarget = target;
        CurrentPath = path;
    }
    
    public bool HasTarget() {
        return CurrentTarget != null && IsInstanceValid(CurrentTarget) && CurrentTarget.IsAlive();
    }

    public bool CanReachTarget() {
        if (!HasTarget()) return false;
        
        float squaredDistance = GlobalPosition.DistanceSquaredTo(CurrentTarget.GlobalPosition);
        float range = Stats.GetValue(StatType.RANGE);
        return squaredDistance <= range * range;
    }

    public bool IsAlive() {
        return CurrentHealth > 0;
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

    public Vector2 GetSize() {
        return new Vector2(Stats.GetValue(StatType.WIDTH), Stats.GetValue(StatType.HEIGHT));
    }

    public float GetTotalAttackSpeed() {
        return Stats.GetValue(StatType.ATTACK_SPEED) * Stats.GetValue(StatType.BONUS_ATTACK_SPEED);
    }

    public void ProcessCombat(double delta) {
        AttackCooldown -= delta;
        if (AttackCooldown > 0) return;
        
        if (CanReachTarget()) {
            TriggerAttack();
        } else if (CurrentPath != null) {
            Sprite.Play(WALK_ANIMATION_NAME);
            Vector2 newPosition = CurrentPath.Advance(Position, (float)(Stats.GetValue(StatType.MOVEMENT_SPEED) * delta));
            FaceTowards(newPosition);
            Position = newPosition;
        } else {
            Sprite.Play(IDLE_ANIMATION_NAME);
        }
    }

    private void FaceTowards(Vector2 target) {
        Sprite.FlipH = (target - Position).X < 0;
    }

    private void TriggerAttack() {
        if (CurrentTarget == null || !IsInstanceValid(CurrentTarget) || !CurrentTarget.IsAlive()) {
            return;
        }
        
        FaceTowards(CurrentTarget.Position);
        
        AttackCooldown = 1f / GetTotalAttackSpeed();
        double animationTime = AttackCooldown * ATTACK_ANIMATION_DURATION;
        Sprite.Play(ATTACK_ANIMATION_NAME, (float)(1f / animationTime));
    }
}