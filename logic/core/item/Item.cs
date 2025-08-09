using System;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.stats;
using ProtoBuf;

namespace MPAutoChess.logic.core.item;

[ProtoContract]
public class Item : IIdentifiable {
    public string Id { get; set; }

    [ProtoMember(1)] public ItemType Type { get; set; }
    [ProtoMember(2)] public int Level { get; set; } = 1;

    public Item() { } // for Protobuf serialization

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

    public void Upgrade() {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("Upgrade can only be called on the server.");
        Level++;
        ServerController.Instance.PublishChange(this);
    }
}