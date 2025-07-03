using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;
using Vector2 = Godot.Vector2;

namespace MPAutoChess.logic.core.placement;

public partial class Board : Area2D, UnitContainer {

    [Export] public Sprite2D GridTexture;
    [Export] public int Columns { get; set; } = 1;
    [Export] public int Rows { get; set; } = 1;

    public Player Player { get; set; }

    private List<Unit> units = new List<Unit>();
    private Dictionary<Unit, Vector2> placements = new Dictionary<Unit, Vector2>();

    public override void _Ready() {
        GridTexture.Scale = new Vector2(Columns, Rows);
        ((ShaderMaterial) GridTexture.Material).SetShaderParameter("columns", Columns);
        ((ShaderMaterial) GridTexture.Material).SetShaderParameter("rows", Rows);
        GridTexture.Visible = false;
    }
    
    public override void _EnterTree() {
        PlayerController.Instance.OnDragStart += OnDragStart;
        PlayerController.Instance.OnDragProcess += OnDragProcess;
        PlayerController.Instance.OnDragEnd += OnDragEnd;
    }

    public override void _ExitTree() {
        PlayerController.Instance.OnDragStart -= OnDragStart;
        PlayerController.Instance.OnDragProcess -= OnDragProcess;
        PlayerController.Instance.OnDragEnd -= OnDragEnd;
    }

    public BoardSearch Search() {
        return new BoardSearch(this);
    }
    
    public Player GetPlayer() {
        return Player;
    }
    
    private static bool DoesOverlap(Vector2 aPos, Vector2 aSize, Vector2 bPos, Vector2 bSize) {
        Vector2 aEnd = aPos + aSize;
        Vector2 bEnd = bPos + bSize;
        return (aPos.X < bPos.X ? aEnd.X > bPos.X : aPos.X < bEnd.X) &&
               (aPos.Y < bPos.Y ? aEnd.Y > bPos.Y : aPos.Y < bEnd.Y);
    }

    public Unit? GetUnitAt(Vector2 position) {
        foreach (Unit unit in units) {
            Vector2 placement = placements[unit];
            if (DoesOverlap(position, Vector2.Zero, placement, unit.GetSize())) {
                return unit;
            }
        }
        return null; // No unit found at the given position
    }

    public Vector2 GetPlacement(Unit unit) {
        return placements.TryGetValue(unit, out Vector2 placement) ? placement : Vector2.One * -1; // Return a negative value if the unit is not placed
    }
    
    public Vector2 ConvertToPlacement(Vector2 position, Vector2 size) {
        position -= size * 0.5f;
        position.X = Mathf.Clamp(Mathf.Round(position.X), 0, Columns - size.X);
        position.Y = Mathf.Clamp(Mathf.Round(position.Y), 0, Rows - size.Y);
        return position;
    }

    public bool CanFitAt(Unit unit, Vector2 pos, Unit? replacedUnit = null) {
        pos = ConvertToPlacement(pos, unit.GetSize());
        
        // slot count check
        int freedSlots = replacedUnit != null && replacedUnit.Container != this ? replacedUnit.Type.SlotsNeeded : 0;
        int requiredSlots = unit.Container != this ? unit.Type.SlotsNeeded : 0;
        int newSlotCount = units.Count + requiredSlots - freedSlots;
        if (Player.BoardSize.Evaluate() < newSlotCount) {
            GD.Print("Not enough slots on the board for unit: " + unit.Type.Name + " replacing: " + (replacedUnit?.Type.Name ?? "none"));
            return false;
        }
        
        // collision check
        foreach (Unit boardUnit in units) {
            if (boardUnit == replacedUnit) {
                continue;
            }
            Vector2 placement = placements[boardUnit];
            if (DoesOverlap(pos, unit.GetSize(), placement, boardUnit.GetSize())) {
                GD.Print($"{unit.Type.Name} is overlapping with {boardUnit.Type.Name} at position {pos}");
                return false;
            }
        }
        return true;
    }

    public Vector2 RemoveUnit(Unit unit) {
        unit.Container = null;
        units.Remove(unit);
        Vector2 placement = placements.TryGetValue(unit, out Vector2 placement2) ? placement2 : Vector2.One * -1;
        placements.Remove(unit);
        RemoveChild(unit.GetOrCreatePassiveInstance());
        return placement;
    }
    public void AddUnit(Unit unit, Vector2 position) {
        position = ConvertToPlacement(position, unit.GetSize());
        placements[unit] = position;
        units.Add(unit);
        unit.Container = this;
        UnitInstance instance = unit.GetOrCreatePassiveInstance();
        AddChild(instance);
        instance.Position = position + unit.GetSize() * 0.5f; // Center the instance on the placement
    }

    public bool IsValidDrop(Unit unit, Vector2 pos) {
        return unit.CanBePlacedAt(this, pos);
    }
    
    public void OnUnitDrop(Unit unit, Vector2 pos) {
        if (!unit.CanBePlacedAt(this, pos)) {
            GD.PrintErr("Invalid drop for unit at position: " + pos);
        }
        
        Unit? existingUnit = GetUnitAt(pos);
        if (existingUnit != null) {
            units.Remove(existingUnit);
            placements.Remove(existingUnit);
        }

        UnitContainer prevContainer = unit.Container;
        Vector2 prevPlacement = prevContainer.RemoveUnit(unit);
        if (existingUnit != null) RemoveUnit(existingUnit);
        AddUnit(unit, pos);
        if (existingUnit != null) prevContainer.AddUnit(existingUnit, prevPlacement);
    }

    private void OnDragStart(Unit unit) {
        GridTexture.Visible = true;
    }

    private void OnDragProcess(Unit unit, UnitDropTarget target, Vector2 position) {
        position = ConvertToPlacement(position, unit.GetSize());
        Vector4 highlightRange = target == this ? new Vector4(position.X, position.Y, position.X + unit.GetSize().X, position.Y + unit.GetSize().Y) : Vector4.Zero;
        ((ShaderMaterial) GridTexture.Material).SetShaderParameter("highlight_range", highlightRange);
    }

    private void OnDragEnd(Unit unit) {
        GridTexture.Visible = false;
    }
}