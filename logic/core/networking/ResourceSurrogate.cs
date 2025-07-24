using System;
using Godot;
using ProtoBuf;

namespace MPAutoChess.logic.core.networking;

[ProtoContract]
public class ResourceSurrogate<T> where T : Resource {
    
    [ProtoMember(1)] public string resourcePath;

    public static implicit operator T?(ResourceSurrogate<T> surrogate) {
        if (surrogate == null) return null;
        if (surrogate.resourcePath == null) throw new ArgumentException("NodeRefSurrogate must have an nodePath set.");

        return ResourceLoader.Load<T>(surrogate.resourcePath);
    }

    public static implicit operator ResourceSurrogate<T>(T obj) {
        if (obj == null) return null;
        if(obj.ResourceLocalToScene) throw new ArgumentException("Cannot serialize Resources that are local to the scene.");
        return new  ResourceSurrogate<T> { resourcePath = obj.ResourcePath };
    }

}