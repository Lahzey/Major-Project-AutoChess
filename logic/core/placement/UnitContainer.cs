using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.placement;

public interface UnitContainer : UnitDropTarget {
    
    public Player GetPlayer();
    
    public Unit? GetUnitAt(Vector2 position);
    
    public Vector2 GetPlacement(Unit unit);
    
    public bool CanFitAt(Unit unit, Vector2 position, Unit? replacedUnit = null);
    
    public Vector2 RemoveUnit(Unit unit);
    
    public void AddUnit(Unit unit, Vector2 position);
    
}