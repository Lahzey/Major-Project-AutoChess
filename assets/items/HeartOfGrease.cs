using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class HeartOfGrease : ItemEffect {

    private const float MOVEMENT_SPEED_MOD = 0.5f;
    private const string STAT_ID = "Heart of Grease";

    protected override void Apply(Item item, UnitInstance unit) {
		unit.Stats.GetCalculation(StatType.MOVEMENT_SPEED).AddPostMult(MOVEMENT_SPEED_MOD, STAT_ID);
	}

    protected override void Process(Item item, UnitInstance unit, double delta) { }

    protected override void Remove(Item item, UnitInstance unit) {
        unit.Stats.GetCalculation(StatType.MOVEMENT_SPEED).RemovePostMult(STAT_ID);
	}
}