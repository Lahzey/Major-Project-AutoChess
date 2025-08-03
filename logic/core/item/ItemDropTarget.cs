using System;
using Godot;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.item;

public abstract partial class ItemDropTarget : Control {

    public abstract Player GetOwningPlayer();

    public abstract bool CanDrop(Vector2 atPosition, ItemDragInfo dragInfo);

    public abstract void OnDrop(Vector2 atPosition, ItemDragInfo dragInfo);
    
    protected bool showingCraftingPreview = false;
    
    protected bool? CanDropCurrentData() {
        Variant dragData = GetViewport().GuiGetDragData();
        if (dragData.VariantType == Variant.Type.Nil) return null;
        
        if (_CanDropData(GetLocalMousePosition(), dragData)) {
            if (dragData.AsGodotObject() == this) return null;
            else return true;
        } else {
            return false;
        }
    }
    
    public override bool _CanDropData(Vector2 atPosition, Variant data) {
        try {
            GodotObject obj = data.AsGodotObject();
            if (obj is ItemDragInfo dragInfo) {
                if (dragInfo.OwningPlayer == GetOwningPlayer()) {
                    return CanDrop(atPosition, dragInfo);
                } else {
                    return false;
                }
            } else return false;
        } catch (Exception e) {
            return false;
        }
    }

    public override void _DropData(Vector2 atPosition, Variant data) {
        GodotObject obj = data.AsGodotObject();
        if (obj is not ItemDragInfo dragInfo) return;
        if (dragInfo.Source == this) return;

        OnDrop(atPosition, dragInfo);
    }
    
    protected virtual bool SetShowCraftingPreview(bool show, ItemType? craftingTarget = null) {
        if (show == showingCraftingPreview) return false;
        
        if (show) {
            ItemTooltip.Instance.Open(craftingTarget);
            ItemTooltip.Instance.Move(GetGlobalPosition() + new Vector2(Size.X + 5, 0));
        } else {
            ItemTooltip.Instance.Close();
        }
        
        showingCraftingPreview = show;
        return true;
    }
}