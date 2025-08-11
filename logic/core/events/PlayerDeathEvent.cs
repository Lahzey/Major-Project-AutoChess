using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.events;

public class PlayerDeathEvent : CancellableEvent {
    
    public Player Player { get; private set; }

    public PlayerDeathEvent(Player player) {
        Player = player;
    }
    
    
}