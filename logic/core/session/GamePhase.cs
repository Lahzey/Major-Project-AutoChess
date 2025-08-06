using Godot;
using MPAutoChess.logic.core.player;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
[ProtoInclude(100, typeof(LootPhase))]
[ProtoInclude(101, typeof(CombatPhase))]
[ProtoInclude(102, typeof(EchoCombatPhase))]
public abstract partial class GamePhase : Node {

    [ProtoMember(1000)] public double RemainingTime { get; set; }

    public abstract string GetTitle(Player forPlayer);

    public abstract int GetPowerLevel();

    public abstract void Start();

    public virtual bool IsFinished() {
        return RemainingTime <= 0;
    }

    public abstract void End();

}