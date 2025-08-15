using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class Hexspike : OnDamageEffect {

    private const float DAMAGE_MOD = 1.3f;
    
    // for tracking which units have damaged this unit
    private Dictionary<UnitInstance, HashSet<UnitInstance>> damagedTrackers = new Dictionary<UnitInstance, HashSet<UnitInstance>>();
    
    // for increasing damage dealt by this unit
    private readonly Dictionary<UnitInstance, Action<DamageEvent>> beforeDamageListeners = new Dictionary<UnitInstance, Action<DamageEvent>>();
    
    protected override bool FilterEvent(Item item, UnitInstance unit, DamageEvent damageEvent) {
        return damageEvent.DamageInstance.Target == unit;
    }

    protected override void OnHit(Item item, UnitInstance unit, DamageEvent damageEvent) {
        damagedTrackers[unit].Add(damageEvent.DamageInstance.Source);
    }

    protected override void Apply(Item item, UnitInstance unit) {
        base.Apply(item, unit);
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;
        
        damagedTrackers[unit] = new HashSet<UnitInstance>();
        
        Action<DamageEvent> listener = damageEvent => {
            if (damageEvent.DamageInstance.Source == unit) {
                bool hasDamagedThis = damagedTrackers[unit].Contains(damageEvent.DamageInstance.Target);
                if (hasDamagedThis) damageEvent.DamageInstance.PreMitigationAmount *= item.ScaleValue(DAMAGE_MOD);
            }
        };
        beforeDamageListeners[unit] = listener;
        EventManager.INSTANCE.AddBeforeListener(listener);
	}

    protected override void Remove(Item item, UnitInstance unit) {
        base.Remove(item, unit);
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;
        
        damagedTrackers.Remove(unit);
        
        EventManager.INSTANCE.RemoveBeforeListener(beforeDamageListeners[unit]);
	}
}