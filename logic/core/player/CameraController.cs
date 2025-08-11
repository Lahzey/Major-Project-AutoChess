using Godot;

namespace MPAutoChess.logic.core.player;

public partial class CameraController : Node {

    private Camera2D camera;

    private Rect2 viewBounds;
    private Rect2 toCover;

    private float targetZoom = 1f;
    private Vector2 targetPosition = Vector2.Zero;
    
    public static CameraController Instance { get; private set; }

    public override void _EnterTree() {
        Instance = this;
    }

    public override void _ExitTree() {
        if (Instance == this) Instance = null;
    }

    public void SetViewBounds(Rect2 bounds) {
        viewBounds = bounds;
        Cover(toCover);
    }

    public override void _Process(double delta) {
        if (camera == null) return;
        
        float currentZoom = camera.Zoom.X;
        float zoomDiff = targetZoom - currentZoom;
        if (Mathf.Abs(zoomDiff) > 0.01f) {
            float newZoom = Mathf.Lerp(currentZoom, targetZoom, (float)delta * 5f);
            camera.Zoom = new Vector2(newZoom, newZoom);
        } else {
            camera.Zoom = new Vector2(targetZoom, targetZoom);
        }
        
        Vector2 currentPosition = camera.Position;
        float positionDiffSquared = (targetPosition - currentPosition).LengthSquared();
        if (positionDiffSquared > 0.01f && positionDiffSquared < 1000f) {
            Vector2 newPosition = currentPosition.Lerp(targetPosition, (float)delta * 5f);
            camera.Position = newPosition;
        } else {
            camera.Position = targetPosition;
        }
    }

    public void Cover(Rect2 bounds) {
        toCover = bounds;

        camera = GetViewport().GetCamera2D();
        if (camera == null)
            return;

        if (viewBounds.Size.X == 0 || viewBounds.Size.Y == 0 || toCover.Size.X == 0 || toCover.Size.Y == 0)
            return;
        
        camera.AnchorMode = Camera2D.AnchorModeEnum.DragCenter;

        float zoomX = viewBounds.Size.X / toCover.Size.X;
        float zoomY = viewBounds.Size.Y / toCover.Size.Y;
        float requiredZoom = Mathf.Min(zoomX, zoomY); // Smallest zoom to ensure full bounds visible
        targetZoom = requiredZoom;

        Vector2 toCoverCenter = toCover.Position + toCover.Size / 2f;
        
        // account for difference between the viewport center and the view bounds center
        Vector2 viewportCenter = GetViewport().GetVisibleRect().Size * 0.5f;
        Vector2 viewBoundsCenter = viewBounds.Position + viewBounds.Size * 0.5f;
        Vector2 boundsCenterOffset = (viewportCenter - viewBoundsCenter) / requiredZoom;

        targetPosition = toCoverCenter + boundsCenterOffset;
    }

    public Vector2 ToWorldPosition(Vector2 viewportPosition) {
        if (camera == null) return viewportPosition;

        Vector2 offset = camera.AnchorMode == Camera2D.AnchorModeEnum.DragCenter ? GetViewport().GetVisibleRect().Size * 0.5f : Vector2.Zero;
        return (viewportPosition - offset) / camera.Zoom + camera.GlobalPosition;
    }
}