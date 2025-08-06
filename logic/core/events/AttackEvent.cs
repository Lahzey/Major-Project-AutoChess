using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.events;

public class AttackEvent : CancellableEvent {
    
    public UnitInstance Source { get; }
    public UnitInstance Target { get; set; }
    
    public AttackEvent(UnitInstance source, UnitInstance target) {
        Source = source;
        Target = target;
    }
    
}