using System;
using Godot;
using Godot.Collections;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.item;

public partial class InventoryPanel : GridContainer {

    [Export] public PackedScene ItemPanelScene { get; set; }
    public Player Player { get; set; }

    private uint currentSlotCount = 0;

    public override void _Process(double delta) {
        Inventory inventory = Player.Inventory;
        Array<Node> children = GetChildren();

        // remove slots if there are too many
        if (inventory.Size < currentSlotCount) {
            for (int i = children.Count - 1; i >= 0; i--) { // backwards looping makes removal easier
                if (children[i] is not ItemPanel itemPanel) continue;
                RemoveChild(itemPanel);
                itemPanel.QueueFree();
                currentSlotCount--;
                children.RemoveAt(i);
                if (currentSlotCount == inventory.Size) break;
            }
        } else if (inventory.Size > currentSlotCount) {
            for (uint i = currentSlotCount; i < inventory.Size; i++) {
                ItemPanel itemPanel = ItemPanelScene.Instantiate<ItemPanel>();
                AddChild(itemPanel);
                children.Add(itemPanel);
                currentSlotCount++;
            }
        }

        int itemsPerColumn = Math.Max(inventory.ExpansionInterval, 1);
        int columns = Mathf.CeilToInt(inventory.Size / (float) itemsPerColumn);
        Columns = columns;

        int column = 0;
        int row = 0;
        foreach (Node child in children) {
            if (child is not ItemPanel itemPanel) continue;
            itemPanel.Player = Player;
            itemPanel.InventoryIndex = row + column * itemsPerColumn;
            itemPanel.Name = "ItemPanel" + itemPanel.InventoryIndex;
            itemPanel.QueueRedraw();
            column++;
            if (column >= columns) {
                column = 0;
                row++;
            }
        }
    }
}