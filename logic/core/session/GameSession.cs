using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.item;
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
        for (int i = 0; i < Players.Length; i++) {
            Player player = Players[i];
            player.Name = $"Player{i + 1}";
            AddChild(player);
        }
        Mode.Name = "GameMode";
        AddChild(Mode);
        UnitPool.Initialize(season);
    }

    public void Start() {
        Started = true;
        Mode.Start();
    }

    public override void _PhysicsProcess(double delta) {
        if (!Started) return;
        
        Mode.Tick(delta);
    }
    
    public Combat GetCombatForPlayer(Player player) {
        if (Mode.GetCurrentPhase() is not CombatPhase combatPhase) return null;
        return combatPhase.GetCombatForPlayer(player);
    }

    public bool IsInCombat(Player player) {
        return GetCombatForPlayer(player) != null;
    }

    public IEnumerable<Combat> GetCurrentCombats() {
        if (Mode.GetCurrentPhase() is not CombatPhase combatPhase) return null;
        return combatPhase.GetAllCombats();
    }

    public ItemConfig GetItemConfig() {
        return Season.GetItemConfig();
    }
}