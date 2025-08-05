using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Godot;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.environment;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
public partial class CombatPhase : GamePhase {

    [ProtoMember(1)] private List<Combat> combats = new List<Combat>();
    [ProtoMember(2)] private Dictionary<long, int> playerCombatIndices = new Dictionary<long, int>();

    public CombatPhase() { } // for Protobuf serialization

    public CombatPhase(IEnumerable<Player> participants) {
        CreateCombats(participants);
    }

    public void CreateCombats(IEnumerable<Player> players) {
        if (GameSession.Instance.Players.Length <= 1) {
            throw new System.InvalidOperationException("Cannot create combats with less than 2 players.");
        }

        for (int i = 0; i < GameSession.Instance.Players.Length; i+=2) {
            Player playerA = GameSession.Instance.Players[i];
            Player playerB = GameSession.Instance.Players[(i + 1) % GameSession.Instance.Players.Length];
            int combatIndex = combats.Count;
            bool isCloneFight = (i + 1) >= GameSession.Instance.Players.Length;
            
            Combat combat = new Combat();
            combat.Prepare(playerA, playerB, isCloneFight);
            AddChild(combat);
            combat.GlobalPosition = playerA.Arena.Board.GlobalPosition;
            
            combat.Name = "Combat" + combatIndex;
            combats.Add(combat);
            
            // map players to their respective combat
            playerCombatIndices.Add(playerA.Account.Id, combatIndex);
            if (!isCloneFight) playerCombatIndices.Add(playerB.Account.Id, combatIndex);
        }
    }

    public override string GetTitle(Player forPlayer) {
        Combat combat = combats[playerCombatIndices[forPlayer.Account.Id]];
        Player otherPlayer = combat.PlayerA == forPlayer ? combat.PlayerB : combat.PlayerA;
        return $"Combat against {otherPlayer.Account.Name}";
    }
    
    public override int GetPowerLevel() {
        return 0;
    }
    
    public override void Start() {
        foreach (Combat combat in combats) {
            combat.Start();
        }
        if (ServerController.Instance.IsServer) return;
        
        new Thread(() => {
            Thread.Sleep(500);
            CallDeferred(MethodName.SetupLocal);
        }).Start();
    }

    private void SetupLocal() {
        SetBoardsVisible(false);
        Combat playerCombat = combats[playerCombatIndices[PlayerController.Current.Player.Account.Id]];
        CameraController.Instance.Cover(playerCombat.GlobalBounds);
    }

    public override bool IsFinished() {
        return false; // TODO determine when combats are finished
    }

    public override void End() {
        if (!ServerController.Instance.IsServer) {
            SetBoardsVisible(false);
            Arena arena = PlayerController.Current.Player.Arena;
            CameraController.Instance.Cover(new Rect2(arena.GlobalPosition, arena.ArenaSize));
        }
    }

    private void SetBoardsVisible(bool visible) {
        foreach (Player player in GameSession.Instance.Players) {
            player.Arena.Board.Visible = visible;
        }
    }
}