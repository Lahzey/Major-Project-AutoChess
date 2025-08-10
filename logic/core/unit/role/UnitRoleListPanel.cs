using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.unit.role;

public partial class UnitRoleListPanel : Control {
    
    [Export] public PackedScene RolePanelScene { get; set; }
    
    public Player Player { get; set; }
    
    private List<UnitRolePanel> rolePanels = new List<UnitRolePanel>();

    public override void _Process(double delta) {
        if (Player == null) return;
        Dictionary<UnitRole, HashSet<UnitType>> unitTypesInRoles = Player.Board.GetUnitTypesInAllRoles();
        List<UnitRole> sortedRoles = new List<UnitRole>(unitTypesInRoles.Keys);
        sortedRoles.Sort((a, b) => unitTypesInRoles[a].Count.CompareTo(unitTypesInRoles[b].Count));
        
        while (unitTypesInRoles.Count > rolePanels.Count) {
            UnitRolePanel rolePanel = RolePanelScene.Instantiate<UnitRolePanel>();
            AddChild(rolePanel);
            rolePanels.Add(rolePanel);
        }
        while (unitTypesInRoles.Count < rolePanels.Count) {
            UnitRolePanel rolePanel = rolePanels[^1];
            rolePanel.QueueFree();
            rolePanels.Remove(rolePanel);
        }

        for (int i = 0; i < rolePanels.Count; i++) {
            UnitRole role = sortedRoles[i];
            UnitRolePanel rolePanel = rolePanels[i];
            rolePanel.Player = Player;
            rolePanel.Role = role;
        }
    }
}