using System;
using Godot;

namespace MPAutoChess.logic.menu;

public partial class ContextMenu : Control {
    
    public static ContextMenu Instance { get; private set; }
    
    [Export] public PopupMenu PopupMenu { get; set; }
    
    private Action[] callbacks;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        Instance = this;
        PopupMenu.IdPressed += id => {
            if (callbacks[id] != null) {
                callbacks[id]();
            }
        };
        PopupMenu.Hide();
    }

    public void ShowContextMenu(Vector2 position, ContextMenuItem[] items) {
        PopupMenu.Clear();
        callbacks = new Action[items.Length];
        for (int i = 0; i < items.Length; i++) {
            ContextMenuItem item = items[i];
            item.AddToContextMenu(PopupMenu);
            callbacks[i] = item.action;
        }

        PopupMenu.Position = (Vector2I) position;
        PopupMenu.ResetSize();
        PopupMenu.Show();
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
    
    internal Action action;
    
    private ContextMenuItem(ContextMenuItemType type, string text, Texture2D? icon, PopupMenu? submenu, Action action) {
        this.type = type;
        this.text = text;
        this.icon = icon;
        this.submenu = submenu;
        this.action = action;
    }
    
    public static ContextMenuItem Label(string text, Action action) {
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