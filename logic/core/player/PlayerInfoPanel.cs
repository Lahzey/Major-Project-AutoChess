using Godot;

namespace MPAutoChess.logic.core.player;

public partial class PlayerInfoPanel : Control {
    
    private static readonly Color PLAYER_BACKGROUND_COLOR = new Color("#00000049");
    private static readonly Color SELECTED_PLAYER_BACKGROUND_COLOR = new Color("#1d3faa83");
    
    [Export] public ProgressBar HealthBar { get; set; }
    [Export] public Label HealthLabel { get; set; }
    [Export] public Label NameLabel { get; set; }
    [Export] public TextureRect ProfilePicture { get; set; }
    [Export] public Control ProfileBackground { get; set; }

    private StyleBoxFlat HealthStyleBox;
    private StyleBoxFlat ProfileBackgroundStyleBox;
    
    public Player Player { get; set; }

    public override void _Ready() {
        HealthStyleBox = HealthLabel.GetThemeStylebox("normal").Duplicate() as StyleBoxFlat;
        HealthLabel.AddThemeStyleboxOverride("normal", HealthStyleBox);
        HealthLabel.MouseDefaultCursorShape = CursorShape.PointingHand;
        HealthLabel.MouseFilter = MouseFilterEnum.Stop;
        HealthLabel.GuiInput += OnInput;
        
        ProfileBackgroundStyleBox = ProfileBackground.GetThemeStylebox("normal").Duplicate() as StyleBoxFlat;
        ProfileBackground.AddThemeStyleboxOverride("normal", ProfileBackgroundStyleBox);
        ProfileBackground.MouseDefaultCursorShape = CursorShape.PointingHand;
        ProfileBackground.MouseFilter = MouseFilterEnum.Stop;
        ProfileBackground.GuiInput += OnInput;
    }
    
    private void OnInput(InputEvent @event) {
        if (@event is InputEventMouseButton mouseButton && mouseButton.IsPressed() && mouseButton.ButtonIndex == MouseButton.Left) {
            PlayerController.Current.GoToArena(Player.Arena);
        }
    }

    public override void _Process(double delta) {
        if (Player == null) return;
        HealthBar.MaxValue = Player.MaxHealth;
        HealthBar.Value = Player.CurrentHealth;
        HealthLabel.Text = Player.CurrentHealth.ToString().PadLeft(3);
        NameLabel.Text = Player.Account.Name;
        ProfilePicture.Texture = Player.Account.ProfilePicture;
        
        Color backgroundColor = PlayerController.Current.CurrentlyShowing.Player == Player ? SELECTED_PLAYER_BACKGROUND_COLOR : PLAYER_BACKGROUND_COLOR;
        HealthStyleBox.BgColor = backgroundColor;
        ProfileBackgroundStyleBox.BgColor = backgroundColor;
    }
}