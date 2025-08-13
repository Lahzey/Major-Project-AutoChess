using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using MPAutoChess.logic.core.item;
using ProtoBuf.Meta;

namespace MPAutoChess.logic.core.networking;

public static class ProtoBufSettings {
    
    private static RuntimeTypeModel model = RuntimeTypeModel.Default;
    
    private static int itemEffectSubTypeId = 1000;

    public static void Set() {
        RegisterSurrogates();
        RegisterGodotStructs();
    }
    
    private static void RegisterSurrogates() {
        // use reflection to find all classes that implement IIdentifiable, are not an interface or abstract class and have the [ProtoContract] attribute
        IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsInterface && !type.IsAbstract);
        foreach (Type type in types) {
            if (type.IsAssignableTo(typeof(Node))) {
                model.Add(type, true);
                Type surrogateType;
                if (type.IsDefined(typeof(ProtoBuf.ProtoContractAttribute), true)) {
                    surrogateType = typeof(NodeDataSurrogate<>).MakeGenericType(type); // if marked with ProtoContract, use NodeDataSurrogate to serialize all its data
                    RuntimeHelpers.RunClassConstructor(surrogateType.TypeHandle); // adds necessary wrappers to the model
                } else {
                    surrogateType = typeof(NodeRefSurrogate<>).MakeGenericType(type); // if not marked with ProtoContract, use NodeRefSurrogate to serialize only the node path
                }
                model[type].SetSurrogate(surrogateType);
            } else if (type.IsAssignableTo(typeof(IIdentifiable))) {
                if (type.IsDefined(typeof(ProtoBuf.ProtoContractAttribute), true)) {
                    Type surrogateType = typeof(IdentifiableSurrogate<>).MakeGenericType(type);
                    model.Add(type, true);
                    model[type].SetSurrogate(surrogateType);
                    // model[type].AsReferenceDefault = true;
                    RuntimeHelpers.RunClassConstructor(surrogateType.TypeHandle); // adds necessary wrappers to the model
                }
            } else if (type.IsAssignableTo(typeof(Resource))) {
                Type surrogateType = typeof(ResourceSurrogate<>).MakeGenericType(type);
                model.Add(type, true);
                model[type].SetSurrogate(surrogateType);
            }

            // I cannot be bothered to add all these subtypes manually
            if (type.IsAssignableTo(typeof(ItemEffect))) {
                model[typeof(ItemEffect)].AddSubType(itemEffectSubTypeId++, type);
            }
        }
    }

    private static void RegisterGodotStructs() {
        model.Add(typeof(Vector2), true).Add(1, nameof(Vector2.X)).Add(2, nameof(Vector2.Y));
        model.Add(typeof(Vector3), true).Add(1, nameof(Vector3.X)).Add(2, nameof(Vector3.Y)).Add(3, nameof(Vector3.Z));
        model.Add(typeof(Vector4), true).Add(1, nameof(Vector4.X)).Add(2, nameof(Vector4.Y)).Add(3, nameof(Vector4.Z)).Add(4, nameof(Vector4.W));
        model.Add(typeof(Color), true).Add(1, nameof(Color.R)).Add(2, nameof(Color.G)).Add(3, nameof(Color.B)).Add(4, nameof(Color.A));
        model.Add(typeof(Transform2D), true).Add(1, nameof(Transform2D.X)).Add(2, nameof(Transform2D.Y)).Add(3, nameof(Transform2D.Origin));
        model.Add(typeof(Rect2), true).Add(1, nameof(Rect2.Position)).Add(2, nameof(Rect2.Size));
        // incomplete list, add more as needed
    }
    
}