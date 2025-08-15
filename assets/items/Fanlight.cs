using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.projectile;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class Fanlight : OnDamageEffect {

    private const double DAMAGE_INTERVAL = 3.0; // seconds
    private const float DAMAGE_PERCENTAGE = 0.3f;
    private static readonly PackedScene PROJECTILE_SCENE = ResourceLoader.Load<PackedScene>("res://assets/items/fanlight_projectile.tscn");
    
    private readonly Dictionary<UnitInstance, float> storedDamage = new Dictionary<UnitInstance, float>();
    private readonly Dictionary<UnitInstance, double> storedDelta = new Dictionary<UnitInstance, double>();

    protected override void Apply(Item item, UnitInstance unit) {
        base.Apply(item, unit);
        storedDamage.Add(unit, 0);
        storedDelta.Add(unit, 0);
    }

    protected override void OnHit(Item item, UnitInstance unit, DamageEvent damageEvent) {
        if (damageEvent.DamageInstance.DamageMedium == DamageInstance.Medium.ITEM) return;
        storedDamage[unit] += damageEvent.DamageInstance.FinalAmount;
    }

    protected override void Process(Item item, UnitInstance unit, double delta) {
        if (!ServerController.Instance.IsServer) return;
        
        double newDelta = storedDelta[unit] + delta;
        if (newDelta < DAMAGE_INTERVAL || !Combat.IsValid(unit.CurrentTarget)) {
            storedDelta[unit] = newDelta;
            return;
        }

        float damage = storedDamage[unit] * item.ScaleValue(DAMAGE_PERCENTAGE);
        if (damage > 0) {
            Projectile projectile = PROJECTILE_SCENE.Instantiate<Projectile>();
            projectile.Initialize(unit, unit.CurrentTarget, () => {
                UnitInstance target = projectile.Target.UnitInstance;
                if (target == null) return;
                target.TakeDamage(unit.CreateDamageInstance(target, DamageInstance.Medium.ITEM, damage, DamageType.PURE));
            }, () => unit.CurrentTarget);
            unit.CurrentCombat.SpawnProjectile(projectile, unit.Position);
        }
        storedDamage[unit] = 0;
        storedDelta[unit] = 0;
    }

    protected override void Remove(Item item, UnitInstance unit) {
        base.Remove(item, unit);
        storedDamage.Remove(unit);
        storedDelta.Remove(unit);
    }
}