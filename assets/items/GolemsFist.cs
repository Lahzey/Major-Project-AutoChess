using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class GolemsFist : OnHitEffect {

    private const float ARMOR_VALUE = 5f;
    private const double DURATION = 3.0; // seconds
    private const string STAT_ID = "Golems Fist";

    private Dictionary<UnitInstance, List<double>> removeQueue = new Dictionary<UnitInstance, List<double>>();

    protected override void Apply(Item item, UnitInstance unit) {
        base.Apply(item, unit);
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;
        removeQueue[unit] = new List<double>();
    }

    protected override void OnHit(Item item, UnitInstance unit, DamageEvent damageEvent) {
        GD.PrintErr("Adding armor");
        unit.Stats.GetCalculation(StatType.ARMOR).AddFlat(item.ScaleValue(ARMOR_VALUE), STAT_ID, true);
        double removeDelay = DURATION;
        foreach (double existingDelay in removeQueue[unit]) {
            removeDelay -= existingDelay;
        }
        removeQueue[unit].Add(removeDelay);
    }

    protected override void Process(Item item, UnitInstance unit, double delta) {
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;
        
        List<double> queue = removeQueue[unit];
        if (queue.Count == 0) return;

        double nextRemove = queue[0];
        nextRemove -= delta;
        if (nextRemove > 0) {
            queue[0] = nextRemove;
        } else {
            queue.RemoveAt(0);
            float stat = unit.Stats.GetCalculation(StatType.ARMOR).GetFlat(STAT_ID)?.Get() ?? 0f;
            stat -= item.ScaleValue(ARMOR_VALUE);
            if (stat > 0) unit.Stats.GetCalculation(StatType.ARMOR).AddFlat(stat, STAT_ID);
            else unit.Stats.GetCalculation(StatType.ARMOR).RemoveFlat(STAT_ID);
        }
    }

    protected override void Remove(Item item, UnitInstance unit) {
        base.Remove(item, unit);
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;
        removeQueue[unit].Clear();
    }
}