using Godot;
using MPAutoChess.logic.core.networking;

namespace MPAutoChess.logic.core.player;

public partial class ViewBounds : Control {
    
    public override void _Ready() {
        if (ServerController.Instance.IsServer) return;
        Resized += OnResize;
        OnResize();
    }

    private void OnResize() {
        CameraController.Instance.SetViewBounds(new Rect2(GlobalPosition, Size));
    }
}