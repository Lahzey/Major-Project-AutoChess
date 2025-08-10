using MPAutoChess.logic.core.session;

namespace MPAutoChess.logic.core.events;

public class PhaseStartEvent : Event {
    
    public GamePhase Phase { get; private set; }
    
    public PhaseStartEvent(GamePhase phase) {
        Phase = phase;
    }
    
}