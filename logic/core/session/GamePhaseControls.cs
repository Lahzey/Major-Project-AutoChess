using Godot;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.session;

public partial class GamePhaseControls : Control {
    
    [Export] public Control SmallControlsPanel { get; set; }
    [Export] public Control LargeControlsPanel { get; set; }
    [Export] public Container PastPhases { get; set; }
    [Export] public Container FuturePhases { get; set; }
    [Export] public Label PhaseTimer { get; set; }
    [Export] public Label PhaseTitle { get; set; }
    [Export] public Control PhaseSpecificControls { get; set; }
    
    private Control phaseControls;


    public override void _Process(double delta) {
        if (ServerController.Instance.IsServer || GameSession.Instance == null || PlayerController.Current == null) return;
        double remainingTime = GameSession.Instance.Mode.GetCurrentPhase()?.RemainingTime?? 0;
        PhaseTimer.Text = remainingTime >= 10 ? remainingTime.ToString("N0") : remainingTime.ToString("N1");
        PhaseTitle.Text = GameSession.Instance.Mode.GetCurrentPhase().GetTitle(PlayerController.Current.Player);
    }
    
    public void SetPhaseControls(Control control) {
        phaseControls?.QueueFree();
        if (control != null) PhaseSpecificControls.AddChild(control);
        phaseControls = control;

        SmallControlsPanel.Visible = control == null;
        LargeControlsPanel.Visible = control != null;
    }
}