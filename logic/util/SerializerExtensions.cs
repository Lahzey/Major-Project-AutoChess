using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Godot;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.session;
using ProtoBuf;
using Environment = System.Environment;

namespace MPAutoChess.logic.util;

public static class SerializerExtensions {

    public const bool DEBUG = false;
    
    public static byte[] Serialize<T>(T obj) {
        NodeStructure.InitializeRoot();

        // serialize actual data
        Stopwatch stopwatch = Stopwatch.StartNew();
        using MemoryStream contentStream = new MemoryStream();
        Serializer.Serialize(contentStream, obj);
        byte[] content = contentStream.ToArray();
        stopwatch.Stop();
        if (DEBUG) GD.Print($"[{Environment.ProcessId}] Serialized {typeof(T).Name} in {stopwatch.ElapsedMilliseconds}ms, size: {content.Length} bytes");

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
        if (DEBUG) GD.Print($"[{Environment.ProcessId}] Serialization of header and copy to result took {stopwatch.ElapsedMilliseconds}ms, final size: {result.Length} bytes");
        
        NodeStructure.root = null; // allow garbage collection of the root node structure

        return result;
    }
    
    public static T Deserialize<T>(byte[] data, Type type = null) {
        return DeserializeWithInit<T>(data, null, type);
    }

    public static T DeserializeWithInit<T>(byte[] data, Action<T> initAction, Type type = null) {
        // deserialize the header
        Stopwatch stopwatch = Stopwatch.StartNew();
        int length = BitConverter.ToInt32(data, 0);
        using MemoryStream headerStream = new MemoryStream(data, sizeof(int), length);
        NodeStructure root = Serializer.Deserialize<NodeStructure>(headerStream);
        NodeStructure.root = root;
        root.EnsureCreated(null);
        stopwatch.Stop();
        if (DEBUG) GD.Print($"[{Environment.ProcessId}] Deserialized NodeStructure in {stopwatch.ElapsedMilliseconds}ms, size: {length + sizeof(int)} bytes");
        
        // deserialize the actual data
        stopwatch.Restart();
        using MemoryStream contentStream = new MemoryStream(data, length + sizeof(int), data.Length - length - sizeof(int));
        T result;
        try {
            result = type != null ? (T) Serializer.Deserialize(type, contentStream) : Serializer.Deserialize<T>(contentStream);
        } catch (Exception e) {
            GD.Print($"Failed Deserialize call with generics {typeof(T)} and type {type}.");
            throw new Exception($"Failed Deserialize call with generics {typeof(T)} and type {type}.", e);
        }
        stopwatch.Stop();
        if (DEBUG) GD.Print($"[{Environment.ProcessId}] Deserialized {type?.Name ?? typeof(T).Name} in {stopwatch.ElapsedMilliseconds}ms, size: {data.Length - length - sizeof(int)} bytes");

        if ((type?.Name ?? typeof(T).Name) == "Stats") {
            GD.Print($"Received Stats object from Deserialize call with generics {typeof(T)} and type {type}, resulting type: {result?.GetType()??null}");
        }
        
        initAction?.Invoke(result);
        
        NodeQueue.AddToSceneTree();
        return result;
    }

    public static string GetNodePath(Node node) {
        if (node == ServerController.Instance.GameSession) {
            return null;
        } else if (ServerController.Instance.GameSession.IsAncestorOf(node)) {
            return ServerController.Instance.GameSession.GetPathTo(node);
        } else {
            throw new ArgumentException($"Cannot get node path of {node.GetType()} at {node.GetPath()}. Node must be a child of the GameSession or the GameSession itself.", nameof(node));
        }
    }

    public static Node FindNode(string path) {
        if (path == null) return ServerController.Instance.GameSession;
        return ServerController.Instance.GameSession.GetNodeOrNull(path) ?? NodeQueue.Find(path);
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

    private string fullPath;

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
                node.Name = nodeName;
                if (node is Node2D node2D) node2D.Transform = transform;
                NodeQueue.Add(node, parent, fullPath);
            }
        } else {
            node = ServerController.Instance.GameSession;
        }
        
        foreach (NodeStructure child in children) {
            child.fullPath = fullPath != null ? $"{fullPath}/{child.nodeName}" : child.nodeName;
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

public static class NodeQueue {
    private static List<Node> queue = new List<Node>();
    private static List<Node> queueParents = new List<Node>();
    private static Dictionary<string, Node> nodePaths = new Dictionary<string, Node>();

    public static void Add(Node node, Node parent, string path) {
        queue.Add(node);
        queueParents.Add(parent);
        nodePaths[path] = node;
        if (SerializerExtensions.DEBUG) GD.Print($"[{Environment.ProcessId}] Added node {node.GetType()} at path {path} to queue.");
    }

    public static void AddToSceneTree() {
        for (int i = 0; i < queue.Count; i++) {
            Node node = queue[i];
            Node parent = queueParents[i];
            parent.AddChild(node);
        }
        queue.Clear();
        queueParents.Clear();
        nodePaths.Clear();
    }

    public static Node Find(string path) {
        Node node = nodePaths.GetValueOrDefault(path);
        if (node != null) return node;
        
        foreach (string nodePath in nodePaths.Keys) {
            if (path.Contains(nodePath)) {
                node = nodePaths[nodePath].GetNodeOrNull(path.Replace(nodePath + "/", ""));
                if (node != null) return node;
            }
        }

        return null;
    }
}