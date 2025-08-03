using System;
using Godot;
using ProtoBuf;

namespace MPAutoChess.logic.core.stats;

[ProtoContract]
[ProtoInclude(100, typeof(ConstantValue))]
public abstract class Value {

    public abstract float Get();

    public abstract Value Clone();
    
    public static implicit operator Value(float value) {
        return new ConstantValue(value);
    }
    
}