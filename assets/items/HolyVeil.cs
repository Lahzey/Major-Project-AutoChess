using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class HolyVeil : OnDamageEffect {

    private const float MAGIC_DAMAGE_MOD = 0.5f;
    
    public HolyVeil() : base(true) {}

    protected override bool FilterEvent(Item item, UnitInstance unit, DamageEvent damageEvent) {
        return damageEvent.DamageInstance.Target == unit && damageEvent.DamageInstance.Type == DamageType.MAGICAL;
    }

    protected override void OnHit(Item item, UnitInstance unit, DamageEvent damageEvent) {
        damageEvent.DamageInstance.MitigationMod *= item.ScaleValue(MAGIC_DAMAGE_MOD);
    }
}