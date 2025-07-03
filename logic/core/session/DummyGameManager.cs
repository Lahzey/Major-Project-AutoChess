using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.session;

[GlobalClass]
public partial class DummyGameManager : Node {
    
    [Export] public Season Season { get; set; }

    public static DummyGameManager Instance { get; private set; }

    public override void _EnterTree() {
        Instance = this;
    }

    public override void _Ready() {
        Start();
    }

    public void Start() {
        GameSession gameSession = new GameSession();
        gameSession.Mode = new EchoMode();
        AddChild(gameSession);
        
        Account[] accounts = new Account[8];
        for (int i = 0; i < 8; i++) {
            accounts[i] = new Account("Player " + (i + 1));
        }
        
        UnitPool.Initialize(Season);
        gameSession.Initialize(accounts);
    }
}