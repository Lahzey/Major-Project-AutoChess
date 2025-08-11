using MPAutoChess.logic.core.session;

namespace MPAutoChess.logic.core.events;

public class PhaseChangeEvent : Event {
    
    public GamePhase PreviousPhase { get; private set; }
    public GamePhase NextPhase { get; private set; }

    public override bool RunsOnServer => true;
    public override bool RunsOnClient => true;

    public PhaseChangeEvent(GamePhase previousPhase, GamePhase nextPhase) {
        PreviousPhase = previousPhase;
        NextPhase = nextPhase;
    }
    
}