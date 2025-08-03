using System;
using Godot;
using MPAutoChess.logic.util;
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
        if (string.IsNullOrEmpty(nodePath)) {
            if (ServerController.Instance.GameSession is T gameSession) return gameSession;
            else throw new ArgumentException($"Failed to deserialize {typeof(T)}. Node with empty path must always be a GameSession.");
        }
        
        Node existingNode = ServerController.Instance.GameSession.GetNodeOrNull(nodePath);
        if (existingNode == null) {
            throw new InvalidOperationException($"Failed to deserialize {typeof(T)}. Could not find node at path '{nodePath}'.");
        }
        
        if (existingNode is not T existing) {
            throw new ArgumentException($"Failed to deserialize {typeof(T)}. Node at path '{nodePath}' is of type {existingNode.GetType()}.");
        }
        GD.Print("Using existing " + typeof(T) + " at path '" + nodePath + "'.");
        return existing;
    }
    
    public static implicit operator T(NodeDataSurrogate<T> surrogate) {
        if (surrogate == null) return null;

        // GD.Print($"Deserialized {typeof(T)} from\n    " + surrogate.data.ToString().Replace("\n", "\n    "));
        
        return surrogate.FromMirror(surrogate.data);
    }

    public static implicit operator NodeDataSurrogate<T>(T obj) {
        if (obj == null) return null;
        
        string path;
        if (obj == ServerController.Instance.GameSession) {
            path = null;
        } else if (ServerController.Instance.GameSession.IsAncestorOf(obj)) {
            path = ServerController.Instance.GameSession.GetPathTo(obj);
            NodeStructure.RegisterNode(path.Split('/'));
        } else {
            throw new ArgumentException($"Failed to serialize {typeof(T)}. Node must be a child of the GameSession or the GameSession itself.");
        }
        
        // GD.Print("Using NodeDataSurrogate for " + typeof(T) + "(" + obj.GetType() + ") at path '" + path + "' with packed scene path '" + obj.SceneFilePath + "'.");
        
        var result = new NodeDataSurrogate<T> {
            nodePath = path,
            packedScenePath = obj.SceneFilePath,
            data = ToMirror(obj)
        };

        // GD.Print($"Serialized {typeof(T)} as\n    " + result.data.ToString().Replace("\n", "\n    "));
        
        return result;
    }
}