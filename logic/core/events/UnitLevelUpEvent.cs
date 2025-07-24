using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.events;

public class UnitLevelUpEvent : Event {
    
    public Unit Unit { get; private set; }
    public Unit[] Copies { get; private set; }

    public UnitLevelUpEvent(Unit unit, params Unit[] copies) {
        Unit = unit;
        Copies = copies;
    }
    
}