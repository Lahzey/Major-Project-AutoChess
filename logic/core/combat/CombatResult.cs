using MPAutoChess.logic.core.player;
using ProtoBuf;

namespace MPAutoChess.logic.core.combat;

[ProtoContract]
public class CombatResult {
    
    [ProtoMember(1)] public long PlayerAId { get; set; } // Account ID of player A
    [ProtoMember(2)] public long PlayerBId { get; set; } // Account ID of player B
    [ProtoMember(3)] public Winner Winner { get; set; }
    [ProtoMember(4)] public int SurvivingUnits { get; set; } // number of units the winner has left
    [ProtoMember(5)] public int DamageDealt { get; set; } // damage dealt to the loser, or both if a draw
    
}

public enum Winner {
    DRAW = 0,
    PLAYER_A = 1,
    PLAYER_B = 2
}