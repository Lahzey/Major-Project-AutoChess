using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using MPAutoChess.logic.core.environment;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.shop;
using MPAutoChess.logic.core.unit;
using MPAutoChess.logic.core.util;
using UnitInstance = MPAutoChess.logic.core.unit.UnitInstance;

namespace MPAutoChess.logic.core.player;

public partial class PlayerController : Node2D {
    
    public Player Player { get; private set; }

    public event Action<Unit> OnDragStart;
    public event Action<Unit, IUnitDropTarget?, Vector2> OnDragProcess;
    public event Action<Unit> OnDragEnd;

    private readonly UnitDragProcessor dragProcessor = new UnitDragProcessor();
    
    private static Dictionary<Player, PlayerController> playerControllers = new Dictionary<Player, PlayerController>();
    public static PlayerController Current { get; private set; }

    public PlayerController() { }

    public PlayerController(Player player) {
        Player = player;
        Name = player.Name +"Controller";
        if (!ServerController.Instance.IsServer) {
            if (Current != null) throw new InvalidOperationException("Only servers can have multiple PlayerController instances.");
            Current = this;
        }
        if (Player == null) throw new ArgumentNullException(nameof(Player), "Player cannot be null.");
        playerControllers.Add(Player, this);
    }

    public override void _Ready() {
        if (!ServerController.Instance.IsServer) {
            CameraController.Instance.Cover(new Rect2(Player.Arena.GlobalPosition, Player.Arena.ArenaSize));
        }
    }

    public static PlayerController GetForPlayer(Player player) {
        return playerControllers[player];
    }

    public void RunInContext(Action action) {
        if (!ServerController.Instance.IsServer) throw new ArgumentException("RunInContext can only be called on the server.");
        Current = this;
        action.Invoke();
        Current = null;
    }

    public override void _Input(InputEvent @event) {
        if (ServerController.Instance.IsServer) return; // Server should not receive input events, but just to be sure
        
        if (@event is InputEventMouseButton mouseButtonEvent) {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left) {
                OnLeftClick(mouseButtonEvent);
            }
        }
    }
    
    private void OnLeftClick(InputEventMouseButton mouseButtonEvent) {
        GD.Print("Left click");
        if (mouseButtonEvent.Pressed) {
            GD.Print("Drag process running: " + dragProcessor.Running);
            if (!dragProcessor.Running) {
                UnitInstance? hoveredUnitInstance = HoverChecker.GetHoveredNodeOrNull<UnitInstance>(CollisionLayers.PASSIVE_UNIT_INSTANCE, this);
                if (hoveredUnitInstance != null) {
                    dragProcessor.Start(hoveredUnitInstance);
                    OnDragStart?.Invoke(dragProcessor.UnitInstance.Unit);
                }
            } else {
                OnDragEnd?.Invoke(dragProcessor.UnitInstance.Unit);
                dragProcessor.Complete();
            }
        } else if(dragProcessor.Running) {
            // Mouse up within 100 ms of mouse down: click to pickup -> click to drop (select and place)
            // Mouse up after 100 ms of mouse down:  mouse down to pickup -> mouse up to drop (drag to destination)
            if ((dragProcessor.DragStartTime - DateTime.Now).TotalMilliseconds > 100) {
                OnDragEnd?.Invoke(dragProcessor.UnitInstance.Unit);
                dragProcessor.Complete();
            }
        }
    }

    public override void _Process(double delta) {
        if (ServerController.Instance.IsServer) return;
        if (dragProcessor.Running) {
            dragProcessor.Update(this);
            OnDragProcess?.Invoke(dragProcessor.UnitInstance.Unit, dragProcessor.HoveredDropTarget, dragProcessor.CurrentMousePosition);
            GD.Print("Called on drag process: " + OnDragProcess);
        }
    }
    
    public void MoveUnit(Unit unit, IUnitDropTarget dropTarget, Vector2 placement) {
        if (dropTarget is not Node dropTargetNode) throw new ArgumentException("Drop target must be a Node.", nameof(dropTarget));
        Rpc(MethodName.RequestMoveUnit, unit.Id, dropTargetNode.GetPath(), placement);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestMoveUnit(string unitId, string dropTargetPath, Vector2 placement) {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("RequestMoveUnit can only be called on the server.");
        
        IIdentifiable identifiable = IIdentifiable.TryGetInstance(unitId);
        if (identifiable is not Unit unit) {
            GD.PrintErr($"Unit with ID {unitId} not found or is not a Unit.");
            return;
        }
        
        Node node = GetNode(dropTargetPath);
        if (node is not IUnitDropTarget dropTarget) {
            GD.PrintErr($"Node at path {dropTargetPath} not found or not a drop target.");
            return;
        }
        
        ServerController.Instance.RunInContext(() => {
            if (unit.Container == null) {
                GD.PrintErr($"Unit {unit.Type.ResourcePath} has no container, cannot move.");
                return;
            }
            if (unit.Container.GetPlayer() != Player || dropTarget.GetPlayer() != Player) {
                throw new InvalidOperationException($"Unit or drop target not owned by current player. Owner: {unit.Container.GetPlayer()?.Account.Id.ToString() ?? "null"} Current: {Player.Account.Id}");
            }
            dropTarget.OnUnitDrop(unit, placement);
        }, this);
    }

    public void RerollShop() {
        if (ServerController.Instance.IsServer)
            RequestShopReroll();
        else
            Rpc(MethodName.RequestShopReroll);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestShopReroll() {
        ServerController.Instance.RunInContext(() => {
            Player.TryPurchase(2, () => Player.Shop.Reroll()); // TODO dynamic cost and fire event
        }, this);
    }

    public void BuyXp() {
        if (ServerController.Instance.IsServer)
            RequestBuyXp();
        else
            Rpc(MethodName.RequestBuyXp);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestBuyXp() {
        ServerController.Instance.RunInContext(() => {
            Player.TryPurchase(1, () => Player.Experience += 1); // TODO dynamic cost and fire event
        }, this);
    }

    public void BuyShopOffer(ShopOffer offer) {
        if (ServerController.Instance.IsServer)
            RequestBuyShopOffer(Player.Shop.IndexOf(offer));
        else
            Rpc(MethodName.RequestBuyShopOffer, Player.Shop.IndexOf(offer));
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestBuyShopOffer(int slotIndex) {
        ServerController.Instance.RunInContext(() => {
            ShopOffer shopOffer = Player.Shop.GetOfferAt(slotIndex);
            shopOffer.TryPurchase();
        }, this);
    }
}