using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.item;

[ProtoContract]
public abstract partial class ItemEffect : GodotObject, IIdentifiable {
    public string Id { get; set; }
    
    private int instanceCounter = 0; // used for a more readable id in the stats
    [ProtoMember(1)] protected int instanceId;

    public ItemEffect() {
        if (ServerController.Instance.IsServer) {
            instanceId = instanceCounter++;
        }
    }
    
    public void TryApply(Item item, UnitInstance unit) {
        try {
            Apply(item, unit);
        } catch (Exception e) {
            GD.PrintErr($"Error applying item effect {GetType()} to unit {unit?.Name}\n{e.GetType()}: {e.Message}\n{e.StackTrace}");
        }
    }
    
    public void TryProcess(Item item, UnitInstance unit, double delta) {
        try {
            Process(item, unit, delta);
        } catch (Exception e) {
            GD.PrintErr($"Error processing item effect {GetType()} for unit {unit?.Name}\n{e.GetType()}: {e.Message}\n{e.StackTrace}");
        }
    }
    
    public void TryRemove(Item item, UnitInstance unit) {
        try {
            Remove(item, unit);
        } catch (Exception e) {
            GD.PrintErr($"Error removing item effect {GetType()} from unit {unit?.Name}\n{e.GetType()}: {e.Message}\n{e.StackTrace}");
        }
    }

    protected abstract void Apply(Item item, UnitInstance unit);

    protected abstract void Process(Item item, UnitInstance unit, double delta);

    protected abstract void Remove(Item item, UnitInstance unit);

}

public abstract partial class OnDamageEffect : ItemEffect {

    private bool before;
    private int priority;

    public OnDamageEffect(bool before = false, int priority = 0) {
        this.before = before;
        this.priority = priority;
    }
    
    private readonly Dictionary<UnitInstance, Action<DamageEvent>> damageListeners = new Dictionary<UnitInstance, Action<DamageEvent>>();
    
    protected abstract void OnHit(Item item, UnitInstance unit, DamageEvent damageEvent);

    protected virtual bool FilterEvent(Item item, UnitInstance unit, DamageEvent damageEvent) {
        return damageEvent.DamageInstance.Source == unit;
    }

    protected override void Apply(Item item, UnitInstance unit) {
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;
        
        Action<DamageEvent> listener = damageEvent => {
            if (!FilterEvent(item, unit, damageEvent)) return;
            OnHit(item, unit, damageEvent);
        };
        damageListeners[unit] = listener;
        
        if (before) EventManager.INSTANCE.AddBeforeListener(listener, priority);
        else EventManager.INSTANCE.AddAfterListener(listener, priority);
    }

    protected override void Process(Item item, UnitInstance unit, double delta) { }


    protected override void Remove(Item item, UnitInstance unit) {
        if (!ServerController.Instance.IsServer) return;
        if (!unit.IsCombatInstance) return;
        
        if (before) EventManager.INSTANCE.RemoveBeforeListener(damageListeners[unit]);
        else EventManager.INSTANCE.RemoveAfterListener(damageListeners[unit]);
    }
}

public abstract partial class OnHitEffect : OnDamageEffect {
    
    public OnHitEffect(bool before = false, int priority = 0) : base(before, priority) { }
    
    protected override bool FilterEvent(Item item, UnitInstance unit, DamageEvent damageEvent) {
        return damageEvent.DamageInstance.Source == unit && damageEvent.DamageInstance.DamageMedium == DamageInstance.Medium.ATTACK;
    }
}