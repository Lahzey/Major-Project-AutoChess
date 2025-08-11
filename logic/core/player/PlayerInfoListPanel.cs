using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.session;

namespace MPAutoChess.logic.core.player;

public partial class PlayerInfoListPanel : Control {
    
    [Export] public PackedScene PlayerInfoPanelScene { get; set; }
    
    private List<PlayerInfoPanel> playerInfoPanels = new List<PlayerInfoPanel>();
    
    public override void _Process(double delta) {
        if (GameSession.Instance == null) return;
        
        while (playerInfoPanels.Count < GameSession.Instance.Players.Length) {
            PlayerInfoPanel panel = PlayerInfoPanelScene.Instantiate<PlayerInfoPanel>();
            playerInfoPanels.Add(panel);
            AddChild(panel);
        }

        while (playerInfoPanels.Count > GameSession.Instance.Players.Length) {
            PlayerInfoPanel panel = playerInfoPanels[^1];
            panel.QueueFree();
            playerInfoPanels.Remove(panel);
        }
        
        for (int i = 0; i < playerInfoPanels.Count; i++) {
            PlayerInfoPanel panel = playerInfoPanels[i];
            panel.Player = GameSession.Instance.Players[i];
        }
    }
}