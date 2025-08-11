using System;
using Godot;

namespace MPAutoChess.logic.menu;

public partial class ContextMenu : Control {
    
    public static ContextMenu Instance { get; private set; }
    
    [Export] public PopupMenu PopupMenu { get; set; }
    
    private Action[] callbacks;
    
    private bool hideOnLeave = false;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        Instance = this;
        PopupMenu.IdPressed += id => {
            if (callbacks[id] != null) {
                callbacks[id]();
            }
        };
        PopupMenu.MouseExited += OnMouseLeave;
        PopupMenu.Hide();
    }
    
    private void OnMouseLeave() {
        if (hideOnLeave) {
            PopupMenu.Hide();
        }
    }

    public void ShowContextMenu(Vector2 position, ContextMenuItem[] items, AnchorPoint anchor = AnchorPoint.TOP_LEFT, bool hideOnLeave = false) {
        PopupMenu.Clear();
        callbacks = new Action[items.Length];
        for (int i = 0; i < items.Length; i++) {
            ContextMenuItem item = items[i];
            item.AddToContextMenu(PopupMenu);
            callbacks[i] = item.action;
        }
        this.hideOnLeave = hideOnLeave;

        PopupMenu.Position = (Vector2I) position;
        PopupMenu.ResetSize();
        
        switch (anchor) {
            case AnchorPoint.TOP_LEFT:
                // nothing to do, that is how godot positions elements already
                break;
            case AnchorPoint.TOP_RIGHT:
                PopupMenu.Position -= new Vector2I(PopupMenu.Size.X, 0);
                break;
            case AnchorPoint.BOTTOM_LEFT:
                PopupMenu.Position -= new Vector2I(0, PopupMenu.Size.Y);
                break;
            case AnchorPoint.BOTTOM_RIGHT:
                PopupMenu.Position -= new Vector2I(PopupMenu.Size.X, PopupMenu.Size.Y);
                break;
            case AnchorPoint.CENTER:
                PopupMenu.Position -= new Vector2I(PopupMenu.Size.X / 2, PopupMenu.Size.Y / 2);
                break;
        }
        
        PopupMenu.Show();
    }
    
    public void HideContextMenu() {
        PopupMenu.Hide();
    }
    
    public enum AnchorPoint {
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_LEFT,
        BOTTOM_RIGHT,
        CENTER,
    }
}

internal enum ContextMenuItemType {
    LABEL,
    CHECK,
    RADIO_CHECK,
    SUBMENU,
    SEPARATOR,
}

public struct ContextMenuItem {
    private ContextMenuItemType type;
    private string text;
    private Texture2D? icon;
    private PopupMenu? submenu;
    
    internal Action? action;
    
    private ContextMenuItem(ContextMenuItemType type, string text, Texture2D? icon, PopupMenu? submenu, Action action) {
        this.type = type;
        this.text = text;
        this.icon = icon;
        this.submenu = submenu;
        this.action = action;
    }
    
    public static ContextMenuItem Label(string text, Action action = null) {
        return new ContextMenuItem(ContextMenuItemType.LABEL, text, null, null, action);
    }
    
    public static ContextMenuItem Check(string text, Action action) {
        return new ContextMenuItem(ContextMenuItemType.CHECK, text, null, null, action);
    }
    
    public static ContextMenuItem RadioCheck(string text, Action action) {
        return new ContextMenuItem(ContextMenuItemType.RADIO_CHECK, text, null, null, action);
    }
    
    public static ContextMenuItem Submenu(string text, PopupMenu submenu) {
        return new ContextMenuItem(ContextMenuItemType.SUBMENU, text, null, submenu, null);
    }
    
    public static ContextMenuItem Separator(string text = "") {
        return new ContextMenuItem(ContextMenuItemType.SEPARATOR, text, null, null, null);
    }

    internal void AddToContextMenu(PopupMenu popupMenu) {
        switch (type) {
            case ContextMenuItemType.LABEL:
                if (icon != null) popupMenu.AddIconItem(icon, text);
                else popupMenu.AddItem(text);
                break;
            case ContextMenuItemType.CHECK:
                if (icon != null) popupMenu.AddIconCheckItem(icon, text);
                else popupMenu.AddCheckItem(text);
                break;
            case ContextMenuItemType.RADIO_CHECK:
                if (icon != null) popupMenu.AddIconRadioCheckItem(icon, text);
                else popupMenu.AddRadioCheckItem(text);
                break;
            case ContextMenuItemType.SUBMENU:
                popupMenu.AddSubmenuNodeItem(text, submenu);
                break;
            case ContextMenuItemType.SEPARATOR:
                popupMenu.AddSeparator(text);
                break;
        }
    }
}