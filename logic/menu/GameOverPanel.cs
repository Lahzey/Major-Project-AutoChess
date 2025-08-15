using Godot;
using MPAutoChess.logic.core.networking;

namespace MPAutoChess.logic.menu;

public partial class GameOverPanel : Control {
    
    public static readonly PackedScene MAIN_MENU_SCENE = ResourceLoader.Load<PackedScene>("res://scenes/MainMenuScene.tscn");
    
    [Export] public Label TitleLabel { get; set; }
    [Export] public Button MainMenuButton { get; set; }
    
    public static GameOverPanel Instance { get; private set; }
    
    public override void _Ready() {
        Instance = this;
        Visible = false;
        
        MainMenuButton.Pressed += () => {
            ServerController.Instance.Disconnect();
            GetTree().ChangeSceneToPacked(MAIN_MENU_SCENE);
        };
    }
    
    public void ShowGameOver(int placement) {
        string placementSuffix = placement switch {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
        string placementTitle = placement == 1 ? "Victory" : "Game Over";
        TitleLabel.Text = placementTitle + "\n" + placement + placementSuffix + " Place";
        Visible = true;
    }
    
    
}