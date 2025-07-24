using Godot;
using MPAutoChess.logic.core;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.seasons.season0;

[ProtoContract]
public class Season0 : Season {
    
    [ProtoMember(1)] private UnitCollection units = GD.Load<UnitCollection>("res://seasons/season0/season0_units.tres");
    
    public override UnitCollection GetUnits() {
        return units;
    }
    
}