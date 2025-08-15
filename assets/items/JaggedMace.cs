using System;
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
public partial class JaggedMace : OnDamageEffect {

    private const float MIN_VAMP = 0.1f;
    private const float VAMP_PER_HIT = 0.02f;
    private const int MAX_VAMP_AFTER = 15;
    private const string STAT_PREFIX = "Jagged Mace ";
    
    private Dictionary<UnitInstance, Dictionary<UnitInstance, int>> hitCounters = new Dictionary<UnitInstance, Dictionary<UnitInstance, int>>();

    protected override void Apply(Item item, UnitInstance unit) {
        base.Apply(item, unit);
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;
        
        hitCounters[unit] = new Dictionary<UnitInstance, int>();
    }

    protected override void OnHit(Item item, UnitInstance unit, DamageEvent damageEvent) {
        Dictionary<UnitInstance, int> hitCounter = hitCounters[unit];
        if (!hitCounter.ContainsKey(damageEvent.DamageInstance.Target)) {
            hitCounter.Add(damageEvent.DamageInstance.Target, 1);
        } else {
            hitCounter[damageEvent.DamageInstance.Target]++;
        }
    }

    protected override void Process(Item item, UnitInstance unit, double delta) {
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;
        
        string statId = STAT_PREFIX + instanceId;
        int hitCount = unit.CurrentTarget != null ? hitCounters[unit].GetValueOrDefault(unit.CurrentTarget) : 0;
        float newValue = Math.Clamp(hitCount, 0, MAX_VAMP_AFTER) * VAMP_PER_HIT + MIN_VAMP;
        newValue = item.ScaleValue(newValue);
        float existingValue = unit.Stats.GetCalculation(StatType.VAMPIRISM).GetFlat(statId)?.Get() ?? 0;
        if (existingValue != newValue) unit.Stats.GetCalculation(StatType.VAMPIRISM).AddFlat(newValue, statId);
    }

    protected override void Remove(Item item, UnitInstance unit) {
        base.Remove(item, unit);
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;
        
        hitCounters.Remove(unit);
    }
}