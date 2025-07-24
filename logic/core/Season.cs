using MPAutoChess.logic.core.unit;
using MPAutoChess.seasons.season0;
using ProtoBuf;

namespace MPAutoChess.logic.core;

[ProtoContract]
[ProtoInclude(1, typeof(Season0))]
public abstract class Season {
    
    public abstract UnitCollection GetUnits();
    
}