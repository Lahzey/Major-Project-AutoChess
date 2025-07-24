using Godot;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using UnitInstance = MPAutoChess.logic.core.unit.UnitInstance;

namespace MPAutoChess.seasons.season0.units.tank;

[GlobalClass, Tool]
public partial class TankSpell : Spell {
    
    private float[] BaseHealAmount { get; set; } = { 100, 200, 300 };
    private float[] HealScaling { get; set; } = { 2, 2, 3 };
    
    private float GetHealAmount(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, BaseHealAmount) + caster.Stats.GetValue(StatType.MAGIC) * GetFromLevelArray(caster.Unit, HealScaling);
    }
    
    public override void Cast(UnitInstance caster) {
        caster.Heal(GetHealAmount(caster));
    }
}