using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.session;

namespace MPAutoChess.logic.core.item;

[GlobalClass]
public partial class ItemConfig : Resource {
    
    [Export] public ItemType[] ItemTypes { get; set; }

    public ItemType? GetRecipeFor(ItemType typeA, ItemType typeB) {
        foreach (ItemType itemType in ItemTypes) {
            if (itemType.CraftedFromA == typeA && itemType.CraftedFromB == typeB) return itemType;
            if (itemType.CraftedFromA == typeB && itemType.CraftedFromB == typeA) return itemType;
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