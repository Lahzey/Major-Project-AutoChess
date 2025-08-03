using Godot;

namespace MPAutoChess.logic.core.player;

public partial class CameraController : Node {

    private Rect2 viewBounds;
    private Rect2 toCover;
    
    public static CameraController Instance { get; private set; }

    public override void _EnterTree() {
        Instance = this;
    }
    
    public void SetViewBounds(Rect2 bounds) {
        viewBounds = bounds;
        Cover(toCover);
    }

    public void Cover(Rect2 bounds) {
        toCover = bounds;
        
        GD.Print($"{System.Environment.ProcessId}: Trying to cover {toCover} within view bounds {viewBounds}");
        
        Camera2D camera = GetViewport().GetCamera2D();
        if (camera == null)
            return;
        
        if (viewBounds.Size.X == 0 || viewBounds.Size.Y == 0 || toCover.Size.X == 0 || toCover.Size.Y == 0)
            return;
        
        float zoomX = viewBounds.Size.X / toCover.Size.X;
        float zoomY = viewBounds.Size.Y / toCover.Size.Y;
        float requiredZoom = Mathf.Min(zoomX, zoomY); // Take the smaller zoom to ensure the entire arena is visible

        Vector2 center = toCover.Position + toCover.Size / 2f;

        camera.AnchorMode = Camera2D.AnchorModeEnum.FixedTopLeft;
        camera.Zoom = new Vector2(requiredZoom, requiredZoom);
        camera.GlobalPosition = center - (viewBounds.Position / requiredZoom + viewBounds.Size / 2 / requiredZoom);
    }
}