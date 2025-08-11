using Godot;
using MPAutoChess.logic.util;

namespace MPAutoChess.logic.core.unit;

public abstract partial class Spell : Node {
    
    public abstract void Cast(UnitInstance caster);

    public virtual string GetName(UnitInstance forUnit) {
        return StringUtil.PascalToReadable(GetType().Name);
    }
    
    public abstract string GetDescription(UnitInstance forUnit);
    
    protected int GetFromLevelArray(Unit caster, int[] array) {
        int level = (int) caster.Level;
        if (level < array.Length) {
            return array[level];
        } else {
            int lastDif = array[^1] - array[^2];
            return array[^1] + lastDif * (level + 1 - array.Length);
        }
    }
    
    protected float GetFromLevelArray(Unit caster, float[] array) {
        if (caster.Level < array.Length) {
            return array[caster.Level];
        } else {
            float lastDif = array[^1] - array[^2];
            return array[^1] + lastDif * (caster.Level + 1 - array.Length);
        }
    }
}