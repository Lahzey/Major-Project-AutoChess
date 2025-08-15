using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.events;

public class HealEvent : CancellableEvent {

    public DamageInstance HealingInstance { get; private set; }

    public HealEvent(DamageInstance healingInstance) {
        HealingInstance = healingInstance;
    }
    
}