using Godot;

namespace MPAutoChess.logic.menu;

public partial class FullScreenButton : BaseButton {
    public override void _Ready() {
        ToggleMode = true;
        Toggled += toggled => {
            DisplayServer.WindowSetMode(toggled ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
        };
    }
}