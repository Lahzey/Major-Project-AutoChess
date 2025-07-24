using System.Diagnostics;
using System.IO;
using Godot;

namespace MPAutoChess.logic.core.util;

public static class SerializerExtensions {
    
    public static byte[] Serialize<T>(T obj) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        using MemoryStream stream = new MemoryStream();
        ProtoBuf.Serializer.Serialize(stream, obj);
        byte[] result = stream.ToArray();
        stopwatch.Stop();
        GD.Print($"Serialized {typeof(T).Name} in {stopwatch.ElapsedMilliseconds}ms, size: {result.Length} bytes");
        return result;
    }
    
    public static T Deserialize<T>(byte[] data) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        using MemoryStream stream = new MemoryStream(data);
        T result = ProtoBuf.Serializer.Deserialize<T>(stream);
        stopwatch.Stop();
        GD.Print($"Deserialized {typeof(T).Name} in {stopwatch.ElapsedMilliseconds}ms, size: {data.Length} bytes");
        return result;
    }
    
}