#nullable enable
using ProtoBuf;

namespace MPAutoChess.logic.core.item;

[ProtoContract]
public class ItemSlot {

    [ProtoMember(1)] private Item? item;
    
    public ItemSlot() {} // for Protobuf serialization
    
    public ItemSlot(Item? item) {
        this.item = item;
    }

    public Item? GetItem() {
        return item;
    }

    public void SetItem(Item? item) {
        this.item = item;
    }
    
}