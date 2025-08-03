using Godot;
using MPAutoChess.logic.core.player;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
[ProtoInclude(100, typeof(LootPhase))]
[ProtoInclude(101, typeof(CombatPhase))]
[ProtoInclude(102, typeof(EchoCombatPhase))]
public abstract partial class GamePhase : Node {

    public abstract string GetName(Player forPlayer);

    public abstract int GetPowerLevel();

    public abstract void Start();

    public abstract bool IsFinished();

    public abstract void End();

}