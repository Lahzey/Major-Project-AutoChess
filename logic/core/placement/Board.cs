using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;
using MPAutoChess.logic.util;
using ProtoBuf;
using Vector2 = Godot.Vector2;

namespace MPAutoChess.logic.core.placement;

[ProtoContract]
public partial class Board : UnitContainer {

    [Export] public Sprite2D GridTexture;
    [Export] [ProtoMember(1)] public int Columns { get; set; } = 1;
    [Export] [ProtoMember(2)] public int Rows { get; set; } = 1;

    public Player Player { get; set; }

    [ProtoMember(3)] private BoardData data = new BoardData();

    public override void _Ready() {
        GridTexture.Scale = new Vector2(Columns, Rows);
        ((ShaderMaterial) GridTexture.Material).SetShaderParameter("columns", Columns);
        ((ShaderMaterial) GridTexture.Material).SetShaderParameter("rows", Rows);
        GridTexture.Visible = false;

        if (PlayerController.Current == null) return; // this is a server, no user input to be handled
        PlayerController.Current.OnDragStart += OnDragStart;
        PlayerController.Current.OnDragProcess += OnDragProcess;
        PlayerController.Current.OnDragEnd += OnDragEnd;
    }

    public override void _ExitTree() {
        if (PlayerController.Current == null) return; // this is a server, no user input to be handled
        PlayerController.Current.OnDragStart -= OnDragStart;
        PlayerController.Current.OnDragProcess -= OnDragProcess;
        PlayerController.Current.OnDragEnd -= OnDragEnd;
    }
    
    public override Player GetPlayer() {
        return Player;
    }
    
    private static bool DoesOverlap(Vector2 aPos, Vector2 aSize, Vector2 bPos, Vector2 bSize) {
        Vector2 aEnd = aPos + aSize;
        Vector2 bEnd = bPos + bSize;
        return (aPos.X < bPos.X ? aEnd.X > bPos.X : aPos.X < bEnd.X) &&
               (aPos.Y < bPos.Y ? aEnd.Y > bPos.Y : aPos.Y < bEnd.Y);
    }

    public override Unit? GetUnitAt(Vector2 placement, Vector2 size = default) {
        foreach (Unit unit in data.units) {
            Vector2 existingPlacement = data.placements[unit];
            if (DoesOverlap(placement, size, existingPlacement, unit.GetSize())) {
                return unit;
            }
        }
        return null; // No unit found at the given position
    }

    public override Vector2 GetPlacement(Unit unit) {
        return data.placements.TryGetValue(unit, out Vector2 placement) ? placement : Vector2.One * -1; // Return a negative value if the unit is not placed
    }
    
    public override Vector2 ConvertToPlacement(Vector2 position, Unit forUnit) {
        Vector2 size = forUnit.GetSize();
        position = ToLocal(position);
        position -= size * 0.5f;
        position.X = Mathf.Clamp(Mathf.Round(position.X), 0, Columns - size.X);
        position.Y = Mathf.Clamp(Mathf.Round(position.Y), 0, Rows - size.Y);
        Unit? blockingUnit = GetUnitAt(position, size);
        return (blockingUnit != null && blockingUnit != forUnit) ? GetPlacement(blockingUnit) : position;
    }

    public override void RemoveUnit(Unit unit) {
        if (!data.units.Contains(unit)) return;
        
        UnitContainerUpdateEvent updateEvent = new UnitContainerUpdateEvent(this, unit, true);
        EventManager.INSTANCE.NotifyBefore(updateEvent);
        
        unit.Container = null;
        data.units.Remove(unit);
        Vector2 placement = data.placements.TryGetValue(unit, out Vector2 placement2) ? placement2 : Vector2.One * -1;
        data.placements.Remove(unit);
        RemoveChild(unit.GetOrCreatePassiveInstance());
        
        EventManager.INSTANCE.NotifyAfter(updateEvent);
        
        OnChange();
    }
    
    public override void AddUnit(Unit unit, Vector2 placement) {
        if (!IsValidDrop(unit, placement)) return; // usually redundant (positions should be checked before calling this method), but can never be too safe
        
        UnitContainerUpdateEvent updateEvent = new UnitContainerUpdateEvent(this, unit, false);
        EventManager.INSTANCE.NotifyBefore(updateEvent);
        data.placements[unit] = placement;
        data.units.Add(unit);
        unit.Container = this;
        UnitInstance instance = unit.GetOrCreatePassiveInstance();
        AddChild(instance);
        instance.Position = placement + unit.GetSize() * 0.5f; // Center the instance on the placement
        
        EventManager.INSTANCE.NotifyAfter(updateEvent);
        
        OnChange();
    }

    public override IEnumerable<Unit> GetUnits() {
        return data.units;
    }

    public override bool IsValidDrop(Unit unit, Vector2 placement, Unit replacedUnit = null) {
        // slot count check
        int freedSlots = replacedUnit != null && replacedUnit.Container != this ? replacedUnit.Type.SlotsNeeded : 0;
        int requiredSlots = unit.Container != this ? unit.Type.SlotsNeeded : 0;
        int newSlotCount = data.units.Count + requiredSlots - freedSlots;
        if (Player.BoardSize.Evaluate() < newSlotCount) {
            GD.Print("Not enough slots on the board for unit: " + unit.Type.Name + " replacing: " + (replacedUnit?.Type.Name ?? "none"));
            return false;
        }
        
        // collision check
        foreach (Unit existingUnit in data.units) {
            if (existingUnit == unit || existingUnit == replacedUnit) {
                continue;
            }
            Vector2 existingPlacement = data.placements[existingUnit];
            if (DoesOverlap(placement, unit.GetSize(), existingPlacement, existingUnit.GetSize())) {
                GD.Print($"{unit.Type.Name} is overlapping with {existingUnit.Type.Name} at position {placement}");
                return false;
            }
        }
        return true;
    }

    private void OnDragStart(Unit unit) {
        if (PlayerController.Current.Player != Player) return;
        GridTexture.Visible = true;
    }

    private void OnDragProcess(Unit unit, IUnitDropTarget target, Vector2 mousePosition) {
        if (PlayerController.Current.Player != Player) return;
        mousePosition = ConvertToPlacement(mousePosition, unit);
        Vector4 highlightRange = target == this ? new Vector4(mousePosition.X, mousePosition.Y, mousePosition.X + unit.GetSize().X, mousePosition.Y + unit.GetSize().Y) : Vector4.Zero;
        ((ShaderMaterial) GridTexture.Material).SetShaderParameter("highlight_range", highlightRange);
    }

    private void OnDragEnd(Unit unit) {
        if (PlayerController.Current.Player != Player) return;
        GridTexture.Visible = false;
    }

    private void OnChange() {
        if (ServerController.Instance.IsServer) {
            Rpc(MethodName.TransferBoard, SerializerExtensions.Serialize(this));
        } else {
            foreach (Unit unit in data.units) {
                unit.Container = this;
                UnitInstance unitInstance = unit.GetOrCreatePassiveInstance();
                if (unitInstance.GetParent() == null) {
                    AddChild(unitInstance);
                } else if (unitInstance.GetParent() != this) {
                    unitInstance.Reparent(this);
                }
                unitInstance.Position = data.placements[unit] + unit.GetSize() * 0.5f;
            }
        }

        ClearLeftoverUnitInstances();
    }

    private void ClearLeftoverUnitInstances() {
        foreach (Node child in GetChildren()) {
            if (child is UnitInstance unitInstance && !data.units.Contains(unitInstance.Unit)) {
                RemoveChild(unitInstance);
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void TransferBoard(byte[] serializedBoard) {
        if (ServerController.Instance.IsServer) throw new InvalidOperationException("TransferBoard can only be called on the client.");
        Board board = SerializerExtensions.Deserialize<Board>(serializedBoard);
        OnChange();
        if (board != this) GD.PrintErr("Deserialized Board does not match the current instance, this should not happen!");
    }

    [ProtoContract]
    public class BoardData {
        [ProtoMember(1)] public List<Unit> units = new List<Unit>();
        [ProtoMember(2)] public Dictionary<Unit, Vector2> placements = new Dictionary<Unit, Vector2>();

        public BoardData() { }
    }
}