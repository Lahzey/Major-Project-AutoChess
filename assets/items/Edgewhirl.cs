using Godot;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class Edgewhirl : OnHitEffect {
    
    private const float STRENGTH_AMOUNT = 2f;
    private const string STAT_ID = "Edgewhirl";
    
    protected override void OnHit(Item item, UnitInstance unit, DamageEvent damageEvent) {
        unit.Stats.GetCalculation(StatType.STRENGTH).AddFlat(item.ScaleValue(STRENGTH_AMOUNT), STAT_ID, true);
    }
}