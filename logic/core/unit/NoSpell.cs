using Godot;

namespace MPAutoChess.logic.core.unit;

[Tool]
public partial class NoSpell : Spell {
    public override float GetCastTime(UnitInstance caster) {
        return 0f; // No cast time for NoSpell
    }
    public override void Cast(UnitInstance caster, UnitInstance target) {
        
    }
    public override string GetDescription(UnitInstance forUnit) {
        return "[i]This unit has no spell.[/i]";
    }
}