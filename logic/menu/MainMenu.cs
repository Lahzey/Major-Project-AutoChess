using Godot;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.menu;

public partial class MainMenu : Control {
    
    [Export] public Button QuitButton { get; set; }
    [Export] public Button TutorialButton { get; set; }
    
    [Export] public LineEdit PlayerNameInput { get; set; }
    [Export] public Button ConnectButton { get; set; }
    [Export] public Button StartGameButton { get; set; }
    [Export] public LobbyController LobbyController { get; set; }
    [Export] public PackedScene InGameScene { get; set; }

    private bool connecting = false;
    private bool connected = false;
    
    private bool connectingToQueue = false;
    private bool inQueue = false;
    
    public override void _Ready() {
        ReevaluateDisabled();
        
        QuitButton.Pressed += () => {
            GetTree().Quit();
        };
        
        TutorialButton.Pressed += () => {
            GetTree().ChangeSceneToPacked(ResourceLoader.Load<PackedScene>("res://scenes/TutorialScene.tscn"));
        };

        PlayerNameInput.TextChanged += text => {
            ReevaluateDisabled();
        };
        ConnectButton.Pressed += Connect;
        StartGameButton.Pressed += JoinQueue;
        
        LobbyController.OnConnected += () => {
            connected = true;
            connecting = false;
            ReevaluateDisabled();
            Account.SetCurrent(new Account(LobbyController.AccountId, LobbyController.AccountName, LobbyController.Secret));
            GD.Print("Connected to lobby!");
        };

        LobbyController.OnQueueEntered += () => {
            connectingToQueue = false;
            inQueue = true;
            ReevaluateDisabled();
        };
        
        LobbyController.OnQueueLeft += () => {
            connectingToQueue = false;
            inQueue = false;
            ReevaluateDisabled();
        };
        
        LobbyController.OnGameStart += StartGame;
    }

    private void Connect() {
        if (string.IsNullOrEmpty(PlayerNameInput.Text)) return;
            
        LobbyController.Connect(PlayerNameInput.Text);
        connecting = true;
        ReevaluateDisabled();
    }
    
    private void JoinQueue() {
        if (!connected) return;

        connectingToQueue = true;
        
        if (inQueue) {
            LobbyController.LeaveQueue();
            inQueue = false;
        } else {
            LobbyController.EnterQueue();
            inQueue = true;
        }
        ReevaluateDisabled();
    }

    private void StartGame(int port) {
        ServerController.SERVER_PORT = port;
        GetTree().ChangeSceneToPacked(InGameScene);
        
        GD.Print($"Starting game on port {port}");
    }

    private void ReevaluateDisabled() {
        PlayerNameInput.Editable = !connecting && !connected;
        ConnectButton.Disabled = connecting || connected || string.IsNullOrEmpty(PlayerNameInput.Text);
        ConnectButton.Text = connected ? "Connected" : (connecting ? "Connecting..." : "Connect");
        StartGameButton.Disabled = !connected || connectingToQueue;
        StartGameButton.Text = connectingToQueue ? "Connecting to Queue..." : inQueue ? "Leave Queue" : "Start Game";
    }
    
}