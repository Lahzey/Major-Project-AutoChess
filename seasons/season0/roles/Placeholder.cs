using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.seasons.season0.roles;

public class Placeholder : UnitRole {
    
    private static readonly Texture2D ICON = ResourceLoader.Load<Texture2D>("res://seasons/season0/roles/placeholder.png");
    
    private static readonly int[] THRESHOLDS = { 2, 4 }; // Example thresholds, can be adjusted as needed
    private static readonly float[] DOUBLE_ATTACK_CHANCES = { 0.25f, 0.5f, };

    public Placeholder() {
        if (!ServerController.Instance.IsServer) return;
        EventManager.INSTANCE.AddAfterListener<AttackEvent>(OnAttack);
    }

    public override string GetDescription() {
        return "Placeholder units have a chance to trigger another attack upon attacking.";
    }

    public override Texture2D GetIcon() {
        return ICON;
    }

    private void OnAttack(AttackEvent attackEvent) {
        if (!attackEvent.Source.Unit.HasRole(this)) return;
            
        int roleCount = attackEvent.Source.CurrentCombat.GetRoleCount(this, attackEvent.Source.IsInTeamA);
        int level = GetLevel(roleCount);
        if (level == 0) return;
            
        if (GameSession.Instance.Random.NextSingle() < DOUBLE_ATTACK_CHANCES[level - 1]) {
            attackEvent.Source.AttackCooldown = 0;
        }
    }

    public override void Apply(IEnumerable<UnitInstance> units) {
        
    }

    public override int[] GetCountThresholds() {
        return THRESHOLDS;
    }
}