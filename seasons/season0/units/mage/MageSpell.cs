using System;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.projectile;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using UnitInstance = MPAutoChess.logic.core.unit.UnitInstance;

namespace MPAutoChess.seasons.season0.units.mage;

[GlobalClass, Tool]
public partial class MageSpell : Spell {
    
    [Export] public PackedScene ProjectileScene { get; set; } = GD.Load<PackedScene>("res://seasons/season0/units/mage/MageSpellProjectile.tscn");
    [Export] public PackedScene ImpactParticlesScene { get; set; } = GD.Load<PackedScene>("res://seasons/season0/units/mage/MageExplosion.tscn");
    
    private const float IMPACT_RADIUS = 3f;
    private const float IMPACT_RADIUS_SQUARED = IMPACT_RADIUS * IMPACT_RADIUS;
    private float[] BaseDamage { get; set; } = { 300, 500, 2000 };
    private float[] DamageScaling { get; set; } = { 5, 5, 10 };
    
    private float GetDamageAmount(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, BaseDamage) + caster.Stats.GetValue(StatType.MAGIC) * GetFromLevelArray(caster.Unit, DamageScaling);
    }

    public override float GetCastTime(UnitInstance caster) {
        return 1.5f;
    }

    public override bool RequiresTarget(UnitInstance caster) {
        return false;
    }

    public override UnitInstance GetTarget(UnitInstance caster) {
        return caster.Enemies.OrderByDescending(enemy =>
            caster.Enemies.Where(otherEnemy => otherEnemy != enemy).Sum(otherEnemy => otherEnemy.Position.DistanceSquaredTo(enemy.Position) <= IMPACT_RADIUS_SQUARED ? 1 : 0)
        ).FirstOrDefault();
    }

    public override void Cast(UnitInstance caster, UnitInstance? target) {
        Projectile projectile = ProjectileScene.Instantiate<Projectile>();
        projectile.Initialize(caster, target.Position, () => Explode(caster, projectile), null); // no recalculation needed, target is static and therefore always valid
        caster.CurrentCombat.SpawnProjectile(projectile, caster.Position);
    }

    private void Explode(UnitInstance caster, Projectile projectile) {
        Vector2 position = projectile.Position;
        float damage = GetDamageAmount(caster);
        foreach (UnitInstance enemy in caster.Enemies) {
            if (position.DistanceSquaredTo(enemy.Position) <= IMPACT_RADIUS_SQUARED) {
                DamageInstance damageInstance = caster.CreateDamageInstance(enemy, DamageInstance.Medium.SPELL, damage, DamageType.MAGICAL);
                enemy.TakeDamage(damageInstance);
            }
        }

        Rpc(MethodName.SpawnParticles, projectile.GlobalPosition);
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void SpawnParticles(Vector2 globalPosition) {
        Node2D impactParticles = ImpactParticlesScene.Instantiate<Node2D>();
        AddChild(impactParticles);
        impactParticles.GlobalPosition = globalPosition;
        impactParticles.ZIndex = 1000;
    }

    public override string GetDescription(UnitInstance forUnit) {
        return $"Throws a fireball at the largest group of enemies, dealing {GetDamageAmount(forUnit):0} magic damage to each enemy hit.";
    }
}