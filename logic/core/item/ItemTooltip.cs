using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.stats;

namespace MPAutoChess.logic.core.item;

public partial class ItemTooltip : Control {
    
    public static ItemTooltip Instance { get; private set; }

    [Export] public TextureRect Icon { get; set; }
    [Export] public Label NameLabel { get; set; }
    [Export] public RichTextLabel DescriptionLabel { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public Container StatsContainer { get; set; }
    [Export] public PackedScene StatDisplayScene { get; set; }
    
    private void Open(Texture2D icon, string name, string description) {
        Icon.Texture = icon;
        NameLabel.Text = name;
        DescriptionLabel.Text = description;
        
        foreach (Node child in StatsContainer.GetChildren()) {
            child.QueueFree();
        }
        
        if (AnimationPlayer.IsPlaying()) AnimationPlayer.Stop();
        AnimationPlayer.Play("open");
    }

    public void Open(ItemType itemType) {
        Open(itemType.Icon, itemType.Name, itemType.Description);
        
        foreach (StatValue stat in itemType.Stats) {
            StatDisplay statDisplay = (StatDisplay) StatDisplayScene.Instantiate();
            statDisplay.StatType = stat.Type;
            statDisplay.StatValue = () => stat.Value;
            StatsContainer.AddChild(statDisplay);
        }
    }

    public void Open(Item item) {
        Open(item.Type.Icon, item.Type.Name, item.Type.Description);

        foreach (StatValue statValue in item.Type.Stats) {
            StatDisplay statDisplay = (StatDisplay) StatDisplayScene.Instantiate();
            statDisplay.StatType = statValue.Type;
            statDisplay.StatValue = () => item.GetStat(statValue.Type);
            StatsContainer.AddChild(statDisplay);
        }
    }

    public void Close() {
        if (AnimationPlayer.IsPlaying()) AnimationPlayer.Stop();
        AnimationPlayer.Play("close");
    }
    
    public override void _Ready() {
        Instance = this;
    }

    public void Move(Vector2 globalPosition) {
        ((Control) GetParent()).SetGlobalPosition(globalPosition);
    }
}
