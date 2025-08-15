using System;
using System.Collections.Generic;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class BulwarkShell : ItemEffect {
    
    private const float REFLECT_AMOUNT = 0.15f;
    
    private Dictionary<UnitInstance, Action<DamageEvent>> damageListeners = new Dictionary<UnitInstance, Action<DamageEvent>>();

    protected override void Apply(Item item, UnitInstance unit) {
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;
        
        Action<DamageEvent> listener = damageEvent => {
            if (damageEvent.DamageInstance.Target != unit) return;
            UnitInstance source = damageEvent.DamageInstance.Source as UnitInstance;
            if (source == null) return;
            float reflectedDamage = damageEvent.DamageInstance.FinalAmount * item.ScaleValue(REFLECT_AMOUNT);
            source.TakeDamage(unit.CreateDamageInstance(source, DamageInstance.Medium.ITEM, reflectedDamage, damageEvent.DamageInstance.Type));
        };
        damageListeners[unit] = listener;
        EventManager.INSTANCE.AddAfterListener(listener);
    }

    protected override void Process(Item item, UnitInstance unit, double delta) { }


    protected override void Remove(Item item, UnitInstance unit) {
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;

        EventManager.INSTANCE.RemoveAfterListener(damageListeners[unit]);
    }
}