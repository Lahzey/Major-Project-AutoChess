using System;
using Godot;
using MPAutoChess.logic.core.stats;

namespace MPAutoChess.logic.core.unit;

[Tool]
public partial class ResourceBar : ProgressBar {
    
    public const float HEALTH_SMALL_GAP = 250f;
    public const float HEALTH_LARGE_GAP = 1000f;
    public const float NO_GAP = 0f;
    public static readonly Color HEALTH_COLOR = new Color(0.55f, 0f, 0f, 1f);
    public static readonly Color MANA_COLOR = new Color(0f, 0.24f, 0.4f, 1f);
    
    public float SmallSeparatorGap { get; set; } = 250f;
    public float LargeSeparatorGap { get; set; } = 1000f;
    
    public Color SmallSeparatorColor { get; set; } = new Color(0f, 0f, 0f, 0.5f);
    public Color LargeSeparatorColor { get; set; } = new Color(0f, 0f, 0f, 1f);

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
        StyleBoxFlat stylebox = (StyleBoxFlat) GetThemeStylebox("fill").Duplicate();
        stylebox.BgColor = color;
        AddThemeStyleboxOverride("fill", stylebox);
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