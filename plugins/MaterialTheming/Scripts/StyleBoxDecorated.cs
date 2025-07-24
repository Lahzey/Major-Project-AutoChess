using Godot;
using Vector2 = Godot.Vector2;

namespace MaterialTheming.Scripts;

[GlobalClass, Tool, Icon("res://plugins/MaterialTheming/icons/StyleBoxDecorated.svg")]
public partial class StyleBoxDecorated : StyleBoxMaterial {
    private const float ONE_THIRD = 1f / 3f; // in case the compiler does not do this already

    private readonly DecoratedPolygonRenderer decoratedRenderer = new DecoratedPolygonRenderer();

    private bool useOutlineRectBorderPattern = true;
    [Export] public bool UseOutlineRectBorderPattern { get => useOutlineRectBorderPattern; set { useOutlineRectBorderPattern = value; TryAdjustContentMargins(); EmitChanged(); } }

    [ExportGroup("Animation")]
    
    private bool animated;
    [Export] public bool Animated { get => animated; set { animated = value; NotifyPropertyListChanged(); EmitChanged(); } }
    
    private float duration = 1f;
    [Export] public float Duration { get => duration; set { duration = value; EmitChanged(); } }
    
    private Color highlightColor;
    [Export] public Color HighlightColor { get => highlightColor; set { highlightColor = value; EmitChanged(); } }
    
    private bool loop = true;
    [Export] public bool Loop { get => loop; set { loop = value; EmitChanged(); } }
    
    private AnimationMode mode;
    [Export] public AnimationMode Mode { get => mode; set { mode = value; EmitChanged(); } }

    public enum AnimationMode : int {
        RADIAL_FILL = 0,
        RADIAL_TRACE = 1,
        BLINK = 2
    }

    protected override PolygonRenderer GetPolygonRenderer() {
        return UseOutlineRectBorderPattern ? decoratedRenderer : defaultPolygonRenderer;
    }

    public override void _ValidateProperty(Godot.Collections.Dictionary property) {
        if (BackgroundMode != BackgroundModeOption.POLYGON) BackgroundMode = BackgroundModeOption.POLYGON; // a bit of a hack to set it here, but we need it to always be set to POLYGON
        base._ValidateProperty(property);
        
        StringName stringName = property["name"].AsStringName();
        PropertyUsageFlags usage = property["usage"].As<PropertyUsageFlags>();
        switch (true) { // just using switch true with case guards as a prettier if
            case true when stringName == StyleBoxMaterial.PropertyName.BackgroundMode:
                property["usage"] = (int) (usage | PropertyUsageFlags.ReadOnly);
                break;
            case true when stringName == PropertyName.Duration:
            case true when stringName == PropertyName.HighlightColor:
            case true when stringName == PropertyName.Loop:
            case true when stringName == PropertyName.Mode:
                if (!Animated) property["usage"] = (int) (usage | PropertyUsageFlags.ReadOnly);
                break;
        }
    }


    protected override void SetMaterialParams(Material material) {
        base.SetMaterialParams(material);
        if (material is not ShaderMaterial shaderMat) return;
        shaderMat.SetShaderParameter("animated", Animated);
        shaderMat.SetShaderParameter("animation_duration", Duration);
        shaderMat.SetShaderParameter("highlight_color", HighlightColor);
        shaderMat.SetShaderParameter("loop_animation", Loop);
        shaderMat.SetShaderParameter("animation_mode", (int) Mode);
    }

    private class DecoratedPolygonRenderer : PolygonRenderer {
        protected override void CreateBorderGeometry(PolygonInfo polygon, SurfaceTool surfaceTool, PrecomputedEdge edge, Rect2 totalRect) {
            float thirdBorder = polygon.BorderThickness * ONE_THIRD;
            Vector2 startOutward = edge.start + edge.outwardNormal * polygon.BorderThickness;
            Vector2 startCornerOffset = (edge.startOuterCorner - startOutward) * ONE_THIRD;
            Vector2 innerStartCorner = edge.start + edge.outwardNormal * thirdBorder + startCornerOffset;
            Vector2 endOutward = edge.end + edge.outwardNormal * polygon.BorderThickness;
            Vector2 endCornerOffset = (edge.endOuterCorner - endOutward) * ONE_THIRD;
            Vector2 innerEndCorner = edge.end + edge.outwardNormal * thirdBorder + endCornerOffset;
            
            // outerTopLeft===========================================================================outerTopRight
            // innerTopLeft==innerTopLeftInner====================================innerTopRightInner==innerTopRight
            // ||###################||                                                    ||#####################||
            // ||###################||                                                    ||#####################||
            // innerBotLeft==innerBotLeftInner====================================innerBotRightInner==innerBotRight
            //      outerBotLeft=================================================================outerBotRight
            Vector2 outerTopLeft = innerStartCorner + edge.outwardNormal * thirdBorder * 2;
            Vector2 outerTopRight = innerEndCorner + edge.outwardNormal * thirdBorder * 2;
            Vector2 innerTopLeft = innerStartCorner + edge.outwardNormal * thirdBorder;
            Vector2 innerTopRight = innerEndCorner + edge.outwardNormal * thirdBorder;
            Vector2 innerTopLeftInner = innerTopLeft + edge.edgeNormal * thirdBorder;
            Vector2 innerTopRightInner = innerTopRight - edge.edgeNormal * thirdBorder;
            Vector2 innerBotLeft = innerStartCorner;
            Vector2 innerBotRight = innerEndCorner;
            Vector2 innerBotLeftInner = innerBotLeft + edge.edgeNormal * thirdBorder;
            Vector2 innerBotRightInner = innerBotRight - edge.edgeNormal * thirdBorder;
            Vector2 outerBotLeft = edge.start;
            Vector2 outerBotRight = edge.end;
            
            // Top side
            AddRectToMesh(surfaceTool, outerTopLeft, outerTopRight, innerTopRight, innerTopLeft, polygon.BorderColor, BORDER_CUSTOM, totalRect);
            
            // Left side
            AddRectToMesh(surfaceTool, innerTopLeft, innerTopLeftInner, innerBotLeftInner, innerBotLeft, polygon.BorderColor, BORDER_CUSTOM, totalRect);
            
            // Right side
            AddRectToMesh(surfaceTool, innerTopRightInner, innerTopRight, innerBotRight, innerBotRightInner, polygon.BorderColor, BORDER_CUSTOM, totalRect);
            
            // Bot side
            AddRectToMesh(surfaceTool, innerBotLeft, innerBotRight, outerBotRight, outerBotLeft, polygon.BorderColor, BORDER_CUSTOM, totalRect);
        }
        
        private static void AddRectOutlineToMesh(SurfaceTool surfaceTool, Vector2 aa, Vector2 ba, Vector2 bb, Vector2 ab, Color color, float thickness, Color custom, Rect2 totalRect) {
            Vector2 right = (ba - aa).Normalized() * thickness;
            Vector2 down = (ab - aa).Normalized() * thickness;
            // Top side
            AddRectToMesh(surfaceTool, aa, ba, ba + down, aa + down, color, custom, totalRect);
            // Bottom side
            AddRectToMesh(surfaceTool, ab - down, bb - down, bb, ab, color, custom, totalRect);
            // Left side    W
            AddRectToMesh(surfaceTool, aa, aa + right, ab + right, ab, color, custom, totalRect);
            // Right side
            AddRectToMesh(surfaceTool, ba - right, ba, bb, bb - right, color, custom, totalRect);
        }
    }
}