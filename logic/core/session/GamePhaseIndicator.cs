using Godot;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.session;

public partial class GamePhaseIndicator : TextureRect {
    
    private static readonly Texture2D UNKNOWN_ICON = ResourceLoader.Load<Texture2D>("res://assets/ui/phases/unknown.png");
    
    [Export] public int PhaseOffset { get; set; }

    public override void _Process(double delta) {
        if (GameSession.Instance == null || PlayerController.Current == null) return;
        GamePhase phase = GameSession.Instance.Mode.GetPhaseAt(GameSession.Instance.Mode.GetCurrentPhaseIndex() + PhaseOffset);
        if (phase == null) {
            TooltipText = PhaseOffset > 0 ? "Unknown" : null;
            Texture = PhaseOffset > 0 ? UNKNOWN_ICON : null;
            Modulate = GamePhase.DEFAULT_ICON_MODULATE;
            return;
        }
        
        TooltipText = phase.GetTitle(PlayerController.Current.Player);
        Texture = phase.GetIcon(PlayerController.Current.Player, out Color modulate);
        Modulate = modulate;
    }
}