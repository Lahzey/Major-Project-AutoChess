using System.Linq;
using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.util;

namespace MPAutoChess.logic.core.unit.role;

public partial class UnitRolePanel : Control {
    
    [Export] public TextureRect Icon { get; set; }
    [Export] public Label NameLabel { get; set; }
    [Export] public AutoFontSize NameSize { get; set; }
    [Export] public Label CountLabel { get; set; }
    [Export] public RichTextLabel ThresholdsLabel { get; set; }

    public Player Player { get; set; }
    public UnitRole Role { get; set; }

    public override void _Process(double delta) {
        Icon.Texture = Role?.GetIcon();
        NameLabel.Text = Role?.GetName() ?? string.Empty;
        if (Role != null && Player != null) {
            int count = Player.Board.GetUnitTypesInAllRoles()[Role].Count;
            int currentThreshold = Role.GetCurrentThreshold(count);
            int level = Role.GetLevel(count);
            CountLabel.Text = level > 0 ? count.ToString() : $"{count.ToString()}/{Role.GetCountThresholds()[0]}";
            ThresholdsLabel.Text = string.Join("   >   ", Role.GetCountThresholds().Select(threshold => threshold == currentThreshold ? $"[b]{threshold}[/b]" : threshold.ToString()));
            ThresholdsLabel.Visible = level > 0;
            NameLabel.SizeFlagsVertical = SizeFlags.Expand | (level > 0 ? SizeFlags.ShrinkEnd : SizeFlags.ShrinkCenter);
            NameSize.SizeType = level > 0 ? FontSizeType.NORMAL : FontSizeType.SUBTITLE;
        } else {
            CountLabel.Text = string.Empty;
            ThresholdsLabel.Text = string.Empty;
        }
    }
}