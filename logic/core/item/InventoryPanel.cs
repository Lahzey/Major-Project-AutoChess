using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.item.consumable;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.item;

public partial class InventoryPanel : Container {
    
    private static readonly List<Consumable> ALWAYS_VISIBLE_CONSUMABLES = new List<Consumable> {
        Consumable.Get<ItemRemover>(),
        Consumable.Get<ItemReroll>()
    };
    
    [Export] public int ConsumablesPerColumn { get; set; } = 2;
    [Export] public int ItemsPerColumn { get; set; } = 10;
    [Export] public Container ColumnsContainer { get; set; }
    [Export] public PackedScene ConsumableButtonScene { get; set; }
    [Export] public PackedScene ItemPanelScene { get; set; }
    
    public Player Player { get; set; }

    private List<VBoxContainer> columns = new List<VBoxContainer>();
    private List<ConsumableButton> consumableButtons = new List<ConsumableButton>();
    private List<ItemPanel> itemPanels = new List<ItemPanel>();

    public override void _Process(double delta) {
        Inventory inventory = Player.Inventory;
        List<Consumable> consumables = Consumable.GetAll().Where(c => Player.GetConsumableCount(c) > 0 || ALWAYS_VISIBLE_CONSUMABLES.Contains(c)).ToList();

        if (consumables.Count != consumableButtons.Count || inventory.Size != itemPanels.Count) {
            Rebuild(inventory, consumables);
        }


        for (int i = 0; i < itemPanels.Count; i++) {
            ItemPanel itemPanel = itemPanels[i];
            itemPanel.Player = Player;
            itemPanel.InventoryIndex = i;
            itemPanel.Name = "ItemPanel" + itemPanel.InventoryIndex;
            itemPanel.QueueRedraw();
        }
        for (int i = 0; i < consumableButtons.Count; i++) {
            ConsumableButton consumableButton = consumableButtons[i];
            consumableButton.SetConsumable(consumables[i]);
            consumableButton.QueueRedraw();
        }
    }

    private void Rebuild(Inventory inventory, List<Consumable> consumables) {
        foreach (VBoxContainer column in columns) {
            column.QueueFree();
        }
        columns.Clear();
        consumableButtons.Clear();
        itemPanels.Clear();

        int requiredColumnsForConsumables = Mathf.CeilToInt(consumables.Count / (float)ConsumablesPerColumn);
        int requiredColumnsForItems = Mathf.CeilToInt(inventory.Size / (float)ItemsPerColumn);
        int totalColumns = Math.Max(requiredColumnsForConsumables, requiredColumnsForItems);

        for (int columnIndex = 0; columnIndex < totalColumns; columnIndex++) {
            VBoxContainer column = new VBoxContainer();
            for (int i = consumableButtons.Count; i < (columnIndex + 1) * ConsumablesPerColumn; i++) {
                if (i >= consumables.Count) {
                    TextureRect filler = new TextureRect();
                    filler.SizeFlagsVertical = SizeFlags.ExpandFill;
                    column.AddChild(filler);
                    continue;
                }
                
                ConsumableButton button = (ConsumableButton) ConsumableButtonScene.Instantiate();
                button.SizeFlagsVertical = SizeFlags.ExpandFill;
                column.AddChild(button);
                consumableButtons.Add(button);
            }
            for (int i = itemPanels.Count; i < (columnIndex + 1) * ItemsPerColumn; i++) {
                if (i >= inventory.Size) break;
                
                ItemPanel itemPanel = (ItemPanel) ItemPanelScene.Instantiate();
                itemPanel.SizeFlagsVertical = SizeFlags.ExpandFill;
                column.AddChild(itemPanel);
                itemPanels.Add(itemPanel);
            }
            ColumnsContainer.AddChild(column);
            columns.Add(column);
        }
    }
}