using System;
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
using MPAutoChess.logic.util;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
public partial class CombatPhase : GamePhase {
    private const double BOOST_DELAY = 30; // if combat reaches 30secs we start boosting
    private const double BOOST_INTERVAL = 10; // further boosts every 10 seconds after that
    private const string BOOST_STAT_ID = "COMBAT_OVERTIME_BOOST";
    private const double TIE_DELAY = 70; // if combat reaches 70secs it is considered a tie
    
    private const double AFTER_COMBAT_TIME = 5;
    
    private static readonly Texture2D ICON = ResourceLoader.Load<Texture2D>("res://assets/ui/phases/combat.png");

    [ProtoMember(1)] public List<Combat> Combats { get; private set; }
    [ProtoMember(2)] private List<CombatResult> combatResults;
    [ProtoMember(3)] private Dictionary<long, int> playerCombatIndices = new Dictionary<long, int>();
    [ProtoMember(4)] private bool finished = false;

    private int boostCounter = 0;

    public override void _Process(double delta) {
        if (Combats == null) {
            RemainingTime -= delta;
            if (RemainingTime <= 0 && ServerController.Instance.IsServer) {
                CreateCombats();
                ServerController.Instance.PublishChange(this);
                Rpc(MethodName.StartCombats);
            }
        } else if (!finished) {
            RemainingTime += delta; // remaining time is used as combat time, it ticks up here

            if (RemainingTime >= TIE_DELAY) {
                foreach (Combat combat in Combats) combat.EndCombat();
            }
            
            double boostTime = RemainingTime - BOOST_DELAY;
            int boostCounter = boostTime >= 0 ? (1 + (int)(boostTime / BOOST_INTERVAL)) : 0;

            if (ServerController.Instance.IsServer && boostCounter != this.boostCounter) {
               foreach (Combat combat in Combats) {
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
                finished = Combats.All(combat => combat.IsFinished());
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
        if (ServerController.Instance.IsServer) {
            combatResults = new List<CombatResult>();
            foreach (Combat combat in Combats) {
                combatResults.Add(combat.Result);
                combat.QueueFree();
            }
            Combats.Clear();
            Combats = null;
            ServerController.Instance.PublishChange(this);
        } else {
            SetBoardsVisible(true);
            PlayerController.Current.GoToArena(PlayerController.Current.Player.Arena);
        }
    }

    public void CreateCombats() {
        List<Player> alivePlayers = GameSession.Instance.AlivePlayers.ToList();
        if (alivePlayers.Count <= 1) {
            throw new System.InvalidOperationException("Cannot create combats with less than 2 players.");
        }
        alivePlayers.Shuffle(GameSession.Instance.Random);

        Combats = new List<Combat>();
        for (int i = 0; i < alivePlayers.Count; i+=2) {
            Player playerA = alivePlayers[i];
            Player playerB = alivePlayers[(i + 1) % alivePlayers.Count];
            int combatIndex = Combats.Count;
            bool isCloneFight = (i + 1) >= alivePlayers.Count;
            
            Combat combat = new Combat();
            combat.Prepare(playerA, playerB, isCloneFight);
            AddChild(combat);
            combat.GlobalPosition = playerA.Arena.Board.GlobalPosition + new Vector2(playerA.Arena.Board.Columns * 0.5f, 0f);
            
            combat.Name = "Combat" + combatIndex;
            Combats.Add(combat);
            
            // map players to their respective combat
            playerCombatIndices.Add(playerA.Account.Id, combatIndex);
            if (!isCloneFight) playerCombatIndices.Add(playerB.Account.Id, combatIndex);
        }
    }
    
    public Combat GetCombatForPlayer(Player player) {
        if (Combats == null || Combats.Count == 0 || !playerCombatIndices.TryGetValue(player.Account.Id, out int combatIndex)) {
            return null;
        }
        return Combats[combatIndex];
    }
    
    public CombatResult GetCombatResultForPlayer(Player player) {
        if (combatResults == null || combatResults.Count == 0 || !playerCombatIndices.TryGetValue(player.Account.Id, out int combatIndex)) {
            return null;
        }
        return combatResults[combatIndex];
    }

    public IEnumerable<Combat> GetAllCombats() {
        return Combats;
    }

    public override string GetTitle(Player forPlayer) {
        Combat combat = GetCombatForPlayer(forPlayer);
        CombatResult combatResult = GetCombatResultForPlayer(forPlayer);
        if (combat == null && combatResult == null) return "Prepare for Combat";
        else if (combatResult == null) {
            Player otherPlayer = combat.PlayerA == forPlayer ? combat.PlayerB : combat.PlayerA;
            return $"Combat against {otherPlayer.Account.Name}";
        } else {
            Player otherPlayer = combatResult.PlayerA == forPlayer ? combatResult.PlayerB : combatResult.PlayerA;
            return $"Combat against {otherPlayer.Account.Name}";
        }
    }

    public override Texture2D GetIcon(Player forPlayer, out Color modulate) {
        modulate = DEFAULT_ICON_MODULATE;
        CombatResult combatResult = GetCombatResultForPlayer(forPlayer);
        if (combatResult != null) {
            bool wasWinner = combatResult.Winner switch {
                Winner.PLAYER_A => combatResult.PlayerAId == forPlayer.Account.Id,
                Winner.PLAYER_B => combatResult.PlayerBId == forPlayer.Account.Id,
                _ => false
            };
            modulate = wasWinner ? new Color("#5a7548") : new Color("#7d3939");
        }
        return ICON;
    }

    public override int GetPowerLevel() {
        return 0;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void StartCombats() {
        RemainingTime = 0;
        foreach (Combat combat in Combats) {
            combat.Start();
        }
        if (ServerController.Instance.IsServer) return;
        
        SetupLocal();
    }

    private void SetupLocal() {
        SetBoardsVisible(false);
        Combat playerCombat = GetCombatForPlayer(PlayerController.Current.Player);
        if (playerCombat == null) return;
        
        bool isPlayerA = playerCombat.PlayerA == PlayerController.Current.Player;
        PlayerController.Current.GoToArena(playerCombat.PlayerA.Arena);
        playerCombat.Rotation = isPlayerA ? 0 : Mathf.Pi;
    }

    private void SetBoardsVisible(bool visible) {
        foreach (Player player in GameSession.Instance.AlivePlayers) {
            player.Arena.Board.Visible = visible;
        }
    }
}