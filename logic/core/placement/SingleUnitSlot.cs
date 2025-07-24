using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;
using MPAutoChess.logic.core.util;
using ProtoBuf;
using PlayerController = MPAutoChess.logic.core.player.PlayerController;

namespace MPAutoChess.logic.core.placement;

[ProtoContract]
public partial class SingleUnitSlot : UnitContainer {
    
    [Export] public Sprite2D hoverEffect;
    [Export] public Texture2D HoverOffTexture;
    [Export] public Texture2D hoverOnTexture;
    
    public Player Player { get; set; }

    [ProtoMember(1)] public Unit? Unit { get; private set; }
    
    public void OnDragStart(Unit unit) {
        hoverEffect.Texture = HoverOffTexture;
    }
    
    public void OnDragProcess(Unit unit, IUnitDropTarget? target, Vector2 pos) {
        if (target == this && IsValidDrop(unit, pos)) {
            hoverEffect.Texture = hoverOnTexture;
        } else {
            hoverEffect.Texture = HoverOffTexture;
        }
    }
    
    public void OnDragEnd(Unit unit) {
        hoverEffect.Texture = null;
    }

    public override void _Ready() {
        hoverEffect.Texture = null; // make sure the hover effect is not visible initially (it is visible in editor for placement)
        if (PlayerController.Current == null) return; // this is a server, no user input to be handled
        PlayerController.Current.OnDragStart += OnDragStart;
        PlayerController.Current.OnDragProcess += OnDragProcess;
        PlayerController.Current.OnDragEnd += OnDragEnd;
        GD.Print("Added callbacks to " + PlayerController.Current);
    }
    
    public override void _ExitTree() {
        if (PlayerController.Current == null) return; // this is a server, no user input to be handled
        PlayerController.Current.OnDragStart -= OnDragStart;
        PlayerController.Current.OnDragProcess -= OnDragProcess;
        PlayerController.Current.OnDragEnd -= OnDragEnd;
    }

    public override bool IsValidDrop(Unit unit, Vector2 placement, Unit? replacedUnit = null) {
        return Unit == null || Unit == replacedUnit || Unit == unit;
    }
    public override Player GetPlayer() {
        return Player;
    }
    public override Unit GetUnitAt(Vector2 placement, Vector2 size = default) {
        return Unit;
    }
    public override Vector2 GetPlacement(Unit unit) {
        return unit == Unit ? Vector2.Zero : Vector2.One * -1; // Return a negative value if the unit is not placed
    }
    public override Vector2 ConvertToPlacement(Vector2 position, Unit forUnit) {
        return Vector2.Zero;
    }
    public override void RemoveUnit(Unit unit) {
        if (unit != Unit) return;
        
        UnitContainerUpdateEvent updateEvent = new UnitContainerUpdateEvent(this, unit, true);
        EventManager.INSTANCE.NotifyBefore(updateEvent);
            
        Unit.Container = null;
        Unit = null;
        RemoveChild(unit.GetOrCreatePassiveInstance());
            
        EventManager.INSTANCE.NotifyAfter(updateEvent);
        
        OnChange();
    }
    public override void AddUnit(Unit unit, Vector2 placement) {
        if (Unit != null) return;
        
        UnitContainerUpdateEvent updateEvent = new UnitContainerUpdateEvent(this, unit, false);
        EventManager.INSTANCE.NotifyBefore(updateEvent);
        
        Unit = unit;
        unit.Container = this;
        UnitInstance instance = unit.GetOrCreatePassiveInstance();
        AddChild(instance);
        instance.Position = Vector2.Zero;
        
        EventManager.INSTANCE.NotifyAfter(updateEvent);
        
        GD.Print("Added unit to SingleUnitSlot: " + unit.Type.ResourcePath);
        OnChange();
    }

    public override IEnumerable<Unit> GetUnits() {
        return new Unit[] { Unit };
    }

    private void OnChange() {
        if (ServerController.Instance.IsServer) {
            GD.Print($"Transferring SingleUnitSlot[{Unit?.Type.ResourcePath ?? "null"}]");
            Rpc(MethodName.TransferSlot, SerializerExtensions.Serialize(this));
        } else if (Unit != null) {
            GD.Print("OnChange called for SingleUnitSlot with unit: " + Unit.Type.ResourcePath);
            Unit.Container = this;
            UnitInstance unitInstance = Unit.GetOrCreatePassiveInstance();
            if (unitInstance.GetParent() == null) {
                AddChild(unitInstance);
                GD.Print("   Added child " + unitInstance.GetPath());
                unitInstance.Visible = true;
            } else if (unitInstance.GetParent() != this) {
                unitInstance.Reparent(this);
                GD.Print("   Reparented child" + unitInstance.GetPath());
            }
            unitInstance.Position = Vector2.Zero;
        }

        ClearLeftoverUnitInstances();
    }

    private void ClearLeftoverUnitInstances() {
        foreach (Node child in GetChildren()) {
            if (child is UnitInstance unitInstance && unitInstance.Unit != Unit) {
                RemoveChild(unitInstance);
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void TransferSlot(byte[] serializedBoard) {
        if (ServerController.Instance.IsServer) throw new InvalidOperationException("TransferBoard can only be called on the client.");
        SingleUnitSlot slot = SerializerExtensions.Deserialize<SingleUnitSlot>(serializedBoard);
        OnChange();
        if (slot != this) GD.PrintErr("Deserialized SingleUnitSlot does not match the current instance, this should not happen!");
    }
}