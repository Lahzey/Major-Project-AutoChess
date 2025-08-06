using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Godot;
using Godot.Collections;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.menu;
using MPAutoChess.logic.util;
using ProtoBuf;
using Color = Godot.Color;

namespace MPAutoChess.logic.core.unit;

[GlobalClass, Tool]
[ProtoContract]
public partial class UnitInstance : CharacterBody2D {
    
    private const float ATTACK_ANIMATION_SPEED = 5f; // duration of the attack animation relative to the attack cooldown
    private const string IDLE_ANIMATION_NAME = "idle";
    private const string WALK_ANIMATION_NAME = "walk";
    private const string ATTACK_ANIMATION_NAME = "attack";
    private const string CAST_ANIMATION_NAME = "cast";
    private const string DEATH_ANIMATION_NAME = "death";
    
    
    private static readonly PackedScene OVERHEAD_UI_SCENE = ResourceLoader.Load<PackedScene>("res://ui/UnitOverlayUI.tscn");
    
    [Export(PropertyHint.Range, "0.0,1.0,0.01")] public float AttackTriggeredAt { get; set; } = 1f; // 0 = start of the animation, 1 = end of the animation
    [Export(PropertyHint.Range, "0.0,1.0,0.01")] public float SpellTriggeredAt { get; set; } = 1f; // 0 = start of the animation, 1 = end of the animation
    
    [ProtoMember(1)] public Unit Unit { get; set; }
    [ProtoMember(2)] public Stats Stats { get; private set; } = new Stats();
    [ProtoMember(3)] public float CurrentHealth { get; set; }
    [ProtoMember(4)] public float CurrentMana { get; set; }
    [ProtoMember(5)] public bool IsDead { get; private set; }
    
    [ProtoMember(6)] public bool IsCombatInstance { get; set; }
    
    public AnimatedSprite2D Sprite { get; set; }
    public Spell Spell { get; set; }
    
    private UnitOverlayUI overlayUi;
    private Vector2 spritePosition;
    private Vector2 spriteScale;
    
    public Combat CurrentCombat { get; set; }
    public bool IsInTeamA { get; set; } // true if this unit is part of team A, false if part of team B
    public UnitInstance? CurrentTarget { get; private set; }
    public PathFinder.Path? CurrentPath { get; private set; }
    public double AttackCooldown { get; set; } = 0.0;
    public bool IsBusy { get; set; } = false; // units are usually busy while they are performing their attack or cast

    public override void _Ready() {
        if (Engine.IsEditorHint()) return;


        ZIndex = 1;
        Array<Node> children = GetChildren();
        Sprite = children.FirstOrDefault(child => child is AnimatedSprite2D) as AnimatedSprite2D;
        Spell = children.FirstOrDefault(child => child is Spell) as Spell;
        spritePosition = Sprite.Position;
        spriteScale = Sprite.Scale;
        Sprite.Play(IDLE_ANIMATION_NAME);
        Sprite.AnimationFinished += PlayIdleAnimation;

        if (Engine.IsEditorHint()) {
            // arbitrary value for editor preview
            Stats = new Stats();
            Stats.GetCalculation(StatType.MAX_HEALTH).BaseValue = 1000f;
            Stats.GetCalculation(StatType.MAX_MANA).BaseValue = 100f;
            CurrentHealth = 800f;
            CurrentMana = 50f;
        } else if (ServerController.Instance.IsServer) {
            Stats = IsCombatInstance ? Unit.Stats.Clone() : Unit.Stats;
            SetFieldsFromStats();
            Stats.SetAutoSendChanges(true);
        } else {
            if (!IsCombatInstance) { // combat units are managed by the server
                if (Unit == null) throw new ArgumentException("Unit not set for UnitInstance " + Name);
                Stats = Unit.Stats;
                SetFieldsFromStats();
            }
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
        overlayUi = null;
    }

    private void PlayIdleAnimation() {
        Sprite.Play(IDLE_ANIMATION_NAME);
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
        if (!ServerController.Instance.IsServer) return; // ignored if called on the client
        CurrentTarget = target;
        CurrentPath = path;
        Rpc(MethodName.TransferTarget, target?.GetPath(), path != null ? SerializerExtensions.Serialize(path) : null);
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void TransferTarget(NodePath targetPath, byte[] serializedPath) {   
        PathFinder.Path path = serializedPath != null && serializedPath.Length > 0 ? SerializerExtensions.Deserialize<PathFinder.Path>(serializedPath) : null;
        Node target = targetPath != null && !targetPath.IsEmpty ? GetNodeOrNull(targetPath) : null;
        CurrentTarget = target as UnitInstance;
        CurrentPath = path;
    }
    
    public bool HasTarget() {
        return CurrentTarget != null && IsInstanceValid(CurrentTarget) && CurrentTarget.IsAlive();
    }

    public bool CanReachTarget() {
        if (!HasTarget()) return false;
        
        float squaredDistance = GlobalPosition.DistanceSquaredTo(CurrentTarget.GlobalPosition);
        float range = Stats.GetValue(StatType.RANGE) + GetSize().Largest()*0.5f + CurrentTarget.GetSize().Largest()*0.5f + Combat.NAVIGATION_GRID_SCALE*0.5f; // NAVIGATION_GRID_SCALE to prevent issues with rounding to grid cells
        return squaredDistance <= range * range;
    }

    public bool IsAlive() {
        return !IsDead;
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
        GlobalPosition = globalPosition.XY();
        // fake 3D effect: higher z means moving sprite up a bit and making it larger and brighter
        Sprite.Position = spritePosition + Vector2.Up * globalPosition.Z * 0.2f;
        Sprite.Scale = spriteScale * Mathf.Pow(Mathf.E, globalPosition.Z * 0.2f);
        float modulate = globalPosition.Z * 0.2f + 1f;
        Sprite.Modulate = new Color(modulate, modulate, modulate);
        ZIndex = 1 + (int) globalPosition.Z;
    }

    public Vector2 GetSize() {
        return new Vector2(Stats.GetValue(StatType.WIDTH), Stats.GetValue(StatType.HEIGHT));
    }

    public float GetTotalAttackSpeed() {
        return Stats.GetValue(StatType.ATTACK_SPEED) * (1 + Stats.GetValue(StatType.BONUS_ATTACK_SPEED));
    }

    public void ProcessCombat(double delta) {
        AttackCooldown -= delta;
        if (AttackCooldown > 0) return;
        if (IsBusy) return;
        
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

    private async void TriggerAttack() {
        if (CurrentTarget == null || !IsInstanceValid(CurrentTarget) || !CurrentTarget.IsAlive()) {
            return;
        }
        
        AttackEvent attackEvent = new AttackEvent(this, CurrentTarget);
        EventManager.INSTANCE.NotifyBefore(attackEvent);
        if (attackEvent.IsCancelled) return;
        
        UnitInstance target = attackEvent.Target;
        FaceTowards(target.Position);
        IsBusy = true;
        
        float attackSpeed = GetTotalAttackSpeed();
        AttackCooldown = 1f / attackSpeed;
        float animationSpeed = attackSpeed * ATTACK_ANIMATION_SPEED;
        float animationDuration = 1f / animationSpeed;
        
        EventManager.INSTANCE.NotifyAfter(attackEvent);
        
        if (!ServerController.Instance.IsServer) {
            Sprite.Play(ATTACK_ANIMATION_NAME, animationSpeed);
            await ToSignal(GetTree().CreateTimer(animationDuration), "timeout");
            IsBusy = false;
            return;
        }


        float attackTriggeredAt = AttackTriggeredAt * animationDuration;
        
        await ToSignal(GetTree().CreateTimer(attackTriggeredAt), "timeout");
        if (target != null && IsInstanceValid(target) && target.IsAlive()) {
            target.Damage(new DamageInstance(this, target, Stats.GetValue(StatType.STRENGTH), DamageType.PHYSICAL, PerformCritRoll(), Stats.GetValue(StatType.CRIT_DAMAGE), true));
        }
        await ToSignal(GetTree().CreateTimer(animationDuration - attackTriggeredAt), "timeout");
        IsBusy = false;
    }

    public void Damage(DamageInstance damageInstance) {
        damageInstance.CalculatePreMitigation();
        damageInstance.SetResistances(Stats.GetValue(StatType.ARMOR), Stats.GetValue(StatType.AEGIS));
        DamageEvent damageEvent = new DamageEvent(damageInstance);
        EventManager.INSTANCE.NotifyBefore(damageEvent);
        if (damageEvent.IsCancelled) return;
        damageInstance.CalculateFinalAmount();
        Rpc(MethodName.TakeDamage, damageInstance.Amount);
        EventManager.INSTANCE.NotifyAfter(damageEvent);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void TakeDamage(float amount) {
        CurrentHealth = Math.Max(0, CurrentHealth - amount);
        if (ServerController.Instance.IsServer) {
            if (CurrentHealth <= 0) Rpc(MethodName.Kill);
        } else {
            // TODO: show damage numbers
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public async void Kill() {
        CurrentHealth = 0;
        IsDead = true;
        if (ServerController.Instance.IsServer) {
            GetParent().RemoveChild(this);
        } else {
            Sprite.Play(DEATH_ANIMATION_NAME);
            await ToSignal(Sprite, "animation_finished");
            GetParent().RemoveChild(this);
            GD.Print("Unit died on client");
        }
    }

    private int PerformCritRoll() {
        CritRollEvent critRollEvent = new CritRollEvent(this, Stats.GetValue(StatType.CRIT_CHANCE), false);
        EventManager.INSTANCE.NotifyBefore(critRollEvent);
        if (critRollEvent.IsCancelled) return 0;

        // overcrit handling
        if (critRollEvent.CanOverCrit && critRollEvent.CritChance > 1f) {
            critRollEvent.CritLevel += (int) critRollEvent.CritChance;
            critRollEvent.CritChance %= 1f; // remaining chance after over crits
        }

        // actual crit roll
        if (GameSession.Instance.Random.NextSingle() < critRollEvent.CritChance) critRollEvent.CritLevel++;
        
        EventManager.INSTANCE.NotifyAfter(critRollEvent);
        return critRollEvent.CritLevel;
    }
}