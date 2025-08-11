using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.shop;

public partial class ShopUI : Control {
    
    private static readonly Color PLAYER_FULL_HEALTH_COLOR = new Color("#114721");
    private static readonly Color PLAYER_LOW_HEALTH_COLOR = new Color("#4a0108");
    
    [Export] public BaseButton RerollButton { get; set; }
    [Export] public Label RerollCost { get; set; }
    [Export] public BaseButton XpButton { get; set; }
    [Export] public Label XpCost { get; set; }
    [Export] public BaseButton FiveXpToggle { get; set; }
    [Export] public Label GoldLabel { get; set; }
    [Export] public ProgressBar PlayerXpBar { get; set; }
    [Export] public Label PlayerLevelLabel { get; set; }
    
    [Export] public TextureProgressBar PlayerHealthBar { get; set; }
    
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
        XpButton.Pressed += () => PlayerController.Current.BuyXp(GetXpAmount());
    }

    private int GetXpAmount() {
        return FiveXpToggle.IsPressed() ? 5 : 1;
    }

    public override void _Process(double delta) {
        if (PlayerController.Current == null) return;
        Player player = PlayerController.Current.Player;
        GoldLabel.Text = player.Gold.ToString() ?? "n/a";
        XpButton.Disabled = player.Gold < GetXpAmount() * Player.COST_PER_XP;
        RerollButton.Disabled = player.Gold < Player.COST_PER_REROLL && player.FreeRerolls <= 0;
        RerollCost.Text = player.FreeRerolls > 0 ? $"({player.FreeRerolls}) Free" : Player.COST_PER_REROLL.ToString();
        XpCost.Text = (GetXpAmount() * Player.COST_PER_XP).ToString();

        PlayerXpBar.MinValue = player.GetXpForLevel(player.Level);
        PlayerXpBar.MaxValue = player.GetXpForLevel(player.Level + 1);
        PlayerXpBar.Value = player.Experience;
        PlayerLevelLabel.Text = "LVL " + player.Level;
        
        PlayerHealthBar.MaxValue = player.MaxHealth;
        PlayerHealthBar.Value = player.CurrentHealth;
        PlayerHealthBar.Modulate = PLAYER_LOW_HEALTH_COLOR.Lerp(PLAYER_FULL_HEALTH_COLOR, player.MaxHealth / (float) player.CurrentHealth);
    }
    
    public void SetOffers(ShopOffer[] offers) {
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