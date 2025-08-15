using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class HoardersPot : ItemEffect {

    // only gives stats
    protected override void Apply(Item item, UnitInstance unit) {}
    protected override void Process(Item item, UnitInstance unit, double delta) {}
    protected override void Remove(Item item, UnitInstance unit) {}
}