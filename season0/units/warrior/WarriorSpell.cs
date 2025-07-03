using Godot;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;
using UnitInstance = MPAutoChess.logic.core.unit.UnitInstance;

namespace MPAutoChess.season0.units.Rare;

[GlobalClass, Tool]
public partial class WarriorSpell : Spell {
    
    private float[] DamageScaling { get; set; } = { 3, 3, 3.2f };
    private float[] BaseHealing { get; set; } = { 0.5f, 0.5f, 0.5f };
    private float[] HealingScaling { get; set; } = { 0.005f, 0.005f, 0.005f };
    
    private float GetDamage(UnitInstance caster) {
        return caster.Stats.GetValue(StatType.STRENGTH) * GetFromLevelArray(caster.Unit, DamageScaling);
    }

    private float GetHealing(UnitInstance caster) {
        return GetFromLevelArray(caster.Unit, BaseHealing) + caster.Stats.GetValue(StatType.MAGIC) * GetFromLevelArray(caster.Unit, HealingScaling);
    }
    
    public override void Cast(UnitInstance caster) {
        // TODO: Implement damage logic
    }
}