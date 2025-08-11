using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit.role;
using MPAutoChess.logic.menu;

namespace MPAutoChess.logic.core.unit;

public partial class UnitInfoPanel : Control {
    
    [Export] public TextureRect SplashArt { get; set; }
    [Export] public Label NameLabel { get; set; }
    [Export] public TextureRect[] RoleIcons { get; set; }
    [Export] public Label[] RoleLabels { get; set; }
    [Export] public Label SellPriceLabel { get; set; }
    
    [Export] public ResourceBar HealthBar { get; set; }
    [Export] public Label HealthBarLabel { get; set; }
    [Export] public ResourceBar ManaBar { get; set; }
    [Export] public Label ManaBarLabel { get; set; }
    
    [Export] public Container ItemIconsContainer { get; set; }
    
    [Export] public Label SpellNameLabel { get; set; }
    [Export] public Label SpellCostLabel { get; set; }
    [Export] public RichTextLabel SpellDescriptionLabel { get; set; }
    
    [Export] public StatLabel[] StatLabels { get; set; }
    
    public static UnitInfoPanel Instance { get; private set; }
    
    private UnitInstance unitInstance;
    private List<ItemIcon> itemIcons = new List<ItemIcon>();
    
    public override void _EnterTree() {
        Instance = this;
        Visible = false;

        HealthBar.MouseEntered += ShowHealthTooltip;
        HealthBar.MouseExited += HideTooltip;
        ManaBar.MouseEntered += ShowManaTooltip;
        ManaBar.MouseExited += HideTooltip;
    }

    public override void _ExitTree() {
        HealthBar.MouseEntered -= ShowHealthTooltip;
        HealthBar.MouseExited -= HideTooltip;
        ManaBar.MouseEntered -= ShowManaTooltip;
        ManaBar.MouseExited -= HideTooltip;
    }

    private void ShowHealthTooltip() {
        if (unitInstance == null) return;
        ContextMenuItem[] contextMenu = unitInstance.Stats.GetCalculation(StatType.MAX_HEALTH).GenerateContextMenu(StatType.MAX_HEALTH).ToArray();
        ContextMenu.Instance.ShowContextMenu(HealthBar.GlobalPosition - new Vector2(HealthBar.Size.Y, HealthBar.Size.Y) * 0.5f, contextMenu, ContextMenu.AnchorPoint.TOP_RIGHT);
    }
    
    private void ShowManaTooltip() {
        if (unitInstance == null) return;
        ContextMenuItem[] contextMenu = unitInstance.Stats.GetCalculation(StatType.MAX_MANA).GenerateContextMenu(StatType.MAX_MANA).ToArray();
        ContextMenu.Instance.ShowContextMenu(ManaBar.GlobalPosition - new Vector2(ManaBar.Size.Y, ManaBar.Size.Y) * 0.5f, contextMenu, ContextMenu.AnchorPoint.TOP_RIGHT);
    }
    
    private void HideTooltip() {
        ContextMenu.Instance.HideContextMenu();
    }

    public void Open(UnitInstance unitInstance) {
        if (unitInstance == null) Close();
        this.unitInstance = unitInstance;
        
        HealthBar.Connect(unitInstance, instance => instance.Stats.GetValue(StatType.MAX_HEALTH), instance => instance.CurrentHealth);
        ManaBar.Connect(unitInstance, instance => instance.Stats.GetValue(StatType.MAX_MANA), instance => instance.CurrentMana);
        Visible = true;
    }

    public void Close() {
        Visible = false;
    }

    public override void _Process(double delta) {
        if (unitInstance == null) return;
        
        SplashArt.Texture = unitInstance.Unit.Type.Icon;
        NameLabel.Text = unitInstance.Unit.Type.Name;
        
        HashSet<UnitRole> roles = unitInstance.Unit.GetRoles();
        using IEnumerator<UnitRole> enumerator = roles.GetEnumerator();
        for (int i = 0; i < RoleIcons.Length; i++) {
            if (enumerator.MoveNext()) {
                UnitRole role = enumerator.Current;
                RoleIcons[i].Texture = role.GetIcon();
                RoleLabels[i].Text = role.GetName();
            } else {
                RoleIcons[i].Texture = null;
                RoleLabels[i].Text = string.Empty;
            }
        }
        
        SellPriceLabel.Text = unitInstance.Unit.GetSellValue().ToString();
        HealthBarLabel.Text = $"{unitInstance.CurrentHealth}/{unitInstance.Stats.GetValue(StatType.MAX_HEALTH)}";
        ManaBarLabel.Text = $"{unitInstance.CurrentMana}/{unitInstance.Stats.GetValue(StatType.MAX_MANA)}";
        SpellNameLabel.Text = unitInstance.Spell?.GetName(unitInstance) ?? "No Spell";
        // SpellCostLabel.Text = unitInstance.Spell?.GetCost().ToString() ?? string.Empty; // TODO: is this needed?
        SpellDescriptionLabel.Text = unitInstance.Spell?.GetDescription(unitInstance) ?? "[i]This unit does not have a spell.[/i]";
        
        foreach (StatLabel statLabel in StatLabels) {
            statLabel.UnitInstance = unitInstance;
            if (statLabel.StatType == StatType.ATTACK_SPEED) {
                statLabel.GetValueFunc = AttackSpeedValueFunc;
                statLabel.GetContextMenuFunc = AttackSpeedContextMenuFunc;
            }
        }
        
        while (itemIcons.Count < unitInstance.Unit.EquippedItems.Count) {
            ItemIcon itemIcon = new ItemIcon();
            itemIcon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
            itemIcon.MouseEntered += () => {
                ItemTooltip.Instance.Open(itemIcon.Item);
                ItemTooltip.Instance.Move(itemIcon.GetGlobalPosition() - new Vector2(ItemTooltip.Instance.Size.X, 0));
            };
            itemIcon.MouseExited += () => ItemTooltip.Instance.Close();
            itemIcons.Add(itemIcon);
            ItemIconsContainer.AddChild(itemIcon);
        }
        while (itemIcons.Count > unitInstance.Unit.EquippedItems.Count) {
            ItemIcon itemIcon = itemIcons[0];
            itemIcons.Remove(itemIcon);
            ItemIconsContainer.RemoveChild(itemIcon);
            itemIcon.QueueFree();
        }
        for (int i = 0; i < itemIcons.Count; i++) {
            itemIcons[i].Item = unitInstance.Unit.EquippedItems[i];
        }
    }

    private static float AttackSpeedValueFunc(UnitInstance unitInstance) {
        return unitInstance.GetTotalAttackSpeed();
    }
    
    private static ContextMenuItem[] AttackSpeedContextMenuFunc(UnitInstance unitInstance) {
        List<ContextMenuItem> items = new List<ContextMenuItem>();
        items.AddRange(unitInstance.Stats.GetCalculation(StatType.ATTACK_SPEED).GenerateContextMenu(StatType.ATTACK_SPEED));
        items.AddRange(unitInstance.Stats.GetCalculation(StatType.BONUS_ATTACK_SPEED).GenerateContextMenu(StatType.BONUS_ATTACK_SPEED));
        items.Add(ContextMenuItem.Separator());
        items.Add(ContextMenuItem.Label("Total Attack Speed: " + StatType.ATTACK_SPEED.ToString(unitInstance.GetTotalAttackSpeed())));
        return items.ToArray();
    }
}