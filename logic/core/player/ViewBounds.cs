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
        Transform2D globalTransform = GetGlobalTransform();
        CameraController.Instance.SetViewBounds(new Rect2(globalTransform.Origin, Size * globalTransform.Scale));
    }
}