using Godot;
using MPAutoChess.logic.core.stats;

namespace MPAutoChess.logic.core.item;

public partial class ItemTooltip : Control {
    
    public static ItemTooltip Instance { get; private set; }

    [Export] public ItemIcon Icon { get; set; }
    [Export] public Label NameLabel { get; set; }
    [Export] public RichTextLabel DescriptionLabel { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public Container StatsContainer { get; set; }
    [Export] public PackedScene StatDisplayScene { get; set; }
    [Export] public Container CraftedFromContainer { get; set; }
    [Export] public ItemIcon CraftedFromA { get; set; }
    [Export] public ItemIcon CraftedFromB { get; set; }
    
    public override void _Ready() {
        Instance = this;
        Visible = false;
    }

    public void Open(Item item) {
        Icon.Item = item;
        NameLabel.Text = item.GetName();
        DescriptionLabel.Text = item.GetDescription();

        CraftedFromContainer.Visible = item.Type.CraftedFromA != null;
        if (CraftedFromContainer.Visible) {
            CraftedFromA.Item = new Item(item.Type.CraftedFromA, item.ComponentLevels.Item1);
            CraftedFromB.Item = new Item(item.Type.CraftedFromB, item.ComponentLevels.Item2);
        }
        
        foreach (Node child in StatsContainer.GetChildren()) {
            child.QueueFree();
        }
        foreach (StatValue statValue in item.Type.Stats) {
            StatDisplay statDisplay = (StatDisplay) StatDisplayScene.Instantiate();
            statDisplay.StatType = statValue.StatType;
            statDisplay.StatValue = () => item.GetStat(statValue.StatType);
            StatsContainer.AddChild(statDisplay);
        }
        
        if (AnimationPlayer.IsPlaying()) AnimationPlayer.Stop();
        AnimationPlayer.Play("open");
    }

    public void Close() {
        if (AnimationPlayer.IsPlaying()) AnimationPlayer.Stop();
        AnimationPlayer.Play("close");
    }

    public void Move(Vector2 globalPosition) {
        ((Control) GetParent()).SetGlobalPosition(globalPosition);
    }
}
