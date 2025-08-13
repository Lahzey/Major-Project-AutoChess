using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.events;

public class HealEvent : CancellableEvent {
    
    public DamageSource Source { get; private set; }
    public UnitInstance Target { get; private set; }
    public float Amount { get; set; }
    
    public HealEvent(DamageSource source, UnitInstance target, float amount) {
        Source = source;
        Target = target;
        Amount = amount;
    }
    
}