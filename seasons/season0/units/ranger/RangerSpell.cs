using Godot;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using UnitInstance = MPAutoChess.logic.core.unit.UnitInstance;

namespace MPAutoChess.seasons.season0.units.ranger;

[GlobalClass, Tool]
public partial class RangerSpell : Spell {
    
    private float[] AttackSpeedGain { get; set; } = { 2, 2, 2 };
    private float[] BaseDuration { get; set; } = { 4, 4, 5 };
    private float[] DurationScaling { get; set; } = { 0.1f, 0.1f, 0.1f };
    
    private float GetAttackSpeedGain(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, AttackSpeedGain);
    }

    private float GetDuration(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, BaseDuration) + caster.Stats.GetValue(StatType.MAGIC) * GetFromLevelArray(caster.Unit, DurationScaling);
    }

    public override void Cast(UnitInstance caster) {
        throw new System.NotImplementedException();
    }
}