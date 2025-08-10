using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.session;

namespace MPAutoChess.logic.core.item;

[GlobalClass]
public partial class ItemConfig : Resource {
    
    [Export] public ItemType[] ItemTypes { get; set; }

    public Item? GetCraftingResult(Item itemA, Item itemB) {
        foreach (ItemType itemType in ItemTypes) {
            if (itemType.CraftedFromA == itemA.Type && itemType.CraftedFromB == itemB.Type) return new Item(itemType, itemA, itemB);
            if (itemType.CraftedFromA == itemB.Type && itemType.CraftedFromB == itemA.Type) return new Item(itemType, itemB, itemA);
        }

        return null;
    }

    public ItemType GetRandomItemType(ItemCategory category, ItemType excludeType = null) {
        IEnumerable<ItemType> filteredTypes = ItemTypes.Where(type => type.Category == category && type != excludeType);
        int count = filteredTypes.Count();
        int randomIndex = GameSession.Instance.Random.Next(count);
        return filteredTypes.ElementAt(randomIndex);
    }
    
}