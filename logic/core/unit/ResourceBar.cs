using System;
using Godot;
using MPAutoChess.logic.core.stats;

namespace MPAutoChess.logic.core.unit;

[Tool, GlobalClass]
public partial class ResourceBar : ProgressBar {
    
    public const float HEALTH_SMALL_GAP = 250f;
    public const float HEALTH_LARGE_GAP = 1000f;
    public const float NO_GAP = 0f;
    public static readonly Color HEALTH_SELF_COLOR = new Color("#166e0f");
    public static readonly Color HEALTH_ALLY_COLOR = new Color("#a69107");
    public static readonly Color HEALTH_ENEMY_COLOR = new Color("#780707");
    public static readonly Color MANA_COLOR = new Color("003d66");
    
    [Export] public float SmallSeparatorGap { get; set; } = 250f;
    [Export] public float LargeSeparatorGap { get; set; } = 1000f;
    
    [Export] public Color SmallSeparatorColor { get; set; } = new Color(0f, 0f, 0f, 0.5f);
    [Export] public Color LargeSeparatorColor { get; set; } = new Color(0f, 0f, 0f, 1f);

    private Color fillColor = Colors.Transparent;
    [Export] public Color FillColor {
        get => fillColor;
        set {
            SetFillColor(value);
            fillColor = value;
        }
    }

    public UnitInstance UnitInstance { get; private set; }
    public Func<UnitInstance, float> GetMaxValue { get; private set; }
    public Func<UnitInstance, float> GetCurrentValue { get; private set; }
    
    public void Connect(UnitInstance unitInstance, Func<UnitInstance, float> getMaxValue, Func<UnitInstance, float> getCurrentValue) {
        UnitInstance = unitInstance;
        GetMaxValue = getMaxValue;
        GetCurrentValue = getCurrentValue;
        
        // Set the initial values
        Value = GetCurrentValue(UnitInstance);
        MaxValue = GetMaxValue(UnitInstance);
    }
    
    public void SetGaps(float smallGap, float largeGap) {
        SmallSeparatorGap = smallGap;
        LargeSeparatorGap = largeGap;
    }
    
    public void SetFillColor(Color color) {
        StyleBox styleBox = GetThemeStylebox("fill");
        StyleBoxFlat styleBoxFlat = styleBox is StyleBoxFlat flat ? (StyleBoxFlat) flat.Duplicate() : new StyleBoxFlat();
        styleBoxFlat.BgColor = color;
        AddThemeStyleboxOverride("fill", styleBoxFlat);
    }

    public override void _Ready() {
        if (FillColor != Colors.Transparent) {
            SetFillColor(FillColor);
        }
    }

    public override void _Process(double delta) {
        if (UnitInstance == null) return;
        Value = GetCurrentValue?.Invoke(UnitInstance) ?? 0f;
        MaxValue = GetMaxValue?.Invoke(UnitInstance) ?? 1000f;
        QueueRedraw();
    }

    public override void _Draw() {
        float separatorWidth = Size.X / 50f;
        if (SmallSeparatorGap > 0) DrawLines(0, Size.Y * 0.5f, SmallSeparatorGap, SmallSeparatorColor, separatorWidth);
        if (LargeSeparatorGap > 0) DrawLines(0, Size.Y, LargeSeparatorGap, LargeSeparatorColor, separatorWidth);
    }
    
    private void DrawLines(float minY, float maxY, float gap, Color color, float width) {
        float valueRange = (float) (MaxValue - MinValue);
        float value = (float) (Value - MinValue);
        if (gap > 0) {
            for (float pos = gap; pos < value; pos += gap) {
                float progress = pos / valueRange;
                DrawLine(
                    new Vector2(progress * Size.X, minY),
                    new Vector2(progress * Size.X, maxY),
                    color,
                    width
                );
            }
        }
    }
}