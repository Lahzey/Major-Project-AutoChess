using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class Forgehammer : ItemEffect {

    private const float RESISTANCES_MOD = 0.5f;
    private const string STAT_ID_PREFIX = "Forgehammer ";

    protected override void Apply(Item item, UnitInstance unit) { }

    protected override void Process(Item item, UnitInstance unit, double delta) {
		if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;

        float resistancesMod = item.ScaleValue(RESISTANCES_MOD);
        float armor = unit.CurrentTarget?.Stats.GetValue(StatType.ARMOR) ?? 0f;
        armor *= resistancesMod;
        float aegis = unit.CurrentTarget?.Stats.GetValue(StatType.AEGIS) ?? 0f;
        aegis *= resistancesMod;

        string statId = STAT_ID_PREFIX + instanceId;
        if (armor != (unit.Stats.GetCalculation(StatType.ARMOR).GetFlat(statId)?.Get() ?? 0)) {
            unit.Stats.GetCalculation(StatType.ARMOR).AddFlat(armor, statId);
        }
        if (aegis != (unit.Stats.GetCalculation(StatType.AEGIS).GetFlat(statId)?.Get() ?? 0)) {
            unit.Stats.GetCalculation(StatType.AEGIS).AddFlat(aegis, statId);
        }
        // TODO: this will cause infinite growth if two oppising units attacking each other both have a forgehammer, find a way to exclude forgehammer stats when getting target stats
	}

    protected override void Remove(Item item, UnitInstance unit) { }
}