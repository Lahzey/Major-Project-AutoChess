using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Godot;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.session;
using ProtoBuf;

namespace MPAutoChess.logic.util;

public static class SerializerExtensions {
    
    public static byte[] Serialize<T>(T obj) {
        NodeStructure.InitializeRoot();

        // serialize actual data
        Stopwatch stopwatch = Stopwatch.StartNew();
        using MemoryStream contentStream = new MemoryStream();
        Serializer.Serialize(contentStream, obj);
        byte[] content = contentStream.ToArray();
        stopwatch.Stop();
        GD.Print($"Serialized {typeof(T).Name} in {stopwatch.ElapsedMilliseconds}ms, size: {content.Length} bytes");

        //  serialize the node structure so it can potentially be recreated on the client
        stopwatch.Restart();
        using MemoryStream headerStream = new MemoryStream();
        Serializer.Serialize(headerStream, NodeStructure.root);
        byte[] header = headerStream.ToArray();
        byte[] headerLength = BitConverter.GetBytes(header.Length);

        byte[] result = new byte[headerLength.Length + header.Length + content.Length];
        Buffer.BlockCopy(headerLength, 0, result, 0, headerLength.Length);
        Buffer.BlockCopy(header, 0, result, headerLength.Length, header.Length);
        Buffer.BlockCopy(content, 0, result, headerLength.Length + header.Length, content.Length);
        stopwatch.Stop();
        GD.Print($"Serialization of header and copy to result took {stopwatch.ElapsedMilliseconds}ms, final size: {result.Length} bytes");
        
        NodeStructure.root = null; // allow garbage collection of the root node structure

        return result;
    }
    
    public static T Deserialize<T>(byte[] data, Type type = null) {
        // deserialize the header
        Stopwatch stopwatch = Stopwatch.StartNew();
        int length = BitConverter.ToInt32(data, 0);
        using MemoryStream headerStream = new MemoryStream(data, sizeof(int), length);
        NodeStructure root = Serializer.Deserialize<NodeStructure>(headerStream);
        root.EnsureCreated(null);
        stopwatch.Stop();
        GD.Print($"Deserialized NodeStructure in {stopwatch.ElapsedMilliseconds}ms, size: {length + sizeof(int)} bytes");
        
        // deserialize the actual data
        stopwatch.Restart();
        using MemoryStream contentStream = new MemoryStream(data, length + sizeof(int), data.Length - length - sizeof(int));
        T result = type != null ? (T) Serializer.Deserialize(type, contentStream) : Serializer.Deserialize<T>(contentStream);
        stopwatch.Stop();
        GD.Print($"Deserialized {typeof(T).Name} in {stopwatch.ElapsedMilliseconds}ms, size: {data.Length - length - sizeof(int)} bytes");
        return result;
    }
    
}

[ProtoContract]
public class NodeStructure {

    public static NodeStructure root;
    
    private static Dictionary<Type, ConstructorInfo> emptyConstructorCache = new Dictionary<Type, ConstructorInfo>();
    
    [ProtoMember(1)] public string nodeName;
    [ProtoMember(2)] public Type nodeType;
    [ProtoMember(3)] public string packedScenePath;
    [ProtoMember(4)] public Transform2D transform;
    [ProtoMember(5)] public List<NodeStructure> children = new List<NodeStructure>();

    public static void InitializeRoot() {
        root = new NodeStructure {
            nodeName = ServerController.Instance.GameSession.Name,
            nodeType = typeof(GameSession),
            packedScenePath = ServerController.Instance.GameSession.SceneFilePath
        };
    }

    public static NodeStructure RegisterNode(string[] path, int index = -1) {
        if (index == -1) index = path.Length - 1;
        string name = path[index];
        if (name.Contains("PlayerUI")) GD.PrintErr("Serializing " + path.Join("/"));

        NodeStructure parent = index == 0 ? root : RegisterNode(path, index - 1);
        
        // check if the node is already registered
        foreach (NodeStructure child in parent.children) {
            if (child.nodeName == name) {
                return child;
            }
        }
        
        // otherwise create and add a new NodeStructure entry
        Node node = ServerController.Instance.GameSession.GetNodeOrNull(string.Join("/", path, 0, index + 1));
        NodeStructure structure = new NodeStructure {
            nodeName = name,
            nodeType = node.GetType(),
            packedScenePath = node.SceneFilePath,
            transform = node is Node2D node2D ? node2D.Transform : Transform2D.Identity
        };
        parent.children.Add(structure);
        return structure;
    }

    public void EnsureCreated(Node parent) {
        Node node;
        if (parent != null) {
            node = parent.GetNodeOrNull(nodeName);
            if (node == null) {
                PackedScene createFrom = string.IsNullOrEmpty(packedScenePath) ? null : ResourceLoader.Load<PackedScene>(packedScenePath);
                node = createFrom != null ? createFrom.Instantiate() : (Node) GetEmptyConstructor(nodeType).Invoke(Array.Empty<object>());
                GD.Print($"Created {nodeType} with name '{nodeName}' under parent {parent.GetType()} at '{ServerController.Instance.GameSession.GetPathTo(parent)}' with packed scene '{packedScenePath}'.");
                parent.AddChild(node);
                node.Name = nodeName;
                if (node is Node2D node2D) node2D.Transform = transform;
            }
        } else {
            node = ServerController.Instance.GameSession;
        }
        
        foreach (NodeStructure child in children) {
            child.EnsureCreated(node);
        }
    }

    private ConstructorInfo GetEmptyConstructor(Type type) {
        if (emptyConstructorCache.TryGetValue(type, out ConstructorInfo constructor)) {
            return constructor;
        } else {
            ConstructorInfo emptyConstructor = type.GetConstructor(Type.EmptyTypes);
            if (emptyConstructor == null) {
                throw new InvalidOperationException("Identifiable type " + type + " does not have a parameterless constructor."); // nodes need to have an empty constructor for godot too, so this is not a restriction
            }
            emptyConstructorCache[type] = emptyConstructor;
            return emptyConstructor;
        }
    }

    public override string ToString() {
        return ($"{nodeType}[{nodeName}] ({packedScenePath})\n" + string.Join("\n", children)).Replace("\n", "\n    ");
    }
}