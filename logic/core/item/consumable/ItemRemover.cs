using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.item.consumable;

public class ItemRemover : Consumable {
    
    private static readonly Texture2D ICON = ResourceLoader.Load<Texture2D>("res://assets/ui/magnet.png");

    public override Texture2D GetIcon() {
        return ICON;
    }
    
    public override bool IsValidTarget(object target, int extraChoice) {
        if (target is not UnitInstance unitInstance) return false;
        Player player = PlayerController.Current.Player;
        return !unitInstance.IsCombatInstance && unitInstance.Unit.Container.GetPlayer() == player && unitInstance.Unit.EquippedItems.Count > 0;
    }
    
    public override bool Consume(object target, int extraChoice) {
        if (!IsValidTarget(target, extraChoice)) return false;
        ((UnitInstance) target).Unit.RemoveItems();
        return true;
    }
    
}