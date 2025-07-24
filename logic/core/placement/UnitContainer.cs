using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.placement;

[ProtoContract]
[ProtoInclude(100, typeof(SingleUnitSlot))]
[ProtoInclude(101, typeof(Board))]
public abstract partial class UnitContainer : Area2D, IUnitDropTarget {

    public abstract Vector2 ConvertToPlacement(Vector2 position, Unit forUnit);
    
    public abstract bool IsValidDrop(Unit unit, Vector2 placement, Unit replacedUnit = null);
    
    public abstract Player GetPlayer();
    
    public abstract Unit? GetUnitAt(Vector2 placement, Vector2 size = default);
    
    public abstract Vector2 GetPlacement(Unit unit);
    
    public abstract void RemoveUnit(Unit unit);
    
    public abstract void AddUnit(Unit unit, Vector2 placement);
    
    public abstract IEnumerable<Unit> GetUnits();

    public void OnUnitDrop(Unit unit, Vector2 placement) {
        Unit? replacedUnit = GetUnitAt(placement, unit.GetSize());
        if (!IsValidDrop(unit, placement, replacedUnit)) return;

        if (replacedUnit != null && replacedUnit != unit) {
            UnitContainer currentContainer = unit.Container;
            if (currentContainer == null) return; // cannot swap replaced unit if current unit is not in a container (never happens currently, but might in the future)
        
            Vector2 currentPlacement = currentContainer.GetPlacement(unit);
            if (!currentContainer.IsValidDrop(replacedUnit, placement, unit)) return; // replaced unit does not fit into current container
        
            currentContainer.RemoveUnit(unit);
            RemoveUnit(replacedUnit);
            currentContainer.AddUnit(replacedUnit, currentPlacement);
            AddUnit(unit, placement);
        } else {
            unit.Container?.RemoveUnit(unit);
            AddUnit(unit, placement);
        }
    }
}