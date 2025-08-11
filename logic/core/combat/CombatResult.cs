using System.Linq;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.session;
using ProtoBuf;

namespace MPAutoChess.logic.core.combat;

[ProtoContract]
public class CombatResult {
    
    [ProtoMember(1)] public long PlayerAId { get; set; } // Account ID of player A (not serializing the whole Player because reference tracking is not working and this object needs to stay slim)
    [ProtoMember(2)] public long PlayerBId { get; set; } // Account ID of player B (not serializing the whole Player because reference tracking is not working and this object needs to stay slim)
    [ProtoMember(3)] public Winner Winner { get; set; }
    [ProtoMember(4)] public int SurvivingUnits { get; set; } // number of units the winner has left
    [ProtoMember(5)] public int DamageDealt { get; set; } // damage dealt to the loser, or both if a draw

    private Player playerA;
    public Player PlayerA {
        get {
            if (playerA != null) return playerA;
            playerA = GameSession.Instance.Players.FirstOrDefault(player => player.Account.Id == PlayerAId);
            return playerA;
        }
    }
    
    private Player playerB;
    public Player PlayerB {
        get {
            if (playerB != null) return playerB;
            playerB = GameSession.Instance.Players.FirstOrDefault(player => player.Account.Id == PlayerBId);
            return playerB;
        }
    }
}

public enum Winner {
    DRAW = 0,
    PLAYER_A = 1,
    PLAYER_B = 2
}