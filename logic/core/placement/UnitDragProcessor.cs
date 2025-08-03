using System;
using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;
using MPAutoChess.logic.util;

namespace MPAutoChess.logic.core.placement;

public class UnitDragProcessor {
    
    public bool Running { get; private set; } = false;
    
    public UnitInstance UnitInstance { get; private set; }
    public Vector2 DragStartPosition { get; private set; }
    public Vector2 OriginalUnitPosition { get; private set; }
    public DateTime DragStartTime { get; private set; }
    
    public IUnitDropTarget? HoveredDropTarget { get; private set; }
    public Vector2 CurrentMousePosition { get; private set; }

    public void Start(UnitInstance unitInstance) {
        UnitInstance = unitInstance;
        DragStartPosition = unitInstance.GetViewport().GetCamera2D().GetGlobalMousePosition();;
        OriginalUnitPosition = unitInstance.GlobalPosition;
        DragStartTime = DateTime.Now;
        CurrentMousePosition = DragStartPosition;
        Running = true;
    }
    
    public void Update(Node2D nodeRef) {
        CurrentMousePosition = nodeRef.GetViewport().GetCamera2D().GetGlobalMousePosition();
        HoveredDropTarget = HoverChecker.GetHoveredNodeOrNull<IUnitDropTarget>(CollisionLayers.UNIT_DROP_TARGET, nodeRef);
        
        // Update unit position to follow the mouse, but hover above the ground
        UnitInstance.SetGlobal3DPostition(CurrentMousePosition.Extend(1f));
    }

    public void Complete(bool forceCancel = false) {
        UnitInstance.SetGlobal3DPostition(OriginalUnitPosition.Extend(0f));
        if (HoveredDropTarget != null && !forceCancel) Drop();
        else Cancel();
        
        UnitInstance = null;
        Running = false;
    }

    private void Cancel() {
        GD.Print("Drop cancelled.");
    }

    private void Drop() {
        if (HoveredDropTarget == null || !IsAllowed()) {
            GD.Print("Drop not allowed.");
            Cancel();
            return;
        }

        Vector2 targetPlacement = HoveredDropTarget.ConvertToPlacement(CurrentMousePosition, UnitInstance.Unit);
        PlayerController.Current.MoveUnit(UnitInstance.Unit, HoveredDropTarget, targetPlacement);
        UnitInstance = null;
    }

    private bool IsAllowed() {
        if (HoveredDropTarget == null) return false;

        Vector2 targetPlacement = HoveredDropTarget.ConvertToPlacement(CurrentMousePosition, UnitInstance.Unit);
        Unit? replacedUnit = HoveredDropTarget is UnitContainer targetContainer ? targetContainer.GetUnitAt(targetPlacement, UnitInstance.Unit.GetSize()) : null;
        if (!HoveredDropTarget.IsValidDrop(UnitInstance.Unit, targetPlacement, replacedUnit)) return false;
        if (replacedUnit != null && replacedUnit != UnitInstance.Unit) {
            if (!UnitInstance.Unit.Container.IsValidDrop(UnitInstance.Unit, UnitInstance.Unit.Container.GetPlacement(UnitInstance.Unit), UnitInstance.Unit)) return false;
        }
        return true;
    }
}