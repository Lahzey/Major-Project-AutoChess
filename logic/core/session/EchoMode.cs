using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
public partial class EchoMode : GameMode {
    
    private List<Player> readyPlayers = new List<Player>();
    
    public override void Tick(double delta) {
        
    }

    public override double GetDefaultPhaseTime() {
        return double.PositiveInfinity;
    }

    protected override GamePhase GetNextPhase() {
        int nextPhaseIndex = GetCurrentPhaseIndex() + 1;
        if (nextPhaseIndex % 2 == 0) {
            return LootPhase.Random();
        } else {
            return new EchoCombatPhase();
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void RequestNextRound() {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("RequestNextRound can only be called on the server.");

        Player player = PlayerController.Current.Player;
        if (!readyPlayers.Contains(player)) {
            readyPlayers.Add(player);
        }

        if (readyPlayers.Count == GameSession.Instance.AlivePlayers.Count()) {
            AdvancePhase();
        }
    }
}

[ProtoContract]
public partial class EchoCombatPhase : GamePhase {
    
    public override string GetTitle(Player forPlayer) {
        return "Combat against Echo";
    }
    public override int GetPowerLevel() {
        return 0;
    }

    public override void Start() {
        
    }

    public override bool IsFinished() {
        return false;
    }

    public override void End() {
        
    }
}