using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class Blooddrinker : OnHitEffect {
    
    private const float HEAL_AMOUNT = 0.25f;

    protected override void OnHit(Item item, UnitInstance unit, DamageEvent damageEvent) {
        float healingAmount = damageEvent.DamageInstance.FinalAmount * item.ScaleValue(HEAL_AMOUNT);
        unit.Heal(unit.CreateDamageInstance(unit, DamageInstance.Medium.ITEM, healingAmount, DamageType.HEALING));
    }
}