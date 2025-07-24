using System;
using Godot;
using ProtoBuf;

namespace MPAutoChess.logic.core.stats;

[ProtoContract]
[ProtoInclude(1, typeof(ConstantValue))]
public abstract class Value {

    public abstract float Get();

    public abstract Value Clone();
}