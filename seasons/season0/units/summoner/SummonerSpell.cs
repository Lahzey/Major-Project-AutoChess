using Godot;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using UnitInstance = MPAutoChess.logic.core.unit.UnitInstance;

namespace MPAutoChess.seasons.season0.units.summoner;

[GlobalClass, Tool]
public partial class SummonerSpell : Spell {
    
    private float[] SummonCount { get; set; } = { 2, 3, 20 };
    private float[] SummonBaseHealth { get; set; } = { 100, 150, 1000 };
    private float[] SummonHealthScaling { get; set; } = { 5, 5, 30 };
    private float[] SummonBaseAttack { get; set; } = { 20, 50, 300 };
    private float[] SummonAttackScaling { get; set; } = { 1, 1, 1 };
    
    private float GetSummonCount(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, SummonCount);
    }
    
    private float GetSummonBaseHealth(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, SummonBaseHealth) + caster.Stats.GetValue(StatType.MAGIC) * GetFromLevelArray(caster.Unit, SummonHealthScaling);
    }
    
    private float GetSummonBaseAttack(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, SummonBaseAttack) + caster.Stats.GetValue(StatType.STRENGTH) * GetFromLevelArray(caster.Unit, SummonAttackScaling);
    }

    public override void Cast(UnitInstance caster) {
        throw new System.NotImplementedException();
    }
}