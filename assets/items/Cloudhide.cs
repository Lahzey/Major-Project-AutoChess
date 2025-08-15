using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.assets.items;

[ProtoContract]
public partial class Cloudhide : ItemEffect {
    protected override void Apply(Item item, UnitInstance unit) {
		//TODO, tenacity and stuns not yet implemented
	}

    protected override void Process(Item item, UnitInstance unit, double delta) {
		//TODO, tenacity and stuns not yet implemented
	}

    protected override void Remove(Item item, UnitInstance unit) {
		//TODO, tenacity and stuns not yet implemented
	}
}