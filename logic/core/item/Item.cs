using MPAutoChess.logic.core.stats;
using ProtoBuf;

namespace MPAutoChess.logic.core.item;

[ProtoContract]
public class Item {

    [ProtoMember(1)] public ItemType Type { get; set; }
    [ProtoMember(2)] public int Level { get; set; } = 1;

    private Item() { }

    public Item(ItemType type) {
        Type = type;
    }

    public float GetStat(StatType type) {
        foreach (StatValue stat in Type.Stats) {
            if (stat.Type == type) {
                float halfValue = stat.Value * 0.5f; // each level increases the stat by 50% of the base value
                return halfValue + halfValue * Level;
            }
        }

        return 0f;
    }
}