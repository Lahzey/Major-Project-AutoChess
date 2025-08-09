using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.util;

namespace MPAutoChess.logic.menu;

public partial class SelectionLayer : Control {
    
    public static SelectionLayer Instance { get; private set; }
    
    private Func<Node, bool> selectionFilter;
    private Action<Node> onSelection;
    private Texture2D validSelectionCursor;
    private Texture2D invalidSelectionCursor;
    
    public override void _EnterTree() {
        Instance = this;
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void _ExitTree() {
        if (Instance == this) {
            Instance = null;
        }
    }

    public override void _GuiInput(InputEvent @event) {
        if (!Visible) return;

        if (@event is InputEventMouseMotion mouseMotionEvent) {
            Node hoveredSelectable = GetHoveredSelectable();
            GD.Print($"Hovered Selectable: {hoveredSelectable?.Name ?? "None"}");
            Input.SetCustomMouseCursor(hoveredSelectable != null ? validSelectionCursor : invalidSelectionCursor, Input.CursorShape.Arrow, new Vector2(16, 16));
        } else if (@event is InputEventMouseButton mouseButtonEvent && !mouseButtonEvent.Pressed) { // only trigger on mouse button release
            AcceptEvent();
            if (mouseButtonEvent.ButtonIndex != MouseButton.Left) {
                Cancel();
                return;
            }

            Node hoveredSelectable = GetHoveredSelectable();
            if (hoveredSelectable != null) Complete(hoveredSelectable);
            else Cancel();
        } else if (@event is InputEventKey keyEvent && keyEvent.IsActionPressed("ui_cancel")) {
            AcceptEvent();
            Cancel();
        }
    }

    private Node GetHoveredSelectable() {
        Vector2 mousePosition = GetViewport().GetMousePosition();
        List<Node> nodes = HoverChecker.GetHoveredNodes<Node>(CollisionLayers.SELECTABLE, PlayerController.Current.Player).ToList();
        GetControlsAtPosition(mousePosition, PlayerUI.Instance, nodes);
        foreach (Node node in nodes) {
            if (selectionFilter(node)) return node;
        }

        return null;
    }

    private void GetControlsAtPosition(Vector2 position, Control parent, List<Node> output) {
        foreach (Node child in parent.GetChildren()) {
            if (child is Control control) {
                if (control.GetGlobalRect().HasPoint(position)) {
                    output.Add(control);
                }
                GetControlsAtPosition(position, control, output);
            }
        }
    }

    private void Complete(Node node) {
        onSelection?.Invoke(node);
        Close();
    }

    public void Cancel() {
        onSelection?.Invoke(null);
        Close();
    }

    public void Select(Func<Node, bool> selectionFilter, Action<Node> onSelection, Texture2D validSelectionCursor = null, Texture2D invalidSelectionCursor = null) {
        this.selectionFilter = selectionFilter;
        this.onSelection = onSelection;
        this.validSelectionCursor = validSelectionCursor;
        this.invalidSelectionCursor = invalidSelectionCursor;
        Visible = true;
        MouseFilter = MouseFilterEnum.Stop;
    }

    private void Close() {
        Visible = false;
        selectionFilter = null;
        MouseFilter = MouseFilterEnum.Ignore;
        onSelection = null;
        Input.SetCustomMouseCursor(null);
    }
    
    
}