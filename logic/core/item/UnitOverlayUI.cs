using System.Collections.Generic;
using Godot;
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

    public UnitInstance UnitInstance { get; set; }

    private List<TextureRect> itemIcons = new List<TextureRect>();

    public UnitOverlayUI() {
        MouseEntered += () => {
            bool? canDrop = CanDropCurrentData();
            if (canDrop == null) return;

            if (canDrop.Value) {
                ItemDragInfo dragInfo = (ItemDragInfo) GetViewport().GuiGetDragData().AsGodotObject();
                ItemType? craftingTarget = UnitInstance.Unit.GetCraftingTargetWith(dragInfo.GetItem(), out _);
                if (craftingTarget != null) {
                    SetShowCraftingPreview(true, craftingTarget);
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
        HealthBar.Connect(UnitInstance, instance => instance.Stats.GetValue(StatType.MAX_HEALTH), instance => instance.CurrentHealth);
        HealthBar.SetGaps(ResourceBar.HEALTH_SMALL_GAP, ResourceBar.HEALTH_LARGE_GAP);
        HealthBar.SetFillColor(ResourceBar.HEALTH_COLOR);
        ManaBar.Connect(UnitInstance, instance => instance.Stats.GetValue(StatType.MAX_MANA), instance => instance.CurrentMana);
        ManaBar.SetGaps(ResourceBar.NO_GAP, ResourceBar.NO_GAP);
        ManaBar.SetFillColor(ResourceBar.MANA_COLOR);
    }

    public override void _Process(double delta) {
        if (UnitInstance.Unit.EquippedItems.Count == 0) {
            IconsContainer.Visible = false;
            IconsSpacer.Visible = true;
            return;
        }
        IconsContainer.Visible = true;
        IconsSpacer.Visible = false;
        
        while (itemIcons.Count < UnitInstance.Unit.EquippedItems.Count) {
            TextureRect itemIcon = new TextureRect();
            itemIcon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
            itemIcons.Add(itemIcon);
            IconsContainer.AddChild(itemIcon);
        }
        while (itemIcons.Count > UnitInstance.Unit.EquippedItems.Count) {
            TextureRect itemIcon = itemIcons[0];
            itemIcons.Remove(itemIcon);
            IconsContainer.RemoveChild(itemIcon);
            itemIcon.QueueFree();
        }

        for (int i = 0; i < itemIcons.Count; i++) {
            itemIcons[i].Texture = UnitInstance.Unit.EquippedItems[i].Type.Icon;
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