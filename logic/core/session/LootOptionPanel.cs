using System;
using Godot;

namespace MPAutoChess.logic.core.session;

public partial class LootOptionPanel : Control {
    
    private static readonly Color DISABLED_COLOR = new Color(1f, 0.5f, 0.5f, 1.0f);
    
    [Export] public TextureButton ChooseButton { get; set; }
    [Export] public TextureRect Texture { get; set; }
    [Export] public Label Label { get; set; }
    
    public Func<bool> IsEnabled { get; set; }


    public override void _Process(double delta) {
        if (IsEnabled != null) {
            bool enabled = IsEnabled();
            ChooseButton.Disabled = !enabled;
            Texture.Modulate = enabled ? Colors.White : DISABLED_COLOR;
        }
    }
}