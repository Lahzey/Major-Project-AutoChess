namespace MPAutoChess.logic.core.events;

public abstract class CancellableEvent : Event {
    
    public bool IsCancelled { get; private set; } = false;

    public void Cancel() {
        IsCancelled = true;
    }
    
}