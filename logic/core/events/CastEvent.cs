using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.events;

public class CastEvent : CancellableEvent {
    
    public UnitInstance Source { get; private set; }
    public UnitInstance Target { get; private set; }
    public float CastTime { get; set; }

    public CastEvent(UnitInstance source, UnitInstance target, float castTime) {
        Source = source;
        Target = target;
        CastTime = castTime;
    }
    
}