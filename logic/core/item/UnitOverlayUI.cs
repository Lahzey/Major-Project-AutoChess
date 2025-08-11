using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.item;

public partial class UnitOverlayUI : ItemDropTarget {
    
    [Export] public ResourceBar HealthBar { get; set; }
    [Export] public ResourceBar ManaBar { get; set; }
    [Export] public Container IconsContainer { get; set; }
    [Export] public Control IconsSpacer { get; set; }
    [Export] public Label LevelLabel { get; set; }

    public UnitInstance UnitInstance { get; set; }

    private List<ItemIcon> itemIcons = new List<ItemIcon>();

    public UnitOverlayUI() {
        MouseEntered += () => {
            bool? canDrop = CanDropCurrentData();
            if (canDrop == null) return;

            if (canDrop.Value) {
                ItemDragInfo dragInfo = (ItemDragInfo) GetViewport().GuiGetDragData().AsGodotObject();
                Item? craftingResult = UnitInstance.Unit.GetCraftingResultWith(dragInfo.GetItem(), out _);
                if (craftingResult != null) {
                    SetShowCraftingPreview(true, craftingResult);
                }
            } else {
                return;
            }
        };
        MouseExited += () => {
            if (showingCraftingPreview) SetShowCraftingPreview(false);
            else if (ItemTooltip.Instance.IsVisible()) ItemTooltip.Instance.Close();
        };
    }

    public override void _Ready() {
        if (ServerController.Instance.IsServer) return; // just to be sure, UnitOverlayUI should not be created on the server side
        
        HealthBar.Connect(UnitInstance, instance => instance.Stats.GetValue(StatType.MAX_HEALTH), instance => instance.CurrentHealth);
        HealthBar.SetGaps(ResourceBar.HEALTH_SMALL_GAP, ResourceBar.HEALTH_LARGE_GAP);
        HealthBar.SetFillColor(GetHealthBarColor());
        ManaBar.Connect(UnitInstance, instance => instance.Stats.GetValue(StatType.MAX_MANA), instance => instance.CurrentMana);
        ManaBar.SetGaps(ResourceBar.NO_GAP, ResourceBar.NO_GAP);
        ManaBar.SetFillColor(ResourceBar.MANA_COLOR);
        SetDefaultCursorShape(UnitInstance.IsCombatInstance ? CursorShape.Arrow : CursorShape.PointingHand);
    }

    private Color GetHealthBarColor() {
        if (UnitInstance.IsCombatInstance) {
            bool teamAIsPlayer = UnitInstance.CurrentCombat.PlayerA == PlayerController.Current.Player;
            bool teamBIsPlayer = UnitInstance.CurrentCombat.PlayerB == PlayerController.Current.Player;
            bool teamAIsEnemy = teamBIsPlayer; // by default, team b is the ally and team a is the enemy, but if current player is in team b, that is swapped
            if ((teamAIsPlayer && UnitInstance.IsInTeamA) || (teamBIsPlayer && !UnitInstance.IsInTeamA)) return ResourceBar.HEALTH_SELF_COLOR;
            return UnitInstance.IsInTeamA == teamAIsEnemy ? ResourceBar.HEALTH_ENEMY_COLOR : ResourceBar.HEALTH_ALLY_COLOR;
        } else {
            return UnitInstance.Unit.Container.GetPlayer() == PlayerController.Current.Player ? ResourceBar.HEALTH_SELF_COLOR : ResourceBar.HEALTH_ENEMY_COLOR;
        }
    }

    public override void _Process(double delta) {
        Visible = UnitInstance.IsVisibleInTree();
        LevelLabel.Text = UnitInstance.GetLevel().ToString();
        
        if (UnitInstance.Unit.EquippedItems.Count == 0) {
            IconsContainer.Visible = false;
            IconsSpacer.Visible = true;
            return;
        }
        IconsContainer.Visible = true;
        IconsSpacer.Visible = false;
        
        while (itemIcons.Count < UnitInstance.Unit.EquippedItems.Count) {
            ItemIcon itemIcon = new ItemIcon();
            itemIcon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
            itemIcons.Add(itemIcon);
            IconsContainer.AddChild(itemIcon);
        }
        while (itemIcons.Count > UnitInstance.Unit.EquippedItems.Count) {
            ItemIcon itemIcon = itemIcons[0];
            itemIcons.Remove(itemIcon);
            IconsContainer.RemoveChild(itemIcon);
            itemIcon.QueueFree();
        }

        for (int i = 0; i < itemIcons.Count; i++) {
            itemIcons[i].Item = UnitInstance.Unit.EquippedItems[i];
        }
    }

    public override Player GetOwningPlayer() {
        return UnitInstance.Unit.Container.GetPlayer();
    }
    
    public override bool CanDrop(Vector2 atPosition, ItemDragInfo dragInfo) {
        return true; // declining an invalid drop here would prevent any user messages from being shown
    }
    
    public override void OnDrop(Vector2 atPosition, ItemDragInfo dragInfo) {
        PlayerController.Current.EquipItem(dragInfo.InventoryIndex, UnitInstance.Unit);
    }
}