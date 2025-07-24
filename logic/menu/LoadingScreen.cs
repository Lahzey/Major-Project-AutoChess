using Godot;

namespace MPAutoChess.logic.menu;

public partial class LoadingScreen : CanvasLayer {
    
    [Export] public Label MessageLabel;
    
    public static LoadingScreen Instance { get; private set; }

    public override void _EnterTree() {
        Instance = this;
    }
    
    public void SetStage(LoadingStage stage) {
        switch (stage) {
            case LoadingStage.CONNECTING:
                MessageLabel.Text = "Connecting";
                break;
            case LoadingStage.LOADING_GAME:
                MessageLabel.Text = "Loading Game";
                break;
            case LoadingStage.WAITING_FOR_PLAYERS:
                MessageLabel.Text = "Waiting for Players";
                break;
            case LoadingStage.STARTING:
                MessageLabel.Text = "Starting Game";
                break;
            case LoadingStage.STARTED:
                MessageLabel.Text = "";
                SetVisible(false);
                break;
            default:
                MessageLabel.Text = "Unknown loading stage";
                break;
        }
    }
}

public enum LoadingStage {
    CONNECTING,
    LOADING_GAME,
    WAITING_FOR_PLAYERS,
    STARTING,
    STARTED
}