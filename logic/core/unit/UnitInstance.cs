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
using MPAutoChess.logic.core.projectile;
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
    
    private const float HALF_PI = Mathf.Pi * 0.5f;
    
    public const float ATTACK_ANIMATION_SPEED = 5f; // duration of the attack animation relative to the attack cooldown
    private const string IDLE_ANIMATION_NAME = "idle";
    private const string WALK_ANIMATION_NAME = "walk";
    private const string ATTACK_ANIMATION_NAME = "attack";
    private const string CAST_ANIMATION_NAME = "cast";
    private const string DEATH_ANIMATION_NAME = "death";

    private const int MANA_PER_ATTACK = 10;
    
    
    private static readonly PackedScene OVERHEAD_UI_SCENE = ResourceLoader.Load<PackedScene>("res://ui/UnitOverlayUI.tscn");
    
    [Export(PropertyHint.Range, "0.0,1.0,0.01")] public float AttackDamageTriggeredAt { get; set; } = 1f; // 0 = start of the animation, 1 = end of the animation
    [Export(PropertyHint.Range, "0.0,1.0,0.01")] public float SpellEffectTriggeredAt { get; set; } = 1f; // 0 = start of the animation, 1 = end of the animation
    
    [ProtoMember(1)] public Unit Unit { get; set; }
    [ProtoMember(2)] public CompoundStats Stats { get; private set; } = new CompoundStats(new Stats());
    [ProtoMember(3)] public float CurrentHealth { get; set; }
    [ProtoMember(4)] public float CurrentMana { get; set; }
    [ProtoMember(5)] public bool IsDead { get; private set; }
    
    [ProtoMember(6)] public bool IsCombatInstance { get; set; }
    [ProtoMember(7)] public bool IsInTeamA { get; set; } // true if this unit is part of team A, false if part of team B
    
    // because reference tracking does not really work with ProtoBuf, we need to figure out the combat instance on the first call on the client
    private Combat currentCombat;
    public Combat CurrentCombat {
        get {
            if (!IsCombatInstance || currentCombat != null) return currentCombat;
            foreach (Combat combat in GameSession.Instance.GetCurrentCombats()) {
                if (combat.GetAllUnits().Contains(this)) {
                    currentCombat = combat;
                }
            }
            return currentCombat;
        }
        set => currentCombat = value;
    }
    public IEnumerable<UnitInstance> Allies => CurrentCombat.GetTeamUnits(IsInTeamA);
    public IEnumerable<UnitInstance> Enemies => CurrentCombat.GetTeamUnits(!IsInTeamA);
    
    public AnimatedSprite2D Sprite { get; set; }
    public Spell Spell { get; set; }
    
    private UnitOverlayUI overlayUi;
    private Vector2 spritePosition;
    private Vector2 spriteScale;
    
    public UnitInstance? CurrentTarget { get; private set; }
    public PathFinder.Path? CurrentPath { get; private set; }
    public double AttackCooldown { get; set; } = 0.0;
    public double ManaGainCooldown { get; set; } = 0.0; // cooldown for mana gain (triggered every second), doubles as a mana lock for buff spells
    public bool IsBusy { get; set; } = false; // units are usually busy while they are performing their attack or cast
    
    private bool initialized = false;

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
            Stats.GetCalculation(StatType.MAX_HEALTH).BaseValue = 1000f;
            Stats.GetCalculation(StatType.MAX_MANA).BaseValue = 100f;
            CurrentHealth = 800f;
            CurrentMana = 50f;
        } else if (ServerController.Instance.IsServer) {
            Stats = new CompoundStats(Unit.Stats);
            SetFieldsFromStats();
            Stats.SetAutoSendChanges(true);
        } else {
            if (!IsCombatInstance) { // combat units are managed by the server
                if (Unit == null) throw new ArgumentException("Unit not set for UnitInstance " + Name);
                Stats = new CompoundStats(Unit.Stats);
                SetFieldsFromStats();
            }
        }
    }

    public override void _EnterTree() {
        if (Engine.IsEditorHint()) return;
        
        // cannot be set on creation because the collision layer is not automatically serialized
        CollisionLayers collisionLayer = IsCombatInstance ? CollisionLayers.COMBAT_UNIT_INSTANCE : CollisionLayers.PASSIVE_UNIT_INSTANCE;
        collisionLayer |= CollisionLayers.SELECTABLE;
        CollisionLayer = (uint)collisionLayer;
        
        if (ServerController.Instance.IsServer) return;
        
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

        if (!initialized) {
            foreach (Item item in Unit.EquippedItems) {
                item.Effect?.Apply(item, this);
            }
            initialized = true;
        }
        
        if (!IsCombatInstance) {
            SetFieldsFromStats();
        }
        
        foreach (Item item in Unit.EquippedItems) {
            item.Effect?.Process(item, this, delta);
        }

        Sprite.GlobalRotation = 0; // prevent the sprite from rotating when arenas are rotated based on persepctive
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
        return Combat.IsValid(CurrentTarget);
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
        ManaGainCooldown -= delta;
        
        if (ManaGainCooldown < 0) {
            CurrentMana += Stats.GetValue(StatType.MANA_REGEN);
            ManaGainCooldown += 1;
        }
        
        if (IsBusy) return;

        if (CanCastSpell()) {
            TriggerSpell();
        } else if (CanReachTarget()) {
            if (AttackCooldown <= 0) TriggerAttack();
        } else if (CurrentPath != null) {
            Sprite.Play(WALK_ANIMATION_NAME);
            Vector2 newPosition = CurrentPath.Advance(Position, (float)(Stats.GetValue(StatType.MOVEMENT_SPEED) * delta));
            FaceTowards(newPosition);
            Position = newPosition;
        } else {
            Sprite.Play(IDLE_ANIMATION_NAME);
        }
    }

    private bool CanCastSpell() {
        if (Spell is null or NoSpell) return false;
        if (Spell.RequiresTarget(this) && !Combat.IsValid(CurrentTarget)) return false;
        float maxMana = Stats.GetValue(StatType.MAX_MANA);
        return CurrentMana >= maxMana;
    }

    private void FaceTowards(Vector2 target) {
        bool isFacingLeft = (target - Position).X < 0;
        if (Sprite.Rotation > HALF_PI || Sprite.Rotation < -HALF_PI) isFacingLeft = !isFacingLeft; // adjust for the sprite rotation
        Sprite.FlipH = isFacingLeft;
    }

    private void TriggerAttack() {
        if (!ServerController.Instance.IsServer) return;
        Rpc(MethodName.Attack);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private async void Attack() {
        if (!Combat.IsValid(CurrentTarget)) {
            return;
        }
        
        AttackEvent attackEvent = new AttackEvent(this, CurrentTarget);
        EventManager.INSTANCE.NotifyBefore(attackEvent);
        if (attackEvent.IsCancelled) return;
        
        FaceTowards(attackEvent.Target.Position);
        IsBusy = true;
        CurrentMana += MANA_PER_ATTACK;
        
        float attackSpeed = GetTotalAttackSpeed();
        AttackCooldown = 1f / attackSpeed;
        float animationSpeed = attackSpeed * ATTACK_ANIMATION_SPEED;
        float animationDuration = 1f / animationSpeed;
        
        if (!ServerController.Instance.IsServer) {
            Sprite.Play(ATTACK_ANIMATION_NAME, animationSpeed);
            await ToSignal(Sprite, "animation_finished");
            IsBusy = false;
            return;
        }

        float damageTriggeredAt = AttackDamageTriggeredAt * animationDuration;
        
        await ToSignal(GetTree().CreateTimer(damageTriggeredAt), "timeout");
        
        if (!Combat.IsValid(attackEvent.Target)) attackEvent.Target = CurrentTarget; // if target dies during attack, we try to use the current target to prevent fizzles
        if (Combat.IsValid(attackEvent.Target)) {
            ExecuteAttack(attackEvent);
        } else {
            AttackCooldown = 0; // if target is dead and no other target is available we reset the cooldown to compensate (will remain busy until the end of the animation though)
        }
        
        await ToSignal(GetTree().CreateTimer(animationDuration - damageTriggeredAt), "timeout");
        IsBusy = false;
    }

    private void ExecuteAttack(AttackEvent attackEvent) {
        DamageInstance damageInstance = CreateDamageInstance(attackEvent.Target, DamageInstance.Medium.ATTACK, Stats.GetValue(StatType.STRENGTH), DamageType.PHYSICAL);
        if (Stats.GetValue(StatType.RANGE) > 0.01f) {
            Projectile projectile = Unit.Type.GetAttackProjectileOrDefault().Instantiate<Projectile>();
            projectile.Initialize(this, attackEvent.Target, () => {
                HitTarget(attackEvent, damageInstance);
            }, () => CurrentTarget);
            CurrentCombat.SpawnProjectile(projectile, Position);
        } else {
            HitTarget(attackEvent, damageInstance);
        }

        EventManager.INSTANCE.NotifyAfter(attackEvent);
    }
    
    public void TriggerSpell() {
        if (!ServerController.Instance.IsServer) return;
        Rpc(MethodName.CastSpell, CurrentMana - Stats.GetValue(StatType.MAX_MANA));
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private async void CastSpell(float newMana) { // mana is not periodically synchronized, so this call effectively does that by sending the new mana value instead of calculating it inside the function
        CastEvent castEvent = new CastEvent(this, Spell.GetTarget(this), Spell.GetCastTime(this));
        EventManager.INSTANCE.NotifyBefore(castEvent);
        if (castEvent.IsCancelled) return;

        CurrentMana = newMana;
        
        FaceTowards(castEvent.Target.Position);
        IsBusy = true;
        
        if (!ServerController.Instance.IsServer) {
            Sprite.Play(CAST_ANIMATION_NAME, 1f / castEvent.CastTime);
            await ToSignal(Sprite, "animation_finished");
            IsBusy = false;
            return;
        }
        
        float effectTriggeredAt = SpellEffectTriggeredAt * castEvent.CastTime;
        
        await ToSignal(GetTree().CreateTimer(effectTriggeredAt), "timeout");
        
        Spell.Cast(this, castEvent.Target);
        EventManager.INSTANCE.NotifyAfter(castEvent);
        
        await ToSignal(GetTree().CreateTimer(castEvent.CastTime - effectTriggeredAt), "timeout");
        IsBusy = false;
    }

    private void HitTarget(AttackEvent attackEvent, DamageInstance damageInstance) {
        attackEvent.Target.TakeDamage(damageInstance);
    }

    public void TakeDamage(DamageInstance damageInstance) {
        damageInstance.CalculatePreMitigation();
        damageInstance.SetResistances(Stats.GetValue(StatType.ARMOR), Stats.GetValue(StatType.AEGIS));
        DamageEvent damageEvent = new DamageEvent(damageInstance);
        EventManager.INSTANCE.NotifyBefore(damageEvent);
        if (damageEvent.IsCancelled) {
            damageInstance.Amount = 0;
            return;
        }
        damageInstance.CalculateFinalAmount();
        Rpc(MethodName.TakeDamage, damageInstance.FinalAmount);
        EventManager.INSTANCE.NotifyAfter(damageEvent);
    }

    public void Heal(DamageSource source, float amount) {
        HealEvent healEvent = new HealEvent(source, this, amount);
        EventManager.INSTANCE.NotifyBefore(healEvent);
        if (healEvent.IsCancelled) return;
        Rpc(MethodName.TakeDamage, healEvent.Amount * -1);
        EventManager.INSTANCE.NotifyAfter(healEvent);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void TakeDamage(float amount) {
        CurrentHealth -= amount;
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
        CollisionLayer = 0;
        CollisionMask = 0;
        if (ServerController.Instance.IsServer) {
            Visible = false;
            CurrentCombat.OnUnitDeath(this);
        } else {
            Sprite.Play(DEATH_ANIMATION_NAME);
            await ToSignal(Sprite, "animation_finished");
            Visible = false;
            CurrentCombat.OnUnitDeath(this);
            GD.Print("Unit died on client");
        }
    }

    public int PerformCritRoll() {
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

    public uint GetLevel() {
        return Unit.Level;
    }

    public DamageInstance CreateDamageInstance(UnitInstance target, DamageInstance.Medium medium, float damage, DamageType damageType) {
        return new DamageInstance(this, target, medium, damage, damageType, PerformCritRoll(), Stats.GetValue(StatType.CRIT_DAMAGE));
    }
}