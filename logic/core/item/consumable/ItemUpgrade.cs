using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;
using MPAutoChess.logic.menu;

namespace MPAutoChess.logic.core.item.consumable;

public class ItemUpgrade : Consumable {
    
    private static readonly Texture2D ICON = ResourceLoader.Load<Texture2D>("res://assets/ui/item_upgrade.png");

    public override Texture2D GetIcon() {
        return ICON;
    }

    protected override void RequestConsume(object target, int extraChoice) { // when upgrading an item on a unit, the client must make an extra choice, so we cannot send the request right away
        if (target is UnitInstance unitInstance) {
            ChoicePopup.Choice[] choices = new ChoicePopup.Choice[unitInstance.Unit.EquippedItems.Count];
            for (int i = 0; i < unitInstance.Unit.EquippedItems.Count; i++) {
                Item item = unitInstance.Unit.EquippedItems[i];
                choices[i] = new ChoicePopup.Choice(item.Type.Icon, item.Type.Name);
            }
            ChoicePopup.Instance.Open("Select an item to upgrade", choices, (int choiceIndex) => {
                PlayerController.Current.UseConsumable(this, unitInstance, choiceIndex);
            });
        } else if (target is Inventory inventory) {
            PlayerController.Current.UseConsumable(this, inventory, extraChoice);
        }
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
            Item item = unitInstance.Unit.EquippedItems[extraChoice];
            item.Upgrade();
            return true;
        } else if (target is Inventory inventory) {
            inventory.GetItem(extraChoice).Upgrade();
            return true;
        }
        
        return false; // IsValidTarget should prevent this code from ever being reached
    }
    
}