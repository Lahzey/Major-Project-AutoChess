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
public partial class DraconicTalons : ItemEffect {
    
    private const float DAMAGE_AMOUNT = 250f;
    private const float RANGE_SQUARED = 6f * 6f;
    
    private Dictionary<UnitInstance, Action<CastEvent>> castListeners = new Dictionary<UnitInstance, Action<CastEvent>>();

    protected override void Apply(Item item, UnitInstance unit) {
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;

        Action<CastEvent> castListener = castEvent => {
            UnitInstance source = castEvent.Source;
            if (source.CurrentCombat != unit.CurrentCombat || source.IsInTeamA == unit.IsInTeamA) return;
            if (source.Position.DistanceSquaredTo(unit.Position) > RANGE_SQUARED) return;
            
            DamageInstance damageInstance = unit.CreateDamageInstance(source, DamageInstance.Medium.ITEM, item.ScaleValue(DAMAGE_AMOUNT), DamageType.MAGICAL);
            damageInstance.CritEnabled = true;
            source.TakeDamage(damageInstance);
        };
        castListeners[unit] = castListener;
        EventManager.INSTANCE.AddAfterListener(castListener);
    }

    protected override void Process(Item item, UnitInstance unit, double delta) { }


    protected override void Remove(Item item, UnitInstance unit) {
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;

        EventManager.INSTANCE.RemoveAfterListener(castListeners[unit]);
    }
}