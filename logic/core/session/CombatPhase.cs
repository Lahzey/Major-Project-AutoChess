using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Godot;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.environment;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
public partial class CombatPhase : GamePhase {
    private const double BOOST_DELAY = 20;
    private const double BOOST_INTERVAL = 5;
    private const string BOOST_STAT_ID = "COMBAT_OVERTIME_BOOST";
    private const double AFTER_COMBAT_TIME = 5;

    [ProtoMember(1)] private List<Combat> combats;
    [ProtoMember(2)] private Dictionary<long, int> playerCombatIndices = new Dictionary<long, int>();
    [ProtoMember(3)] private bool finished = false;

    private int boostCounter = 0;

    public override void _Process(double delta) {
        if (combats == null) {
            RemainingTime -= delta;
            if (RemainingTime <= 0 && ServerController.Instance.IsServer) {
                CreateCombats();
                ServerController.Instance.PublishChange(this);
                Rpc(MethodName.StartCombats);
            }
        } else if (!finished) {
            RemainingTime += delta;
            double boostTime = RemainingTime - BOOST_DELAY;
            int boostCounter = boostTime >= 0 ? (1 + (int)(boostTime / BOOST_INTERVAL)) : 0;

            if (ServerController.Instance.IsServer && boostCounter != this.boostCounter) {
               foreach (Combat combat in combats) {
                    foreach (UnitInstance unit in combat.GetAllUnits()) {
                        unit.Stats.GetCalculation(StatType.BONUS_ATTACK_SPEED).AddPostMult(1f + boostCounter * 0.5f, BOOST_STAT_ID);
                        unit.Stats.GetCalculation(StatType.STRENGTH).AddPostMult(1f + boostCounter * 0.5f, BOOST_STAT_ID);
                        unit.Stats.GetCalculation(StatType.MAGIC).AddPostMult(1f + boostCounter * 0.5f, BOOST_STAT_ID);
                        unit.Stats.GetCalculation(StatType.ARMOR).AddPostMult(1f / (boostCounter + 1), BOOST_STAT_ID);
                        unit.Stats.GetCalculation(StatType.AEGIS).AddPostMult(1f / (boostCounter + 1), BOOST_STAT_ID);
                    }
               }
            }
            this.boostCounter = boostCounter;

            if (ServerController.Instance.IsServer) {
                finished = !combats.Any(combat => !combat.IsFinished());
                if (finished) {
                    RemainingTime = AFTER_COMBAT_TIME;
                    ServerController.Instance.PublishChange(this);
                }
            }
        } else {
            RemainingTime -= delta;
        }
    }
    
    public override void Start() {
        RemainingTime = 30;
    }

    public override bool IsFinished() {
        return finished && RemainingTime <= 0;
    }

    public override void End() {
        if (!ServerController.Instance.IsServer) {
            SetBoardsVisible(true);
            Arena arena = PlayerController.Current.Player.Arena;
            CameraController.Instance.Cover(new Rect2(arena.GlobalPosition, arena.ArenaSize));
        }
    }

    public void CreateCombats() {
        if (GameSession.Instance.Players.Length <= 1) {
            throw new System.InvalidOperationException("Cannot create combats with less than 2 players.");
        }

        combats = new List<Combat>();
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
        if (combats == null) return "Prepare for Combat";
        Combat combat = combats[playerCombatIndices[forPlayer.Account.Id]];
        Player otherPlayer = combat.PlayerA == forPlayer ? combat.PlayerB : combat.PlayerA;
        return $"Combat against {otherPlayer.Account.Name}";
    }
    
    public override int GetPowerLevel() {
        return 0;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void StartCombats() {
        RemainingTime = 0;
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

    private void SetBoardsVisible(bool visible) {
        foreach (Player player in GameSession.Instance.Players) {
            player.Arena.Board.Visible = visible;
        }
    }
}