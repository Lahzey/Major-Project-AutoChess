using Godot;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class Juggerblade : OnHitEffect {

    private const float DAMAGE_PER_HEALTH = 0.02f;
    
    public Juggerblade() : base(true) {}
    
    protected override void OnHit(Item item, UnitInstance unit, DamageEvent damageEvent) {
        float damage = unit.Stats.GetValue(StatType.MAX_HEALTH) * DAMAGE_PER_HEALTH;
        damageEvent.DamageInstance.PreMitigationAmount += item.ScaleValue(damage);
    }
}