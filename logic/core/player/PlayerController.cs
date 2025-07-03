using System;
using Godot;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.unit;
using UnitInstance = MPAutoChess.logic.core.unit.UnitInstance;

namespace MPAutoChess.logic.core.player;

public partial class PlayerController : Node2D {
    
    [Export] public Player CurrentPlayer { get; private set; }

    public event Action<Unit> OnDragStart;
    public event Action<Unit, UnitDropTarget?, Vector2> OnDragProcess;
    public event Action<Unit> OnDragEnd;
    
    private UnitInstance? unitInstanceUnderMouse;
    private UnitDropTarget? unitDropTargetUnderMouse;
    private Vector2 unitDropPositionUnderMouse;
    
    public UnitInstance CurrentlyDraggedUnit { get; private set; }
    private Vector2 dragStartPosition;
    private Vector2 distToDragStartPosition;
    private DateTime dragStartTime;
    
    public static PlayerController Instance { get; private set; }

    public override void _EnterTree() {
        if (Instance != null) {
            GD.PrintErr("PlayerController instance already exists, this should not happen!");
            return;
        }
        Instance = this;
    }

    public override void _Input(InputEvent @event) {
        if (@event is InputEventMouseButton mouseButtonEvent) {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left) {
                OnLeftClick(mouseButtonEvent);
            }
        } else if (@event is InputEventMouseMotion mouseMotionEvent) {
            if (CurrentlyDraggedUnit != null) {
                Vector2 mousePosition = GetViewport().GetCamera2D().GetGlobalMousePosition();
                CurrentlyDraggedUnit.GlobalPosition = mousePosition;
            }
        }
    }
    
    private void OnLeftClick(InputEventMouseButton mouseButtonEvent) {
        Vector2 mousePosition = GetViewport().GetCamera2D().GetGlobalMousePosition();
        if (mouseButtonEvent.Pressed) {
            if (CurrentlyDraggedUnit == null) TryStartDrag(mousePosition);
            else TryDrop();
        } else if(CurrentlyDraggedUnit != null) {
            // Mouse up within 100 ms of mouse down: click to pickup -> click to drop (select and place)
            // Mouse up after 100 ms of mouse down:  mouse down to pickup -> mouse up to drop (drag to destination)
            if ((dragStartTime - DateTime.Now).TotalMilliseconds > 100) {
                TryDrop();
            }
        }
    }

    private void TryStartDrag(Vector2 mousePosition) {
        if (unitInstanceUnderMouse == null) return;
        
        dragStartPosition = mousePosition;
        distToDragStartPosition = unitInstanceUnderMouse.GlobalPosition - dragStartPosition;
        dragStartTime = DateTime.Now;
        CurrentlyDraggedUnit = unitInstanceUnderMouse;
        OnDragStart?.Invoke(unitInstanceUnderMouse.Unit);
    }

    private void TryDrop() {
        if (CurrentlyDraggedUnit == null) return;
        
        // if dropped outside a drop target, cancel the drag
        if (unitDropTargetUnderMouse == null) {
            GD.Print("Dropped outside a valid drop target, cancelling drag.");
            CancelDrag();
            return;
        }
        
        if (!unitDropTargetUnderMouse.IsValidDrop(CurrentlyDraggedUnit.Unit, unitDropPositionUnderMouse)) {
            GD.PrintErr("Invalid drop position for unit: " + CurrentlyDraggedUnit.Unit.Type.Name);
            CancelDrag();
            return;
        }

        unitDropTargetUnderMouse.OnUnitDrop(CurrentlyDraggedUnit.Unit, unitDropPositionUnderMouse);
        OnDragEnd?.Invoke(CurrentlyDraggedUnit.Unit);
        CurrentlyDraggedUnit = null;
    }

    private void CancelDrag() {
        CurrentlyDraggedUnit.GlobalPosition = dragStartPosition + distToDragStartPosition;
        OnDragEnd?.Invoke(CurrentlyDraggedUnit.Unit);
        CurrentlyDraggedUnit = null;
    }

    public override void _Process(double delta) {
        CheckUnderMouse();
        if (CurrentlyDraggedUnit != null) OnDragProcess?.Invoke(CurrentlyDraggedUnit.Unit, unitDropTargetUnderMouse, unitDropPositionUnderMouse);
    }

    private void CheckUnderMouse() {
        Vector2 mousePosition = GetViewport().GetCamera2D().GetGlobalMousePosition();
        PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;
        PhysicsPointQueryParameters2D queryParameters = new PhysicsPointQueryParameters2D {
            Position = mousePosition,
            CollideWithAreas = true,
            CollisionMask = (uint) (CollisionLayers.PassiveUnitInstance | CollisionLayers.UnitDropTarget)
        };

        unitInstanceUnderMouse?.SetHightlight(false);
        unitDropTargetUnderMouse = null;
        unitInstanceUnderMouse = null;
        unitDropPositionUnderMouse = mousePosition; // only not relative if no drop target is under mouse
        
        foreach (IntersectionHit2D intersectionHit in spaceState.IntersectPointTyped(queryParameters)) {
            if (intersectionHit.Collider.Obj is UnitDropTarget dropTarget) {
                unitDropTargetUnderMouse = dropTarget;
                unitDropPositionUnderMouse = intersectionHit.Collider.As<Node2D>().ToLocal(mousePosition);
            } else if (intersectionHit.Collider.Obj is UnitInstance unitInstance) {
                unitInstanceUnderMouse = unitInstance;
                unitInstance.SetHightlight(true);
            }
        }
    }

    public void RerollShop() {
        CurrentPlayer.Shop.Reroll();
    }
}