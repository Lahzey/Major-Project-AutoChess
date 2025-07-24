using System;
using Godot;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
public partial class GameSession : Node {
    
    public static GameSession Instance { get; private set; }
    
    [ProtoMember(1)] public Season Season { get; set; }
    
    [ProtoMember(2)] public GameMode Mode { get; set; }

    [ProtoMember(3)] public Player[] Players { get; private set; }

    [ProtoMember(4)] public bool Started { get; private set; } = false;
    
    public Random Random { get; private set; } = new  Random();

    public override void _EnterTree() {
        Instance = this;
    }

    public void Initialize(Season season, GameMode gameMode, Player[] players) {
        Season = season;
        Mode = gameMode;
        Players = players;
        UnitPool.Initialize(season);
    }

    public void Start() {
        Started = true;
    }

    public override void _PhysicsProcess(double delta) {
        if (!Started) return;
        Mode.Tick(delta);
    }

    public bool IsInCombat(Player player) {
        return false; // TODO
    }
}

[ProtoContract]
public partial class NodeTest : Node {
    public string Id { get; set; }
    
    [ProtoMember(1)] public Player Child { get; set; }
    [ProtoMember(2)] public int Value { get; set; }

    public static NodeTest Create() {
        NodeTest test = new NodeTest();
        test.Child = new Player();
        test.Child.Account = new Account(1, "TestPlayer");
        test.Child.Gold = 99999;
        test.Child.Name = "TestPlayer";
        test.Value = 50;
        ServerController.Instance.AddChild(test);
        test.AddChild(test.Child);
        return test;
    }

    public override string ToString() {
        return $"NodeTest[Value={Value} | Gold={Child?.Gold.ToString() ?? "null"}]";
    }
}