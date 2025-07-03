using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.shop;

[Tool]
public partial class ShopUI : Control {
    
    [Export] public Button RerollButton { get; set; }
    [Export] public Button XpButton { get; set; }
    [Export] public Label GoldLabel { get; set; }
    [Export] public Container ShopSlotContainer { get; set; }
    [Export] public PackedScene ShopSlotScene { get; set; }
    
    // to test how it will look in the editor
    private uint debugSlotCount = 0;
    [Export(PropertyHint.Range, "0,20")] public uint DebugSlotCount {
        get => debugSlotCount;
        set {
            debugSlotCount = Math.Min(value, 20); // arrays cannot hold uint max value, or even int max value, this should be more than enough
            if (Engine.IsEditorHint()) AddOffers(new ShopOffer[debugSlotCount]);
        }
    }

    private ShopSlot[] ShopSlots;

    public override void _Ready() {
        List<ShopSlot> slots = new List<ShopSlot>();
        foreach (Node child in ShopSlotContainer.GetChildren()) {
            if (child is ShopSlot slot) {
                slots.Add(slot);
            }
        }
        ShopSlots = slots.ToArray();
        RerollButton.Pressed += () => PlayerController.Instance.RerollShop(); // () => instead of adding the method directly to ensure Instance reference is checked each time
    }

    public override void _Process(double delta) {
        GoldLabel.Text = PlayerController.Instance?.CurrentPlayer?.Gold.ToString() ?? "n/a";
    }
    
    public void AddOffers(ShopOffer[] offers) {
        List<ShopSlot> slots = new List<ShopSlot>();
        foreach (Node child in ShopSlotContainer.GetChildren()) {
            if (child is ShopSlot slot) {
                slots.Add(slot);
            }
        }
        if (slots.Count > offers.Length) {
            // remove excess slots
            for (int i = slots.Count - 1; i >= offers.Length; i--) {
                slots[i].QueueFree();
                slots.RemoveAt(i);
            }
        } else if (slots.Count < offers.Length) {
            // add new slots
            for (int i = slots.Count; i < offers.Length; i++) {
                ShopSlot slot = (ShopSlot) ShopSlotScene.Instantiate();
                slot.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                ShopSlotContainer.AddChild(slot);
                slots.Add(slot);
            }
        }

        for (int i = 0; i < offers.Length; i++) {
            ShopOffer offer = offers[i];
            offer?.FillShopSlot(slots[i]); // only null during editor time for testing the looks
        }
    }
    
}