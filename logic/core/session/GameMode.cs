using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.util;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
[ProtoInclude(100, typeof(LiveMode))]
[ProtoInclude(101, typeof(EchoMode))]
public abstract partial class GameMode : Node {
    
    [ProtoMember(1)] protected List<GamePhase> phases = new List<GamePhase>();
    [ProtoMember(2)] public int PowerLevel { get; protected set; }
    
    public static GameMode GetByName(string name) {
        return name switch {
            "EchoMode" => new EchoMode(),
            "LiveMode" => new LiveMode(),
            _ => throw new ArgumentException($"Unknown GameMode: {name}")
        };
    }
    
    public abstract void Tick(double delta);
    protected abstract GamePhase GetNextPhase();
    public abstract double GetDefaultPhaseTime();

    public virtual void Start() {
        if (ServerController.Instance.IsServer) AdvancePhase();
    }

    public virtual int GetPhaseStartGold() {
        return Math.Min(GetCurrentPhaseIndex() + 1, 5);
    }

    public virtual int GetPhaseStartExperience() {
        return 2;
    }

    public GamePhase GetCurrentPhase() {
        int currentIndex = GetCurrentPhaseIndex();
        return GetPhaseAt(currentIndex);
    }

    public virtual GamePhase GetPhaseAt(int index) {
        if (index < 0 || index >= phases.Count) return null;
        return phases[index];
    }
    
    public virtual int GetCurrentPhaseIndex() {
        return phases.Count - 1;
    }

    protected virtual void AdvancePhase() {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("Phase can only be advanced on the server.");
        StartPhase(GetNextPhase());
    }

    protected virtual void StartPhase(GamePhase phase) {
        if (!ServerController.Instance.IsServer && PlayerController.Current.Player.Dead) return;
        GamePhase? currentPhase = GetCurrentPhase();
        PhaseChangeEvent phaseChangeEvent = new PhaseChangeEvent(currentPhase, phase);
        EventManager.INSTANCE.NotifyBefore(phaseChangeEvent);
        
        if (currentPhase != null) {
            currentPhase.End();
            RemoveChild(currentPhase);
        }
        
        phases.Add(phase);
        PowerLevel += phase.GetPowerLevel();
        phase.RemainingTime = GetDefaultPhaseTime();

        if (ServerController.Instance.IsServer) {
            phase.Name = $"Phase{phases.Count}";
            AddChild(phase);
            Rpc(MethodName.TriggerClientStartPhase, SerializerExtensions.Serialize(phase));
            foreach (Player player in GameSession.Instance.AlivePlayers) {
                player.AddExperience(GetPhaseStartExperience());
                player.AddGold(GetPhaseStartGold());
                player.AddInterest();
            }
        } else {
            PlayerUI.Instance.GamePhaseControls.SetPhaseControls(null);
        }
        
        phase.Start();
        EventManager.INSTANCE.NotifyAfter(phaseChangeEvent);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    protected virtual void TriggerClientStartPhase(byte[] serializedGamePhase) {
        GamePhase phase = SerializerExtensions.Deserialize<GamePhase>(serializedGamePhase);
        StartPhase(phase);
    }
}