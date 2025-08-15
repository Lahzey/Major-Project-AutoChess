using Godot;

namespace MPAutoChess.logic.menu;

public partial class Tutorial : Control {
    
    [Export] public Button BackButton { get; set; }
    
    public override void _Ready() {
        BackButton.Pressed += () => {
            GetTree().ChangeSceneToPacked(ResourceLoader.Load<PackedScene>("res://scenes/MainMenuScene.tscn"));
        };
    }
    
}