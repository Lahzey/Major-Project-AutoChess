using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.events;

public class PlayerDamageEvent : Event {
    
    public Player? Source { get; private set; } // null if the damage is either from a clone fight or from other sources (like loot downsides)
    public Player Target { get; private set; }
    public float Damage { get; set; } // will be rounded to the nearest integer when applied
    
    public PlayerDamageEvent(Player? source, Player target, float damage) {
        Source = source;
        Target = target;
        Damage = damage;
    }
    
}