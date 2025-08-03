using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.environment;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
public partial class CombatPhase : GamePhase {

    private Dictionary<Player, Combat> combats = new Dictionary<Player, Combat>();

    public virtual void CreateCombats() {
        if (GameSession.Instance.Players.Length <= 1) {
            throw new System.InvalidOperationException("Cannot create combats with less than 2 players.");
        }

        for (int i = 0; i < GameSession.Instance.Players.Length; i+=2) {
            Player playerA = GameSession.Instance.Players[i];
            Player playerB = GameSession.Instance.Players[(i + 1) % GameSession.Instance.Players.Length];
            Combat combat = new Combat();
            combat.Prepare(playerA, playerB);
            AddChild(combat);
            combat.GlobalPosition = playerA.Arena.Board.GlobalPosition + new Vector2(playerA.Arena.Board.Columns * 0.5f, 0);
            
            // map players to their respective combat
            combats.Add(playerA, combat);
            if (i+1 < GameSession.Instance.Players.Length) { // if count is odd the last player fights a clone of the first player
                combats.Add(playerB, combat);
            }
        }
    }

    public override string GetName(Player forPlayer) {
        Combat combat = combats[forPlayer];
        Player otherPlayer = combat.PlayerA == forPlayer ? combat.PlayerB : combat.PlayerA;
        return $"Combat against {otherPlayer.Account.Name}";
    }
    
    public override int GetPowerLevel() {
        return 0;
    }
    
    public override void Start() {
        CreateCombats();
        if (ServerController.Instance.IsServer) return;
        
        SetBoardsVisible(false);
        Combat playerCombat = combats[PlayerController.Current.Player];
        CameraController.Instance.Cover(playerCombat.GlobalBounds);
    }

    public override bool IsFinished() {
        return false; // TODO determine when combats are finished
    }

    public override void End() {
        if (ServerController.Instance.IsServer) return;
        
        SetBoardsVisible(false);
        Arena arena = PlayerController.Current.Player.Arena;
        CameraController.Instance.Cover(new Rect2(arena.GlobalPosition, arena.ArenaSize));
    }

    private void SetBoardsVisible(bool visible) {
        foreach (Player player in GameSession.Instance.Players) {
            player.Arena.Board.Visible = visible;
        }
    }
}