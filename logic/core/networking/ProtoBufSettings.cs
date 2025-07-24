using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using ProtoBuf.Meta;

namespace MPAutoChess.logic.core.networking;

public static class ProtoBufSettings {
    
    private static RuntimeTypeModel model = RuntimeTypeModel.Default;

    public static void Set() {
        RegisterIdentifiableSurrogates();
        RegisterNodeSurrogates();
        RegisterResourceSurrogates();
        RegisterGodotStructs();
    }
    
    private static void RegisterIdentifiableSurrogates() {
        // use reflection to find all classes that implement IIdentifiable, are not an interface or abstract class and have the [ProtoContract] attribute
        IEnumerable<Type> identifiableTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsAssignableTo(typeof(IIdentifiable)) && !type.IsInterface && !type.IsAbstract && type.IsDefined(typeof(ProtoBuf.ProtoContractAttribute), true));
        foreach (Type type in identifiableTypes) {
            Type surrogateType = typeof(IdentifiableSurrogate<>).MakeGenericType(type);
            model.Add(type, true);
            model[type].SetSurrogate(surrogateType);
            // model[type].AsReferenceDefault = true;
            RuntimeHelpers.RunClassConstructor(surrogateType.TypeHandle);
        }
    }
    
    private static void RegisterNodeSurrogates() {
        // use reflection to find all classes that extend Node and are not abstract
        IEnumerable<Type> nodeTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsAssignableTo(typeof(Node)) && !type.IsAbstract);
        foreach (Type type in nodeTypes) {
            model.Add(type, true);
            Type surrogateType;
            if (type.IsDefined(typeof(ProtoBuf.ProtoContractAttribute))) {
                surrogateType = typeof(NodeDataSurrogate<>).MakeGenericType(type); // if marked with ProtoContract, use NodeDataSurrogate to serialize all its data
                RuntimeHelpers.RunClassConstructor(surrogateType.TypeHandle);
            } else {
                surrogateType = typeof(NodeRefSurrogate<>).MakeGenericType(type); // if not marked with ProtoContract, use NodeRefSurrogate to serialize only the node path
            }
            model[type].SetSurrogate(surrogateType);
            // model[type].AsReferenceDefault = true;
        }
    }
    
    private static void RegisterResourceSurrogates() {
        // use reflection to find all classes that extend Resource and are not abstract
        IEnumerable<Type> resourceTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsAssignableTo(typeof(Resource)) && !type.IsAbstract);
        foreach (Type type in resourceTypes) {
            Type surrogateType = typeof(ResourceSurrogate<>).MakeGenericType(type);
            model.Add(type, true);
            model[type].SetSurrogate(surrogateType);
        }
    }

    private static void RegisterGodotStructs() {
        model.Add(typeof(Vector2), true).Add(1, "X").Add(2, "Y");
        model.Add(typeof(Vector3), true).Add(1, "X").Add(2, "Y").Add(3, "Z");
        model.Add(typeof(Vector4), true).Add(1, "X").Add(2, "Y").Add(3, "Z").Add(4, "W");
        model.Add(typeof(Color), true).Add(1, "R").Add(2, "G").Add(3, "B").Add(4, "A");
        // incomplete list, add more as needed
    }
    
}