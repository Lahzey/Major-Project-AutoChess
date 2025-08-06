namespace MPAutoChess.logic.core.events;

public abstract class Event {
    
    public virtual bool RunsOnClient => false;
    public virtual bool RunsOnServer => true;
    
}