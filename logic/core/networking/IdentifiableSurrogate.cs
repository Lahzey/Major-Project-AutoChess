using System;
using Godot;
using MPAutoChess.logic.core.stats;
using ProtoBuf;
using Environment = System.Environment;

namespace MPAutoChess.logic.core.networking;

[ProtoContract]
public class IdentifiableSurrogate<T> : MergeSurrogateBase<T> where T : class, IIdentifiable, new() {
    
    [ProtoMember(1)] public string id;
    [ProtoMember(2)] public Type type; // required to support inheritance (with ProtoInclude), so it recreates the correct type (generic type T of this surrogate is always the base type)
    [ProtoMember(3)] public TypeMirror data;

    static IdentifiableSurrogate() {
        InitMirrorType();
    }

    protected override T GetDeserializationTarget() {
        IIdentifiable existingIdentifiable = IIdentifiable.TryGetInstance(id);
        if (existingIdentifiable == null) {
            T target = (T) GetEmptyConstructor(type).Invoke(Array.Empty<object>());
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

        if (typeof(T).IsAssignableTo(typeof(Stats))) {
            GD.Print($"[{Environment.ProcessId}]: Received IdentifiableSurrogate for {typeof(T)} with ID {surrogate.id}");
        }

        return surrogate.FromMirror(surrogate.data);
    }

    public static implicit operator IdentifiableSurrogate<T>(T obj) {
        if (obj == null) return null;
        if (obj.GetId() == null) throw new ArgumentException("Cannot serialize Identifiable without an ID.");

        if (typeof(T) != obj.GetType()) throw new InvalidOperationException($"[{Environment.ProcessId}]: Tried to create IdentifiableSurrogate for {typeof(T)} (actual type: {obj.GetType()}) with ID {obj.GetId()}");

        if (typeof(T).IsAssignableTo(typeof(Stats))) {
            GD.Print($"[{Environment.ProcessId}]: Creating IdentifiableSurrogate for {typeof(T)} (actual type: {obj.GetType()}) with ID {obj.GetId()}");
        }
        
        var result = new IdentifiableSurrogate<T> {
            id = obj.GetId(),
            data = ToMirror(obj)
        };
        
        if (printDebugMessages) Console.WriteLine("Create IdentifiableSurrogate for " + typeof(T) + " with ID " + result.id);
        return result;
    }
}