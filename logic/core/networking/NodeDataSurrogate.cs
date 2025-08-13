using System;
using Godot;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.util;
using ProtoBuf;
using Environment = System.Environment;

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
        Node existingNode = SerializerExtensions.FindNode(nodePath);
        if (existingNode == null) {
            GD.PrintErr($"Failed to deserialize {typeof(T)}. Could not find node at path '{nodePath}'.");
            GD.PrintErr(NodeStructure.root);
            throw new InvalidOperationException($"Failed to deserialize {typeof(T)}. Could not find node at path '{nodePath}'.");
        }
        
        if (existingNode is not T existing) {
            throw new ArgumentException($"Failed to deserialize {typeof(T)}. Node at path '{nodePath}' is of type {existingNode.GetType()}.");
        }
        return existing;
    }
    
    public static implicit operator T(NodeDataSurrogate<T> surrogate) {
        return surrogate?.FromMirror(surrogate.data);
    }

    public static implicit operator NodeDataSurrogate<T>(T obj) {
        if (obj == null) return null;
        
        string path = SerializerExtensions.GetNodePath(obj);
        if (path != null) NodeStructure.RegisterNode(path.Split('/'));
        
        NodeDataSurrogate<T> result = new NodeDataSurrogate<T> {
            nodePath = path,
            packedScenePath = obj.SceneFilePath,
            data = ToMirror(obj)
        };
        
        return result;
    }
}