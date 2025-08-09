using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.menu;

namespace MPAutoChess.logic.core.item.consumable;

public partial class ConsumableButton : TextureButton {
    
    [Export] public Label CountLabel { get; set; }
    
    public Consumable Consumable { get; private set; }

    public ConsumableButton() {
        Pressed += OnClick;
    }

    private void OnClick() {
        if (Consumable == null) return;
        Consumable capturedConsumable = Consumable;
        Action<Node> onSelect = (node) => {
            if (node != null) capturedConsumable.OnUse(node);
        };
        SelectionLayer.Instance.Select(capturedConsumable.CanBeUsedOn, onSelect, capturedConsumable.GetValidTargetCursor(), capturedConsumable.GetInvalidTargetCursor());
    }

    public void SetConsumable(Consumable consumable) {
        Consumable = consumable;
        if (consumable == null) {
            TextureNormal = null;
            return;
        }
        TextureNormal = consumable.GetIcon();
        TooltipText = consumable.GetName();
    }

    public override void _Process(double delta) {
        if (Consumable == null) return;
        if (PlayerController.Current?.Player == null) return;

        uint count = PlayerController.Current.Player.GetConsumableCount(Consumable);
        CountLabel.Text = count.ToString();
        Disabled = count == 0;
        Modulate = Disabled ? new Color(0.2f, 0.2f, 0.2f) : Colors.White;
    }
}