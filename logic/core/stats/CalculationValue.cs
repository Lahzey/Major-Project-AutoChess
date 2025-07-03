namespace MPAutoChess.logic.core.stats;

public class CalculationValue : Value {
    
    private Calculation calculation;
    
    public CalculationValue(Calculation calculation) {
        this.calculation = calculation;
    }
    
    public override float Get() {
        return calculation.Evaluate();
    }
}