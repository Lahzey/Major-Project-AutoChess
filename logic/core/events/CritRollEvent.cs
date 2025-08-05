using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.events;

public class CritRollEvent : CancellableEvent {
    
    public UnitInstance Source { get; set; }
    
    // before crit roll
    public float CritChance { get; set; }
    public bool CanOverCrit { get; set; }
    
    // after crit roll
    public int CritLevel { get; set; }
    
    public CritRollEvent(UnitInstance source, float critChance, bool canOverCrit) {
        Source = source;
        CritChance = critChance;
        CanOverCrit = canOverCrit;
    }
    
}