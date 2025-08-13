using Godot;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using UnitInstance = MPAutoChess.logic.core.unit.UnitInstance;

namespace MPAutoChess.seasons.season0.units.summoner;

[GlobalClass, Tool]
public partial class SummonerSpell : Spell {
    
    [Export] public UnitType SummonType { get; set; } = ResourceLoader.Load<UnitType>("res://seasons/season0/units/summoner/summoner_pet_type.tres");
    
    private int[] SummonCount { get; set; } = { 2, 3, 20 };
    private float[] SummonBaseHealth { get; set; } = { 100, 150, 1000 };
    private float[] SummonHealthScaling { get; set; } = { 5, 5, 30 };
    private float[] SummonBaseStrength { get; set; } = { 20, 50, 300 };
    private float[] SummonStrengthScaling { get; set; } = { 1, 1, 1 };
    
    private int GetSummonCount(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, SummonCount);
    }
    
    private float GetSummonBaseHealth(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, SummonBaseHealth) + caster.Stats.GetValue(StatType.MAGIC) * GetFromLevelArray(caster.Unit, SummonHealthScaling);
    }
    
    private float GetSummonBaseAttack(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, SummonBaseStrength) + caster.Stats.GetValue(StatType.STRENGTH) * GetFromLevelArray(caster.Unit, SummonStrengthScaling);
    }

    public override float GetCastTime(UnitInstance caster) {
        return 0.5f;
    }

    public override bool RequiresTarget(UnitInstance caster) {
        return false;
    }

    public override void Cast(UnitInstance caster, UnitInstance? target) {
        for (int i = 0; i < GetSummonCount(caster); i++) {
            Unit unit = new Unit(SummonType, null);
            unit.Stats.GetCalculation(StatType.MAX_HEALTH).BaseValue = GetSummonBaseHealth(caster);
            unit.Stats.GetCalculation(StatType.STRENGTH).BaseValue = GetSummonBaseAttack(caster);
            UnitInstance summon = unit.CreateInstance(true);
            caster.CurrentCombat.SummonUnit(summon, caster.Position, caster.IsInTeamA);
        }
    }

    public override string GetDescription(UnitInstance forUnit) {
        int summonCount = GetSummonCount(forUnit);
        return $"Summons {summonCount} minions which fight for you. Minion Stats:\n" +
               $"Health: {StatType.MAX_HEALTH.ToString(GetSummonBaseHealth(forUnit))}\n" +
               $"Strength: {StatType.STRENGTH.ToString(GetSummonBaseAttack(forUnit))}";
    }
}