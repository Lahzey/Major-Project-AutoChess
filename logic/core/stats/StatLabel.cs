using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using MPAutoChess.logic.core.unit;
using MPAutoChess.logic.menu;

namespace MPAutoChess.logic.core.stats;

[Tool]
public partial class StatLabel : Control {

    [Export] public string Stat { get; set; } = StatType.STRENGTH.Name;
    [Export] public TextureRect Icon { get; set; }
    [Export] public Label ValueLabel { get; set; }

    public UnitInstance UnitInstance { get; set; }
    public Func<UnitInstance, float> GetValueFunc { get; set; } = null;
    public Func<UnitInstance, ContextMenuItem[]> GetContextMenuFunc { get; set; } = null;
    
    public StatType StatType => StatType.Parse(Stat);

    public override void _ValidateProperty(Dictionary property) {
        if ((string)property["name"] == PropertyName.Stat) {
            string[] names = StatType.GetAllValues().Select(st => st.Name).ToArray();
            property["hint"] = (int)PropertyHint.Enum;
            property["hint_string"] = string.Join(",", names);
        }
    }

    public override void _EnterTree() {
        MouseEntered += ShowTooltip;
        MouseExited += HideTooltip;
    }

    public override void _ExitTree() {
        MouseEntered -= ShowTooltip;
        MouseExited -= HideTooltip;
    }

    private void ShowTooltip() {
        if (UnitInstance == null) return;
        StatType statType = StatType;
        Calculation calculation = UnitInstance.Stats.GetCalculation(statType);
        if (calculation == null) return;
        
        ContextMenuItem[] contextMenu = GetContextMenuFunc?.Invoke(UnitInstance) ?? calculation.GenerateContextMenu(statType).ToArray();
        ContextMenu.Instance.ShowContextMenu(GlobalPosition - new Vector2(Size.Y, Size.Y) * 0.5f, contextMenu, ContextMenu.AnchorPoint.TOP_RIGHT);
    }
    
    private void HideTooltip() {
        ContextMenu.Instance.HideContextMenu();
    }

    public override void _Ready() {
        MouseFilter = MouseFilterEnum.Stop;
        Icon.MouseFilter = MouseFilterEnum.Ignore;
        ValueLabel.MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void _Process(double delta) {
        StatType statType = StatType;
        Icon.Texture = statType.Icon;
        
        float statValue = GetValueFunc?.Invoke(UnitInstance) ?? (UnitInstance?.Stats.GetValue(statType) ?? 0);
        ValueLabel.Text = statType.ToString(statValue);
    }
}