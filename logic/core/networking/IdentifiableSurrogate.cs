using System;
using Godot;
using MPAutoChess.logic.core.stats;
using ProtoBuf;

namespace MPAutoChess.logic.core.networking;

[ProtoContract]
public class IdentifiableSurrogate<T> : MergeSurrogateBase<T> where T : class, IIdentifiable, new() {
    
    [ProtoMember(1)] public string id;
    [ProtoMember(2)] public TypeMirror data;

    static IdentifiableSurrogate() {
        InitMirrorType();
    }

    protected override T GetDeserializationTarget() {
        IIdentifiable existingIdentifiable = IIdentifiable.TryGetInstance(id);
        if (existingIdentifiable == null) {
            T target = (T) GetEmptyConstructor().Invoke(Array.Empty<object>());
            target.SetId(id);
            return target;
        } else {
            if (!existingIdentifiable.GetType().IsAssignableTo(typeof(T))) throw new ArgumentException("Identifiable with ID " + id + " already exists as a different type: " + existingIdentifiable.GetType().FullName + " instead of " + typeof(T).FullName);
            return (T) existingIdentifiable;
        }
    }

    public static implicit operator T(IdentifiableSurrogate<T> surrogate) {
        if (surrogate == null) return null;
        if (printDebugMessages) Console.WriteLine($"Converting IdentifiableSurrogate({surrogate.id})[{surrogate.data}] to Identifiable of type " + typeof(T));
        if (surrogate.id == null) throw new ArgumentException("IdentifiableSurrogate must have an ID set.");
        if (surrogate.data == null) throw new ArgumentException("IdentifiableSurrogate must have data set to non-null value (empty is ok).");

        return surrogate.FromMirror(surrogate.data);
    }

    public static implicit operator IdentifiableSurrogate<T>(T obj) {
        if (obj == null) return null;
        if (obj.GetId() == null) throw new ArgumentException("Cannot serialize Identifiable without an ID.");
        
        var result = new IdentifiableSurrogate<T> {
            id = obj.GetId(),
            data = ToMirror(obj)
        };
        
        if (printDebugMessages) Console.WriteLine("Create IdentifiableSurrogate for " + typeof(T) + " with ID " + result.id);
        return result;
    }
}