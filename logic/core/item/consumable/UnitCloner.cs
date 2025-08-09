using Godot;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.item.consumable;

public class UnitCloner : Consumable {
    
    private static readonly Texture2D ICON = ResourceLoader.Load<Texture2D>("res://assets/ui/unit_cloner.png");

    public override Texture2D GetIcon() {
        return ICON;
    }
    
    public override bool IsValidTarget(object target, int extraChoice) {
        if (target is not UnitInstance unitInstance) return false;
        Player player = PlayerController.Current.Player;
        return !unitInstance.IsCombatInstance && unitInstance.Unit.Container.GetPlayer() == player && unitInstance.Unit.Type.Cost > 0; // non-buyable units have a cost of 0
    }
    
    public override bool Consume(object target, int extraChoice) {
        if (!IsValidTarget(target, extraChoice)) return false;
        if (target is UnitInstance unitInstance) {
            SingleUnitSlot? targetSlot = PlayerController.Current.Player.Bench.GetFirstFreeSlot();
            Unit createdUnit = UnitPool.For(unitInstance.Unit.Type).TryTakeUnit(unitInstance.Unit.Type, true);
            if (targetSlot != null) targetSlot.AddUnit(createdUnit, Vector2.Zero);
            else PlayerController.Current.Player.MoveToTemporaryBench(createdUnit);
            return true;
        }
        
        // IsValidTarget should prevent this code from ever being reached
        return false;
    }
}