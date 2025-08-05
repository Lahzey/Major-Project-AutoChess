using MPAutoChess.logic.core.combat;

namespace MPAutoChess.logic.core.events;

public class DamageEvent : CancellableEvent {
    
    public DamageInstance DamageInstance { get; private set; }

    public DamageEvent(DamageInstance damageInstance) {
        DamageInstance = damageInstance;
    }
    
}