using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.placement;

public interface IUnitDropTarget {
    
    public Player GetPlayer();
    
    public Vector2 ConvertToPlacement(Vector2 position, Unit forUnit);
    
    public bool IsValidDrop(Unit unit, Vector2 placement, Unit? replacedUnit = null);
    
    public void OnUnitDrop(Unit unit, Vector2 placement);

}