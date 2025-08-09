using System.Collections.Generic;
using Godot;

namespace MPAutoChess.logic.menu;

public partial class WorldControls : Control {
    public static WorldControls Instance { get; private set; }
    public static WorldControls OverlayInstance { get; private set; }
    
    [Export] public bool IsOverlay { get; set; }

    private Dictionary<Control, PositioningInfo> controls = new Dictionary<Control, PositioningInfo>();

    private Camera2D camera;

    public struct PositioningInfo {
        public Node2D attachedTo;
        public Rect2 attachedToBounds;
        public AttachmentPoint attachmentPoint;
        public GrowthDirection xGrowthDirection;
        public GrowthDirection yGrowthDirection;
        public Vector2 size;
        public Vector2 offset;
    }


    public override void _EnterTree() {
        if (IsOverlay) OverlayInstance = this;
        else Instance = this;
    }

    public void AddControl(Control control, PositioningInfo positioning) {
        controls[control] = positioning;
        AddChild(control);
    }

    public void RemoveControl(Control control, bool autoFree = true) {
        controls.Remove(control);
        RemoveChild(control);
        if (autoFree) control.QueueFree();
    }

    public override void _Process(double delta) {
        if (camera == null) {
            camera = GetViewport().GetCamera2D();
            if (camera == null) return;
        }
        
        foreach ((Control control, PositioningInfo info) in controls) {
            if (info.attachedTo == null || !IsInstanceValid(info.attachedTo))
                continue;

            // Convert attachedTo world position and size to screen coordinates
            Rect2 attachedToGlobalBounds = new Rect2(info.attachedToBounds.Position * info.attachedTo.GlobalScale, info.attachedToBounds.Size * info.attachedTo.GlobalScale);
            Vector2 attachedToScreenPosition = (info.attachedTo.GlobalPosition + attachedToGlobalBounds.Position - camera.GlobalPosition) * camera.Zoom;
            Vector2 attachedToScreenSize = attachedToGlobalBounds.Size * camera.Zoom;

            // Apply scaled size and offset (relative to the attached node’s scale)
            Vector2 scaledSize = info.size * info.attachedTo.GlobalScale * camera.Zoom;
            Vector2 scaledOffset = info.offset * info.attachedTo.GlobalScale * camera.Zoom;

            Vector2 finalPosition = attachedToScreenPosition + scaledOffset;

            switch (info.xGrowthDirection) {
                case GrowthDirection.NEGATIVE: 
                    finalPosition.X -= scaledSize.X;
                    break;
                case GrowthDirection.BOTH:
                    finalPosition.X -= scaledSize.X / 2;
                    break;
                case GrowthDirection.POSITIVE:
                    // No adjustment needed, position is already correct
                    break;
            }
            
            switch (info.yGrowthDirection) {
                case GrowthDirection.NEGATIVE:
                    finalPosition.Y -= scaledSize.Y;
                    break;
                case GrowthDirection.BOTH:
                    finalPosition.Y -= scaledSize.Y / 2;
                    break;
                case GrowthDirection.POSITIVE:
                    // No adjustment needed, position is already correct
                    break;
            }

            switch (info.attachmentPoint) {
                case AttachmentPoint.TOP_LEFT:
                    // No adjustment needed, position is already correct
                    break;
                case AttachmentPoint.TOP_CENTER:
                    finalPosition.X += attachedToScreenSize.X / 2;
                    break;
                case AttachmentPoint.TOP_RIGHT:
                    finalPosition.X += attachedToScreenSize.X;
                    break;
                case AttachmentPoint.CENTER_LEFT:
                    finalPosition.Y += attachedToScreenSize.Y / 2;
                    break;
                case AttachmentPoint.CENTER:
                    finalPosition.X += attachedToScreenSize.X / 2;
                    finalPosition.Y += attachedToScreenSize.Y / 2;
                    break;
                case AttachmentPoint.CENTER_RIGHT:
                    finalPosition.X += attachedToScreenSize.X;
                    finalPosition.Y += attachedToScreenSize.Y / 2;
                    break;
                case AttachmentPoint.BOTTOM_LEFT:
                    finalPosition.Y += attachedToScreenSize.Y;
                    break;
                case AttachmentPoint.BOTTOM_CENTER:
                    finalPosition.X += attachedToScreenSize.X / 2;
                    finalPosition.Y += attachedToScreenSize.Y;
                    break;
                case AttachmentPoint.BOTTOM_RIGHT:
                    finalPosition.X += attachedToScreenSize.X;
                    finalPosition.Y += attachedToScreenSize.Y;
                    break;
            }
            
            control.Size = scaledSize;
            control.Position = finalPosition;
        }
    }
}

public enum AttachmentPoint {
    TOP_LEFT,
    TOP_CENTER,
    TOP_RIGHT,
    CENTER_LEFT,
    CENTER,
    CENTER_RIGHT,
    BOTTOM_LEFT,
    BOTTOM_CENTER,
    BOTTOM_RIGHT,
}

public enum GrowthDirection : int {
    NEGATIVE = -1,
    BOTH = 0,
    POSITIVE = 1,
}