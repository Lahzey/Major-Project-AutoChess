using Godot;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.item;

[ProtoContract]
public abstract partial class ItemEffect : GodotObject, IIdentifiable {
    public string Id { get; set; }

    public abstract void Apply(Item item, UnitInstance unit);
    
    public abstract void Process(Item item, UnitInstance unit, double delta);

    public abstract void Remove(Item item, UnitInstance unit);

}