using System;
using Godot;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.session;

public partial class GameSession : Node {
    
    public static GameSession Instance { get; private set; }

    public GameMode Mode { get; set; }

    public Player[] Players { get; private set; }
    
    public Random Random { get; private set; } = new  Random();

    public override void _EnterTree() {
        Instance = this;
    }

    public void Initialize(Account[] accounts) {
        Players = new Player[accounts.Length];
        for (int i = 0; i < accounts.Length; i++) {
            Players[i] = new Player();
            Players[i].Account = accounts[i];
        }
    }

    public override void _PhysicsProcess(double delta) {
        Mode.Tick(delta);
    }
}