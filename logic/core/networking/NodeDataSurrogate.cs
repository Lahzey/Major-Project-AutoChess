using System;
using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.placement;
using ProtoBuf;

namespace MPAutoChess.logic.core.networking;

[ProtoContract]
public class NodeDataSurrogate<T> : MergeSurrogateBase<T> where T : Node, new() {
    
    [ProtoMember(1)] public string nodePath;
    [ProtoMember(2)] public string packedScenePath;
    [ProtoMember(3)] public TypeMirror data;

    static NodeDataSurrogate() {
        InitMirrorType();
    }

    protected override T GetDeserializationTarget() {
        Node existingNode = ServerController.Instance.GameSession.GetNode(nodePath);
        if (existingNode == null) {
            PackedScene packedScene = packedScenePath != null && packedScenePath.Length > 0 ? ResourceLoader.Load<PackedScene>(packedScenePath) : null;
            GD.PrintErr($"Deserialized Node at path {nodePath} does not exist in the scene tree. New instance was created from {(packedScene == null ? "constructor" : packedScenePath)} to prevent crash, but it is highly recommended to have an instance ready for merging.");
            return (T) GetEmptyConstructor().Invoke(Array.Empty<object>());
        }
        
        if (existingNode is not T existing) {
            throw new ArgumentException("Path mismatch: Node at path " + nodePath + " already exists as a different type: " + existingNode.GetType() + " instead of " + typeof(T));
        }
        return existing;
    }
    
    public static implicit operator T(NodeDataSurrogate<T> surrogate) {
        if (surrogate == null) return null;
        if (surrogate.nodePath == null) throw new ArgumentException($"Failed to deserialize {typeof(T)}: NodeDataSurrogate must have an nodePath set.");

        GD.Print($"Deserialized {typeof(T)} from\n    " + surrogate.data.ToString().Replace("\n", "\n    "));
        
        return surrogate.FromMirror(surrogate.data);
    }

    public static implicit operator NodeDataSurrogate<T>(T obj) {
        if (obj == null) return null;
        
        NodePath path = ServerController.Instance.GameSession.GetPathTo(obj);
        if (path == null || path.IsEmpty) throw new ArgumentException("Failed to serialize node of type " + obj.GetType() + " because it is not attached to a parent node.");
        
        GD.Print("Using NodeDataSurrogate for " + typeof(T) + "(" + obj.GetType() + ") at path " + path);
        
        var result = new NodeDataSurrogate<T> {
            nodePath = path,
            packedScenePath = obj.SceneFilePath,
            data = ToMirror(obj)
        };

        GD.Print($"Serialized {typeof(T)} as\n    " + result.data.ToString().Replace("\n", "\n    "));
        
        return result;
    }
}