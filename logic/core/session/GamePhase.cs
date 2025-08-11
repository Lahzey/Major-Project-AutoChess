using Godot;
using MPAutoChess.logic.core.player;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
[ProtoInclude(100, typeof(LootPhase))]
[ProtoInclude(101, typeof(CombatPhase))]
[ProtoInclude(102, typeof(EchoCombatPhase))]
public abstract partial class GamePhase : Node {
    
    protected static readonly Texture2D DEFAULT_ICON = ResourceLoader.Load<Texture2D>("res://assets/ui/phases/default.png");
    public static readonly Color DEFAULT_ICON_MODULATE = new Color("#b0bccf");

    [ProtoMember(1000)] public double RemainingTime { get; set; }

    public abstract string GetTitle(Player forPlayer);

    public virtual Texture2D GetIcon(Player forPlayer, out Color modulate) {
        modulate = DEFAULT_ICON_MODULATE;
        return DEFAULT_ICON;
    }

    public abstract int GetPowerLevel();

    public abstract void Start();

    public virtual bool IsFinished() {
        return RemainingTime <= 0;
    }

    public abstract void End();

}