using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.events;

public class UnitContainerUpdateEvent : Event {
    
    public UnitContainer Container { get; private set; }
    public Unit UpdatedUnit { get; private set; }
    public bool Removing { get; set; }
    
    public UnitContainerUpdateEvent(UnitContainer container, Unit updatedUnit, bool removing) {
        Container = container;
        UpdatedUnit = updatedUnit;
        Removing = removing;
    }
    
}