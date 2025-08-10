using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.item.consumable;

public class ItemReroll : Consumable {
    
    private static readonly Texture2D ICON = ResourceLoader.Load<Texture2D>("res://assets/ui/item_reroll.png");

    public override Texture2D GetIcon() {
        return ICON;
    }

    public override bool IsValidTarget(object target, int extraChoice) {
        Player player = PlayerController.Current.Player;
        if (target is UnitInstance unitInstance) {
            return !unitInstance.IsCombatInstance && unitInstance.Unit.Container.GetPlayer() == player && unitInstance.Unit.EquippedItems.Count > 0;
        } else if (target is Inventory inventory) {
            return inventory.Player == player && inventory.GetItem(extraChoice) != null;
        }
        return false;
    }
    
    public override bool Consume(object target, int extraChoice) {
        if (!IsValidTarget(target, extraChoice)) return false;
        if (target is UnitInstance unitInstance) {
            foreach (Item item in unitInstance.Unit.EquippedItems) {
                ItemType newType = GameSession.Instance.Season.GetItemConfig().GetRandomItemType(item.Type.Category, item.Type);
                Item newItem = new Item(newType);
                newItem.ComponentLevels = item.ComponentLevels; // keep component levels
                unitInstance.Unit.ReplaceItem(item, newItem);
            }
            unitInstance.Unit.RemoveItems(); // put them back into the inventory for convenience
            return true;
        } else if (target is Inventory inventory) {
            Item item = inventory.GetItem(extraChoice);
            ItemType newType = GameSession.Instance.Season.GetItemConfig().GetRandomItemType(item.Type.Category, item.Type);
            Item newItem = new Item(newType);
            inventory.ReplaceItem(extraChoice, newItem);
            return true;
        }
        
        // IsValidTarget should prevent this code from ever being reached
        return false;
    }
}