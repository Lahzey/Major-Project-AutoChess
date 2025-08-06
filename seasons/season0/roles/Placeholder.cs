using MPAutoChess.logic.core.events;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.seasons.season0.roles;

public class Placeholder : UnitRole {

    public Placeholder() {
        if (!ServerController.Instance.IsServer) return;
        EventManager.INSTANCE.AddAfterListener((AttackEvent attackEvent) => {
            if (attackEvent.Source.Unit.Type.Roles.HasRole<Placeholder>()) {
                if (GameSession.Instance.Random.NextSingle() < 0.5) { // 50% chance
                    attackEvent.Source.AttackCooldown = 0;
                }
            }
        });
    }
    
    public override void OnBoardUpdate(Board board) {
    }

    public override void OnCombatStart(Board board) {
    }
    public override void OnCombatEnd(Board board) {
    }

    public override string GetDescription() {
        return "Placeholder units have a chance to trigger another attack upon attacking.";
    }

    public override int[] GetCountThresholds() {
        return new[] { 2, 4 };
    }
}