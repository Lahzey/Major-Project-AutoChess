using System;
using System.Collections.Generic;
using Godot;
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

    public virtual GamePhase GetCurrentPhase() {
        return phases[GetCurrentPhaseIndex()];
    }
    
    public virtual int GetCurrentPhaseIndex() {
        return phases.Count - 1;
    }

    protected virtual void AdvancePhase() {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("Phase can only be advanced on the server.");
        StartPhase(GetNextPhase());
    }

    protected virtual void StartPhase(GamePhase phase) {
        if (phases.Count > 0) {
            phases[^1].End();
            RemoveChild(phases[^1]);
        }
        
        phases.Add(phase);
        PowerLevel += phase.GetPowerLevel();
        phase.RemainingTime = GetDefaultPhaseTime();

        if (ServerController.Instance.IsServer) {
            phase.Name = $"Phase{phases.Count}";
            AddChild(phase);
            Rpc(MethodName.TriggerStartPhase, SerializerExtensions.Serialize(phase));
        } else {
            PlayerUI.Instance.GamePhaseControls.SetPhaseControls(null);
        }
        
        phase.Start();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    protected virtual void TriggerStartPhase(byte[] serializedGamePhase) {
        GamePhase phase = SerializerExtensions.Deserialize<GamePhase>(serializedGamePhase);
        StartPhase(phase);
    }
}