using Godot;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.player;
using ProtoBuf;

namespace MPAutoChess.logic.core.environment;

[ProtoContract]
public partial class Arena : Node2D {

    [Export] public Vector2 ArenaSize { get; set; } = new Vector2(19.20f, 10.80f);
    
    [Export] [ProtoMember(1)] public Board Board { get; set; }
    [Export] [ProtoMember(2)] public Bench Bench { get; set; }

    private Player player;

    public Player Player {
        get => player;
        set {
            player = value;
            Board.Player = value;
            Bench.Player = value;
        }
    }

    public void FitCameraToArena(Camera2D camera) {
        if (camera == null)
            return;
        
        Vector2 viewportSize = camera.GetViewportRect().Size;
        if (viewportSize.X == 0 || viewportSize.Y == 0) // prevent division by zero
            return;
        
        float zoomX = viewportSize.X / ArenaSize.X;
        float zoomY = viewportSize.Y / ArenaSize.Y;
        float requiredZoom = Mathf.Min(zoomX, zoomY); // Take the smaller zoom to ensure the entire arena is visible

        Vector2 center = ArenaSize / 2f;

        camera.AnchorMode = Camera2D.AnchorModeEnum.DragCenter;
        camera.Zoom = new Vector2(requiredZoom, requiredZoom);
        camera.GlobalPosition = GlobalPosition + center;
    }
}