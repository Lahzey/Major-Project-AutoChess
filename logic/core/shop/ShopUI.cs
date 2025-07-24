using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.shop;

public partial class ShopUI : Control {
    
    [Export] public BaseButton RerollButton { get; set; }
    [Export] public BaseButton XpButton { get; set; }
    [Export] public Label GoldLabel { get; set; }
    [Export] public Container ShopSlotContainer { get; set; }
    [Export] public PackedScene ShopSlotScene { get; set; }

    private ShopSlot[] ShopSlots;

    public override void _Ready() {
        List<ShopSlot> slots = new List<ShopSlot>();
        foreach (Node child in ShopSlotContainer.GetChildren()) {
            if (child is ShopSlot slot) {
                slots.Add(slot);
            }
        }
        ShopSlots = slots.ToArray();
        RerollButton.Pressed += () => PlayerController.Current.RerollShop(); // () => instead of adding the method directly to ensure Instance reference is checked each time
        XpButton.Pressed += () => PlayerController.Current.BuyXp();
    }

    public override void _Process(double delta) {
        GoldLabel.Text = PlayerController.Current?.Player?.Gold.ToString() ?? "n/a";
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