using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.combat;

[ProtoContract]
public struct DamageSource {

    // add properties here to support different source types in the future
    [ProtoMember(1)] public UnitInstance? UnitInstance { get; private set; }

    public DamageSource(UnitInstance unitInstance) {
        UnitInstance = unitInstance;
    }
    
    public static implicit operator DamageSource(UnitInstance unitInstance) {
        return new DamageSource(unitInstance);
    }


    public bool IsValid() {
        if (UnitInstance != null) return Combat.IsValid(UnitInstance);
        
        return false;
    }
}