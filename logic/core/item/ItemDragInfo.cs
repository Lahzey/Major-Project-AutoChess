using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.item;

public partial class ItemDragInfo : GodotObject { // needs to be a GodotObject to be used as drag data
    public GodotObject Source { get; private set; }
    public Player OwningPlayer { get; private set; }
    public int InventoryIndex { get; private set; }
    
    public ItemDragInfo(GodotObject source, Player owningPlayer, int inventoryIndex) {
        Source = source;
        OwningPlayer = owningPlayer;
        InventoryIndex = inventoryIndex;
    }
    
    public Inventory GetInventory() {
        return OwningPlayer.Inventory;
    }
    
    public Item GetItem() {
        return GetInventory().GetItem(InventoryIndex);
    }
}