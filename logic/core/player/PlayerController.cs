using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.item.consumable;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.shop;
using MPAutoChess.logic.core.unit;
using MPAutoChess.logic.util;
using Environment = System.Environment;
using UnitInstance = MPAutoChess.logic.core.unit.UnitInstance;

namespace MPAutoChess.logic.core.player;

public partial class PlayerController : Node {
    public Player Player { get; private set; }

    public event Action<Unit> OnDragStart;
    public event Action<Unit, IUnitDropTarget?, Vector2> OnDragProcess;
    public event Action<Unit> OnDragEnd;

    private readonly UnitDragProcessor dragProcessor = new UnitDragProcessor();

    private static System.Collections.Generic.Dictionary<Player, PlayerController> playerControllers = new System.Collections.Generic.Dictionary<Player, PlayerController>();
    public static PlayerController Current { get; private set; }

    public PlayerController() { }

    public PlayerController(Player player) {
        Player = player;
        Name = player.Name + "Controller";
        if (!ServerController.Instance.IsServer) {
            if (Current != null) throw new InvalidOperationException("Only servers can have multiple PlayerController instances.");
            Current = this;
            EventManager.INSTANCE.AddAfterListener<CombatStartEvent>(OnCombatStart);
        }

        if (Player == null) throw new ArgumentNullException(nameof(Player), "Player cannot be null.");
        playerControllers.Add(Player, this);
    }

    private void OnCombatStart(CombatStartEvent e) {
        if (e.Combat.PlayerA != Player && (e.Combat.PlayerB != Player || e.Combat.IsCloneFight)) return; // Not our combat, ignore
        
        if (dragProcessor.Running && dragProcessor.UnitInstance.Unit.Container == Player.Board) {
            dragProcessor.Complete(true);
        }
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

    public override void _UnhandledInput(InputEvent @event) {
        if (ServerController.Instance.IsServer) return; // Server sho1uld not receive input events, but just to be sure

        if (@event is InputEventMouseButton mouseButtonEvent) {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left) {
                OnLeftClick(mouseButtonEvent);
            }
        }
    }

    private void OnLeftClick(InputEventMouseButton mouseButtonEvent) {
        if (mouseButtonEvent.Pressed) {
            if (!dragProcessor.Running) {
                UnitInstance? hoveredUnitInstance = HoverChecker.GetHoveredNodeOrNull<UnitInstance>(CollisionLayers.PASSIVE_UNIT_INSTANCE, Player);
                if (hoveredUnitInstance != null) {
                    CollisionLayers hoveredLayer = (CollisionLayers)hoveredUnitInstance.CollisionLayer;
                }
                if (hoveredUnitInstance != null && hoveredUnitInstance.Unit.Container.GetPlayer() == Player) {
                    dragProcessor.Start(hoveredUnitInstance);
                    OnDragStart?.Invoke(dragProcessor.UnitInstance.Unit);
                }
            } else {
                OnDragEnd?.Invoke(dragProcessor.UnitInstance.Unit);
                dragProcessor.Complete();
            }
        } else if (dragProcessor.Running) {
            // Mouse up within 250 ms of mouse down: click to pickup -> click to drop (select and place)
            // Mouse up after 250 ms of mouse down:  mouse down to pickup -> mouse up to drop (drag to destination)
            if ((Environment.TickCount64 - dragProcessor.DragStartTime) > 250) {
                OnDragEnd?.Invoke(dragProcessor.UnitInstance.Unit);
                dragProcessor.Complete();
            }
        }
    }

    public override void _Process(double delta) {
        if (ServerController.Instance.IsServer) return;
        if (dragProcessor.Running) {
            dragProcessor.Update(Player);
            OnDragProcess?.Invoke(dragProcessor.UnitInstance.Unit, dragProcessor.HoveredDropTarget, dragProcessor.CurrentMousePosition);
        }
    }


    public void MakeChoice<T>(T choosable, int choice) where T : Node, Choosable {
        if (ServerController.Instance.IsServer) {
            RequestMakeChoice(choosable.GetPath(), choice);
        } else {
            this.RpcToServer(MethodName.RequestMakeChoice, choosable.GetPath(), choice);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestMakeChoice(string choosablePath, int choice) {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("RequestMakeChoice can only be called on the server.");
        Node choosableNode = GetNodeOrNull(choosablePath);
        if (choosableNode == null) {
            GD.PrintErr($"Node at path {choosablePath} not found.");
            return;
        }

        if (choosableNode is not Choosable choosable) {
            GD.PrintErr($"Node at path {choosablePath} is not a Choosable.");
            return;
        }

        ServerController.Instance.RunInContext(() => { choosable.Choose(choice); }, this);
    }

    public void MoveUnit(Unit unit, IUnitDropTarget dropTarget, Vector2 placement) {
        if (dropTarget is not Node dropTargetNode) throw new ArgumentException("Drop target must be a Node.", nameof(dropTarget));
        this.RpcToServer(MethodName.RequestMoveUnit, unit.Id, dropTargetNode.GetPath(), placement);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestMoveUnit(string unitId, string dropTargetPath, Vector2 placement) {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("RequestMoveUnit can only be called on the server.");

        IIdentifiable identifiable = IIdentifiable.TryGetInstance(unitId);
        if (identifiable is not Unit unit) {
            GD.PrintErr($"Unit with ID {unitId} not found or is not a Unit.");
            return;
        }

        Node node = GetNodeOrNull(dropTargetPath);
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
                GD.PrintErr($"Unit or drop target not owned by current player. Unit Owner: {unit.Container.GetPlayer()?.Account.Id.ToString() ?? "null"} | Drop Target Owner: {dropTarget.GetPlayer()?.Account.Id.ToString() ?? "null"} | Current: {Player.Account.Id}");
                return;
            }

            dropTarget.OnUnitDrop(unit, placement);
        }, this);
    }

    public void RerollShop() {
        if (ServerController.Instance.IsServer)
            RequestShopReroll();
        else
            this.RpcToServer(MethodName.RequestShopReroll);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestShopReroll() {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("RequestShopReroll can only be called on the server.");
        ServerController.Instance.RunInContext(() => {
            Player.TryPurchase(2, () => Player.Shop.Reroll()); // TODO dynamic cost and fire event
        }, this);
    }

    public void BuyXp() {
        if (ServerController.Instance.IsServer)
            RequestBuyXp();
        else
            this.RpcToServer(MethodName.RequestBuyXp);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestBuyXp() {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("RequestBuyXp can only be called on the server.");
        ServerController.Instance.RunInContext(() => {
            Player.TryPurchase(1, () => Player.Experience += 1); // TODO dynamic cost and fire event
        }, this);
    }

    public void BuyShopOffer(ShopOffer offer) {
        if (ServerController.Instance.IsServer)
            RequestBuyShopOffer(Player.Shop.IndexOf(offer));
        else
            this.RpcToServer(MethodName.RequestBuyShopOffer, Player.Shop.IndexOf(offer));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestBuyShopOffer(int slotIndex) {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("RequestBuyShopOffer can only be called on the server.");
        ServerController.Instance.RunInContext(() => {
            ShopOffer shopOffer = Player.Shop.GetOfferAt(slotIndex);
            shopOffer.TryPurchase();
        }, this);
    }

    public void SwapItems(int indexA, int indexB, bool enableCrafting) {
        this.RpcToServer(MethodName.RequestSwapItems, indexA, indexB, enableCrafting);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestSwapItems(int indexA, int indexB, bool enableCrafting) {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("RequestEquipItem can only be called on the server.");

        if (indexA == indexB) {
            GD.PrintErr("Cannot swap items at the same index.");
            return;
        }

        ServerController.Instance.RunInContext(() => {
            Item? itemA = Player.Inventory.GetItem(indexA);
            Item? itemB = Player.Inventory.GetItem(indexB);


            Item? craftingResult = null;
            if (enableCrafting && itemA != null && itemB != null)
                craftingResult = GameSession.Instance.GetItemConfig().GetCraftingResult(itemA, itemB);

            if (craftingResult != null) {
                Player.Inventory.ReplaceItem(Mathf.Min(indexA, indexB), craftingResult);
                Player.Inventory.ReplaceItem(Mathf.Max(indexA, indexB), null);
            } else {
                Player.Inventory.ReplaceItem(indexA, itemB);
                Player.Inventory.ReplaceItem(indexB, itemA);
            }
        }, this);
    }

    public void EquipItem(int inventoryIndex, Unit unit) {
        this.RpcToServer(MethodName.RequestEquipItem, inventoryIndex, unit.Id);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestEquipItem(int inventoryIndex, string unitId) {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("RequestEquipItem can only be called on the server.");

        if (IIdentifiable.TryGetInstance(unitId) is not Unit unit) {
            GD.PrintErr($"Unit with ID {unitId} not found or is not a Unit");
            return;
        }

        ServerController.Instance.RunInContext(() => {
            if (unit.Container.GetPlayer() != Player) {
                GD.PrintErr($"Unit {unit.Type.ResourcePath} is not owned by player {Player.Name}");
                return;
            }

            Item item = Player.Inventory.GetItem(inventoryIndex);
            if (item == null) {
                GD.PrintErr($"No item found at inventory index {inventoryIndex} for player {Player.Name}");
                return;
            }

            Item? craftingResult = unit.GetCraftingResultWith(item, out Item craftedFrom);
            bool success;
            if (craftingResult != null) {
                unit.ReplaceItem(craftedFrom, craftingResult);
                success = true;
            } else {
                success = unit.EquipItem(item);
            }

            if (success) Player.Inventory.ReplaceItem(inventoryIndex, null);
        }, this);
    }

    public void UseConsumable(Consumable consumable, Node target, int extraChoice = -1) {
        this.RpcToServer(MethodName.RequestUseConsumable, consumable.GetTypeName(), target.GetPath(), extraChoice);
    }

    public void UseConsumable(Consumable consumable, IIdentifiable target, int extraChoice = -1) {
        this.RpcToServer(MethodName.RequestUseConsumable, consumable.GetTypeName(), target.GetId(), extraChoice);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestUseConsumable(string consumableTypeName, string targetPathOrId, int extraChoice) {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("RequestUseConsumable can only be called on the server.");
        Consumable? consumable = Consumable.GetByTypeName(consumableTypeName);
        if (consumable == null) {
            GD.PrintErr($"Consumable of type {consumableTypeName} not found.");
            return;
        }

        object? target = IIdentifiable.TryGetInstance(targetPathOrId) ?? (object)GetNodeOrNull(targetPathOrId);
        if (target == null) {
            GD.PrintErr($"Target Identifiable or Node at ID/Path {targetPathOrId} not found.");
            return;
        }

        ServerController.Instance.RunInContext(() => {
            uint consumableCount = Player.GetConsumableCount(consumable);
            if (consumableCount == 0) {
                GD.PrintErr($"Player {Player.Name} has no consumables of type {consumableTypeName}.");
                return;
            }

            if (!consumable.IsValidTarget(target, extraChoice)) {
                GD.PrintErr($"Consumable {consumableTypeName} is not valid for target at path/id {targetPathOrId} with choice {extraChoice}.");
                return;
            }

            bool success = consumable.Consume(target, extraChoice);
            if (success) Player.SetConsumableCount(consumable, consumableCount - 1);
            else GD.PrintErr($"Consumable {consumableTypeName} could not be consumed on target at path/id {targetPathOrId} with choice {extraChoice}.");
        }, this);
    }
}