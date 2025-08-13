using Godot;
using MPAutoChess.logic.core.combat;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using UnitInstance = MPAutoChess.logic.core.unit.UnitInstance;

namespace MPAutoChess.seasons.season0.units.ranger;

[GlobalClass, Tool]
public partial class RangerSpell : Spell {
    
    private const string BUFF_ID = "ranger_spell_attack_speed_buff";
    
    private float[] AttackSpeedGain { get; set; } = { 2, 2, 2 };
    private float[] BaseDuration { get; set; } = { 4, 4, 5 };
    private float[] DurationScaling { get; set; } = { 0.1f, 0.1f, 0.1f };
    
    private float GetAttackSpeedGain(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, AttackSpeedGain);
    }

    private float GetDuration(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, BaseDuration) + caster.Stats.GetValue(StatType.MAGIC) * GetFromLevelArray(caster.Unit, DurationScaling);
    }

    public override float GetCastTime(UnitInstance caster) {
        return 0.1f;
    }

    public override bool RequiresTarget(UnitInstance caster) {
        return false;
    }

    public override async void Cast(UnitInstance caster, UnitInstance? target) {
        caster.Stats.GetCalculation(StatType.BONUS_ATTACK_SPEED).AddFlat(GetAttackSpeedGain(caster), BUFF_ID);
        await ToSignal(GetTree().CreateTimer(GetDuration(caster)), "timeout");
        if (Combat.IsValid(caster)) caster.Stats.GetCalculation(StatType.BONUS_ATTACK_SPEED).RemoveFlat(BUFF_ID);
    }

    public override string GetDescription(UnitInstance forUnit) {
        return $"Focuses on his attacks, gaining {StatType.BONUS_ATTACK_SPEED.ToString(GetAttackSpeedGain(forUnit))} for {GetDuration(forUnit):1} seconds.";
    }
}