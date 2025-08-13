using Godot;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.combat;

[ProtoContract]
public struct Target {

    // add properties here to support different target types in the future
    [ProtoMember(1)] public Vector2 FixedTarget { get; private set; } = Vector2.Zero;
    [ProtoMember(2)] public UnitInstance? UnitInstance { get; private set; } = null;

    public Target(UnitInstance unitInstance) {
        UnitInstance = unitInstance;
    }

    public Target(Vector2 fixedTarget) {
        FixedTarget = fixedTarget;
    }

    public Vector2 GetPosition() {
        if (UnitInstance != null) return UnitInstance.Position;
        else return FixedTarget;
    }
    
    public static implicit operator Target(UnitInstance unitInstance) {
        return new Target(unitInstance);
    }
    
    public static implicit operator Target(Vector2 fixedTarget) {
        return new Target(fixedTarget);
    }

    public bool IsValid() {
        if (UnitInstance != null) return Combat.IsValid(UnitInstance);

        return true;
    }
}