namespace MPAutoChess.logic.core.stats;

public class ConstantValue : Value {
    
    private float value;
    
    public ConstantValue(float value) {
        this.value = value;
    }
    
    public override float Get() {
        return value;
    }
    
}