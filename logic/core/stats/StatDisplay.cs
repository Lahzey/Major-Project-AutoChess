using System;
using Godot;
using MPAutoChess.logic.util;
using AutoFontSize = MPAutoChess.logic.util.AutoFontSize;

namespace MPAutoChess.logic.core.stats;

public partial class StatDisplay : Control {

    [Export] private TextureRect Icon { get; set; }
    [Export] private Label Label { get; set; }
    [Export] private AutoFontSize AutoFontSize { get; set; }

    public FontSizeType FontSize {
        get => AutoFontSize.SizeType;
        set => AutoFontSize.SizeType = value;
    }

    private StatType statType;

    public StatType StatType {
        get => statType;
        set {
            statType = value;
            Icon.Texture = statType?.Icon ?? null;
        }
    }

    public Func<float> StatValue { get; set; }

    public override void _Process(double delta) {
        float statVal = StatValue?.Invoke() ?? 0f;
        Label.Text = StatType?.ToString(statVal) ?? statVal.ToString("0.####");
    }
}