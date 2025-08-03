using System;
using System.Collections.Generic;
using Godot;

namespace MPAutoChess.logic.core.session;

public partial class LootPhaseUI : VBoxContainer {
    
    [Export] public Label TitleLabel { get; set; }
    [Export] public Container LootOptionsContainer { get; set; }
    [Export] public PackedScene LootOptionScene { get; set; }
    
    private List<LootOptionPanel> lootOptions = new List<LootOptionPanel>();

    public void AddLootOption(Texture2D texture, string name, string description, Action onClick, Func<bool> isEnabled = null) {
        LootOptionPanel lootOption = (LootOptionPanel) LootOptionScene.Instantiate();
        lootOption.Texture.Texture = texture;
        lootOption.ChooseButton.TooltipText = description;
        lootOption.Label.Text = name;
        lootOption.ChooseButton.Pressed += onClick;
        lootOption.IsEnabled = isEnabled;
        LootOptionsContainer.AddChild(lootOption);
        lootOptions.Add(lootOption);
    }
    
    
}