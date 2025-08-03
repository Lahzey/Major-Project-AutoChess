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
        
        Node existingNode = ServerController.Instance.GameSession.GetNodeOrNull(surrogate.nodePath);
        if (existingNode == null) {
            throw new ArgumentException($"Failed to deserialize {typeof(T)}. NodeRefSurrogate could not find node at path: {surrogate.nodePath}.");
        } else if (existingNode is T existing) {
            return existing;
        } else {
            throw new ArgumentException($"Failed to deserialize {typeof(T)}. Node at path {surrogate.nodePath} already exists as a different type: {existingNode.GetType()}.");
        }
    }

    public static implicit operator NodeRefSurrogate<T>(T obj) {
        if (obj == null) return null;
        if (obj.GetParent() == null) throw new ArgumentException($"Failed to serialize {typeof(T)}. Cannot serialize Nodes that are not part of the scene tree.");
        GD.Print("Using NodeRefSurrogate for " + typeof(T) + "(" + obj.GetType() + ")");
        return new  NodeRefSurrogate<T> { nodePath = ServerController.Instance.GameSession.GetPathTo(obj) };
    }

}