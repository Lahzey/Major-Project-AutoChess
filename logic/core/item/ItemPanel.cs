using System;
using System.Collections.Generic;
using Godot;
using MaterialTheming.Scripts;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.menu;
using Environment = System.Environment;

namespace MPAutoChess.logic.core.item;

public partial class ItemPanel : ItemDropTarget {
    
    private const long CRAFT_HOVER_TIME = 2000; // how many milliseconds until a drop onto another item is considered a craft instead of a swap

    [Export] public ItemIcon Icon { get; set; }
    [Export] public StyleBox CraftHoverStyle { get; set; }

    private StyleBoxDecorated styleBox;
    
    private Color defaultBorderColor;
    private Color hoverBorderColor;
    private Color dropAcceptBorderColor;
    private Color dropRefuseBorderColor;

    public Player Player { get; set; }
    public int InventoryIndex { get; set; }

    private long craftStartedAt = -1; // not using DateTime for performance reasons

    private int starCount = 0;

    public override Player GetOwningPlayer() {
        return Player;
    }
    
    public override void _Ready() {
        if (GetThemeStylebox("panel") is StyleBoxDecorated decoratedStyleBox) {
            styleBox = (StyleBoxDecorated)decoratedStyleBox.Duplicate(true);
            AddThemeStyleboxOverride("panel", styleBox); // makes the style box unique to each node, allowing them to control color individually
            defaultBorderColor = styleBox.BackgroundPolygon.BorderColor;
            hoverBorderColor = defaultBorderColor.Lightened(0.25f);
            dropAcceptBorderColor = defaultBorderColor.Lerp(Colors.Green, 0.2f);
            dropRefuseBorderColor = defaultBorderColor.Lerp(Colors.Red, 0.2f);
        } else {
            GD.PrintErr("Could not find stylebox 'panel'");
        }

        MouseEntered += () => {
            bool? canDrop = CanDropCurrentData();
            if (canDrop == null) {
                styleBox.BackgroundPolygon.BorderColor = hoverBorderColor;
                Item item = GetItem();
                if (item != null) {
                    ItemTooltip.Instance.Open(item);
                    ItemTooltip.Instance.Move(GetGlobalPosition() + new Vector2(Size.X + 5, 0));
                }
                return;
            }

            if (canDrop.Value) {
                ItemDragInfo dragInfo = (ItemDragInfo) GetViewport().GuiGetDragData().AsGodotObject();
                Item? craftingResult = GetCraftingResultWith(dragInfo.GetItem());
                if (dragInfo.InventoryIndex != InventoryIndex && craftingResult != null) {
                    SetShowCraftingPreview(true, craftingResult);
                } else {
                    styleBox.BackgroundPolygon.BorderColor = dropAcceptBorderColor;
                }
            } else {
                styleBox.BackgroundPolygon.BorderColor = dropRefuseBorderColor;
            }
        };
        MouseExited += () => {
            styleBox.BackgroundPolygon.BorderColor = defaultBorderColor;
            SetShowCraftingPreview(false);
           if (ItemTooltip.Instance.IsVisible()) ItemTooltip.Instance.Close();
        };
    }

    public override void _GuiInput(InputEvent @event) {
        if (@event is InputEventMouseButton mouseButton) {
            if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed) {
                Item item = GetItem();
                if (item == null) return;
                ContextMenu.Instance.ShowContextMenu(GetGlobalMousePosition(), new ContextMenuItem[] {
                    ContextMenuItem.Label("Drop", () => {
                    }),
                    ContextMenuItem.Label("Duplicate", () => {
                    })
                });
                GetViewport().SetInputAsHandled();
            }
        }
    }
    
    protected override bool SetShowCraftingPreview(bool show, Item? craftingResult = null) {
        bool changed = base.SetShowCraftingPreview(show, craftingResult);
        
        if (show) {
            AddThemeStyleboxOverride("panel", CraftHoverStyle);
            craftStartedAt = Environment.TickCount64;
        } else {
            AddThemeStyleboxOverride("panel", styleBox);
            craftStartedAt = -1;
        }
        return changed;
    }

    private Item? GetCraftingResultWith(Item with) {
        Item item = GetItem();
        if (item == null || with == null) return null;

        return GameSession.Instance.GetItemConfig().GetCraftingResult(item, with);
    }

    public override void _Process(double delta) {
        Icon.Item = GetItem();
    }

    public int GetIndex() {
        return InventoryIndex;
    }
    
    public Item? GetItem() {
        return Player?.Inventory?.GetItem(InventoryIndex);
    }

    public override Variant _GetDragData(Vector2 atPosition) {
        if (GetItem() == null) return (GodotObject) null;
        
        ItemIcon dragPreviewTexture = new ItemIcon();
        dragPreviewTexture.Item = GetItem();
        dragPreviewTexture.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        
        Control dragPreview = new Control();
        dragPreview.AddChild(dragPreviewTexture);
        dragPreviewTexture.Size = Size;
        dragPreviewTexture.Position = -GetLocalMousePosition();
        
        SetDragPreview(dragPreview);

        Icon.Modulate = Colors.Transparent;

        return new ItemDragInfo(this, Player, InventoryIndex);
    }

    public override void _Notification(int what) {
        switch ((long) what) { // why does it pass an int when the notification types are stored as longs???
            case NotificationDragEnd:
                Icon.Modulate = Colors.White;
                if (styleBox.BackgroundPolygon.BorderColor == dropAcceptBorderColor || styleBox.BackgroundPolygon.BorderColor == dropRefuseBorderColor) {
                    styleBox.BackgroundPolygon.BorderColor = hoverBorderColor;
                }
                SetShowCraftingPreview(false);
                break;
        }
    }

    public override bool CanDrop(Vector2 atPosition, ItemDragInfo dragInfo) {
        return true;
    }

    public override void OnDrop(Vector2 atPosition, ItemDragInfo dragInfo) {
        bool enableCrafting = craftStartedAt > 0 && (Environment.TickCount64 - craftStartedAt >= CRAFT_HOVER_TIME);
        PlayerController.Current.SwapItems(dragInfo.InventoryIndex, InventoryIndex, enableCrafting);
    }
}