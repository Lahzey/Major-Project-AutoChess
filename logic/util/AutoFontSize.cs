using Godot;

namespace MPAutoChess.logic.util;

[Tool, GlobalClass]
public partial class AutoFontSize : Node {

    [Export] public FontSizeType SizeType { get; set; } = FontSizeType.NORMAL;
    [Export] public string FontSizeProperty { get; set; } = "font_size";


    public override void _Process(double delta) {
        Node parent = GetParent();
        if (parent is Control control) {
            control.AddThemeFontSizeOverride(FontSizeProperty, FontSizeCalculator.GetFontSize(SizeType, GetViewport()));
        }
    }
    

}