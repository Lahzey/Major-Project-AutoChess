using System;
using Godot;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.stats;
using ProtoBuf;

namespace MPAutoChess.logic.core.item;

[ProtoContract]
public class Item : IIdentifiable {
    public string Id { get; set; }

    [ProtoMember(1)] public ItemType Type { get; set; }
    [ProtoMember(2)] public Tuple<int, int> ComponentLevels { get; set; }
    [ProtoMember(3)] public ItemEffect? Effect { get; private set; }
    
    public int Level => ComponentLevels.Item1 + ComponentLevels.Item2;

    public Item() { } // for Protobuf serialization

    public Item(ItemType type, int level = 0) {
        Type = type;
        int halfLevel = level / 2;
        ComponentLevels = new Tuple<int, int>(halfLevel + (level % 2), halfLevel);
        Effect = Type.EffectScript?.New().As<ItemEffect>();
    }

    public Item(ItemType type, Item fromA, Item fromB) {
        Type = type;
        ComponentLevels = new Tuple<int, int>(fromA.Level, fromB.Level);
        Effect = Type.EffectScript?.New().As<ItemEffect>();
    }
    
    public float ScaleValue(float value) {
        // scales the value based on the item level
        float halfValue = value * 0.5f; // each level increases the stat by 50% of the base value
        return value + halfValue * Level;
    }

    public float GetStat(StatType type) {
        foreach (StatValue stat in Type.Stats) {
            if (stat.StatType == type) {
                return ScaleValue(stat.Value);
            }
        }

        return 0f;
    }

    public void Upgrade() {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("Upgrade can only be called on the server.");
        
        bool upgradeComponent1 = GameSession.Instance.Random.NextSingle() < 0.5f;
        ComponentLevels = new  Tuple<int, int>(ComponentLevels.Item1 + (upgradeComponent1 ? 1 : 0), ComponentLevels.Item2 + (upgradeComponent1 ? 0 : 1));
        ServerController.Instance.PublishChange(this);
    }

    public string GetName() {
        return Type.Name + new string('+', Level);
    }

    public string GetDescription() {
        return Type.Description;
    }

    public int GetSellValue() {
        int sellValue = Type.Category switch {
            ItemCategory.COMPONENT => 5,
            ItemCategory.ITEM => 9,
            ItemCategory.MYTHICAL_COMPONENT => 15,
            ItemCategory.MYTHICAL_ITEM => 28,
        };
        int sellValuePerLevel = 10;

        return sellValue + (Level * sellValuePerLevel);
    }
}