using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.events;

public class TargetChangeEvent : Event {
    
    public UnitInstance Source { get; private set; }
    public UnitInstance OldTarget { get; private set; }
    public UnitInstance NewTarget { get; private set; }
    
    public TargetChangeEvent(UnitInstance source, UnitInstance oldTarget, UnitInstance newTarget) {
        Source = source;
        OldTarget = oldTarget;
        NewTarget = newTarget;
    }
    
}