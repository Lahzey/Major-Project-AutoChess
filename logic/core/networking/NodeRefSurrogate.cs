using System;
using Godot;
using ProtoBuf;

namespace MPAutoChess.logic.core.networking;

[ProtoContract]
public class NodeRefSurrogate<T> where T : Node {
    
    [ProtoMember(1)] public string nodePath;

    public static implicit operator T?(NodeRefSurrogate<T> surrogate) {
        if (surrogate == null) return null;
        if (surrogate.nodePath == null) throw new ArgumentException("NodeRefSurrogate must have an nodePath set.");

        return ServerController.Instance.GetNode<T>(surrogate.nodePath);
    }

    public static implicit operator NodeRefSurrogate<T>(T obj) {
        if (obj == null) return null;
        if (obj.GetParent() == null) throw new ArgumentException("Cannot serialize Nodes that are not part of the scene tree.");
        GD.Print("Using NodeRefSurrogate for " + typeof(T) + "(" + obj.GetType() + ")");
        return new  NodeRefSurrogate<T> { nodePath = obj.GetPath() };
    }

}