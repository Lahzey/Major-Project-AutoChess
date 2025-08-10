using System;
using Godot;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.shop;
using MPAutoChess.logic.core.unit.role;

namespace MPAutoChess.logic.core.player;

public partial class PlayerUI : Control {
    
    public static PlayerUI Instance { get; private set; }

    [Export] public Control FreeSpace { get; set; }
    [Export] public ShopUI Shop { get; set; }
    [Export] public InventoryPanel Inventory { get; set; }
    [Export] public GamePhaseControls GamePhaseControls { get; set; }
    [Export] public UnitRoleListPanel UnitRoleList { get; set; }

    public override void _EnterTree() {
        Instance = this;
    }

    public override void _Ready() {
        // so we can avoid null checks on children (at least in the process function)
        SetProcessMode(ProcessModeEnum.Disabled);
        SetVisible(false);
        SetProcessInput(false);
    }

    public override void _ExitTree() {
        if (Instance == this) {
            Instance = null;
        }
    }

    public void SetPlayer(Player player) {
        Inventory.Player = player;
        UnitRoleList.Player = player;
        SetProcessMode(ProcessModeEnum.Inherit);
        SetVisible(true);
        SetProcessInput(true);
    }
}