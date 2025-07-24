using Godot;
using ProtoBuf;

namespace MPAutoChess.logic.core.unit;

[GlobalClass, ProtoContract]
public partial class UnitCollection : Resource {
    
    [Export] public UnitType[] CommonUnits { get; set; }
    [Export] public UnitType[] UncommonUnits { get; set; }
    [Export] public UnitType[] RareUnits { get; set; }
    [Export] public UnitType[] EpicUnits { get; set; }
    [Export] public UnitType[] LegendaryUnits { get; set; }
    
}