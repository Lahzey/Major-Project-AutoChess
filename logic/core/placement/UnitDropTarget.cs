using Godot;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.placement;

public interface UnitDropTarget {
    
    public bool IsValidDrop(Unit unit, Vector2 pos);
    
    public void OnUnitDrop(Unit unit, Vector2 pos);

}