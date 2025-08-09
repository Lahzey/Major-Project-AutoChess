using MPAutoChess.logic.core.networking;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
public partial class LiveMode : GameMode {
    public override void Tick(double delta) {
        if (ServerController.Instance.IsServer && GetCurrentPhase().IsFinished()) AdvancePhase();
    }

    protected override GamePhase GetNextPhase() {
        int nextPhaseIndex = GetCurrentPhaseIndex() + 1;
        if (nextPhaseIndex % 2 == 0) {
            return LootPhase.Random();
        } else {
            return new CombatPhase();
        }
    }

    public override double GetDefaultPhaseTime() {
        return 60.0;
    }
}