using Godot;
using MPAutoChess.logic.core;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.seasons.season0;

[ProtoContract]
public class Season0 : Season {
    
    [ProtoMember(1)] private UnitCollection units = GD.Load<UnitCollection>("res://seasons/season0/season0_units.tres");
    [ProtoMember(2)] private ItemConfig itemConfig = GD.Load<ItemConfig>("res://seasons/season0/season0_items.tres");
    
    public override UnitCollection GetUnits() {
        return units;
    }

    public override ItemConfig GetItemConfig() {
        return itemConfig;
    }
}