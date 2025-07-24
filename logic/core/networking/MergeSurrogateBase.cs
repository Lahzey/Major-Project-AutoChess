

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf;
using ProtoBuf.Meta;

namespace MPAutoChess.logic.core.networking;

public abstract class MergeSurrogateBase<T> where T : new() {
    public static bool printDebugMessages = false;

    private static ConstructorInfo emptyConstructor;
    private static Dictionary<int, FieldInfo> fields;
    private static Dictionary<int, PropertyInfo> properties;
    private static Dictionary<int, bool> overwriteListFlags = new Dictionary<int, bool>();

    private static Type wrapperType;
    
    protected static ConstructorInfo GetEmptyConstructor() {
        if (emptyConstructor != null) return emptyConstructor;
        
        emptyConstructor = typeof(T).GetConstructor(Type.EmptyTypes);
        if (emptyConstructor == null) {
            throw new InvalidOperationException("Identifiable type " + typeof(T) + " does not have a parameterless constructor."); // should never happen because of the where T : new() constraint
        }
        return emptyConstructor;
    }

    protected static Dictionary<int, FieldInfo> GetFields() {
        if (fields != null) return fields;
        
        fields = new Dictionary<int, FieldInfo>();
        foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
            if (field.IsDefined(typeof(ProtoMemberAttribute), false)) {
                ProtoMemberAttribute attribute = field.GetCustomAttribute<ProtoMemberAttribute>();
                if (attribute != null) {
                    fields[attribute.Tag] = field;
                    overwriteListFlags[attribute.Tag] = attribute.OverwriteList;
                    if (printDebugMessages) Console.WriteLine($"Found field [{attribute.Tag}]{field.Name} for {typeof(T)}");
                }
            }
        }

        return fields;
    }
    
    protected static Dictionary<int, PropertyInfo> GetProperties() {
        if (properties != null) return properties;
        
        properties = new Dictionary<int, PropertyInfo>();
        foreach (PropertyInfo property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
            if (property.IsDefined(typeof(ProtoMemberAttribute), false)) {
                ProtoMemberAttribute attribute = property.GetCustomAttribute<ProtoMemberAttribute>();
                if (attribute != null) {
                    properties[attribute.Tag] = property;
                    overwriteListFlags[attribute.Tag] = attribute.OverwriteList;
                    if (printDebugMessages) Console.WriteLine($"Found property [{attribute.Tag}]{property.Name} for {typeof(T)}");
                }
            }
        }

        return properties;
    }

    public static void InitMirrorType() {
        if (wrapperType != null) return;
        
        List<Type> genericTypes = new List<Type>();
        foreach (KeyValuePair<int, FieldInfo> fieldEntry in GetFields()) {
            genericTypes.Add(fieldEntry.Value.FieldType);
        }
        foreach (KeyValuePair<int, PropertyInfo> propertyEntry in GetProperties()) {
            genericTypes.Add(propertyEntry.Value.PropertyType);
        }
        
        wrapperType = genericTypes.Count switch {
            1 => typeof(TypeMirror<>).MakeGenericType(genericTypes.ToArray()),
            2 => typeof(TypeMirror<,>).MakeGenericType(genericTypes.ToArray()),
            3 => typeof(TypeMirror<,,>).MakeGenericType(genericTypes.ToArray()),
            4 => typeof(TypeMirror<,,,>).MakeGenericType(genericTypes.ToArray()),
            5 => typeof(TypeMirror<,,,,>).MakeGenericType(genericTypes.ToArray()),
            6 => typeof(TypeMirror<,,,,,>).MakeGenericType(genericTypes.ToArray()),
            7 => typeof(TypeMirror<,,,,,,>).MakeGenericType(genericTypes.ToArray()),
            _ => throw new NotSupportedException("GenericWrapper for more than 4 types is not supported.")
        };

        RuntimeTypeModel.Default[typeof(TypeMirror)].AddSubType(TypeMirror.SubTypeFieldNumber++, wrapperType);
    }

    protected static TypeMirror ToMirror(T obj) {
        if (obj == null) return null;
        List<Type> genericTypes = new List<Type>();
        List<int> tags = new List<int>();
        List<object> values = new List<object>();

        foreach (KeyValuePair<int, FieldInfo> fieldEntry in GetFields()) {
            genericTypes.Add(fieldEntry.Value.FieldType);
            tags.Add(fieldEntry.Key);
            values.Add(fieldEntry.Value.GetValue(obj));
        }
        foreach (KeyValuePair<int, PropertyInfo> propertyEntry in GetProperties()) {
            genericTypes.Add(propertyEntry.Value.PropertyType);
            tags.Add(propertyEntry.Key);
            values.Add(propertyEntry.Value.GetValue(obj));
        }
        if (genericTypes.Count == 0) return new TypeMirror() { tags = Array.Empty<int>() };
        
        TypeMirror cloneInstance = (TypeMirror) Activator.CreateInstance(wrapperType);
        cloneInstance.tags = tags.ToArray();
        cloneInstance.Set(values.ToArray());

        return cloneInstance;
    }

    protected T? FromMirror(TypeMirror clone) {
        if (clone == null) return default;
        T target = GetDeserializationTarget();
        for (int i = 0; i < clone.tags.Length; i++) {
            int tag = clone.tags[i];
            object value = clone.Get(i);
            if (GetFields().TryGetValue(tag, out FieldInfo field)) {
                if (printDebugMessages) Console.WriteLine($"Setting field [{tag}]{field.Name} for {typeof(T)} to {value}");
                value = HandleOverwriteListFlag(target, field.FieldType, field.GetValue(target), value, tag);
                field.SetValue(target, value);
            } else if (GetProperties().TryGetValue(tag, out PropertyInfo property)) {
                if (printDebugMessages) Console.WriteLine($"Setting property [{tag}]{property.Name} for {typeof(T)} to {value}");
                value = HandleOverwriteListFlag(target, property.PropertyType, property.GetValue(target), value, tag);
                property.SetValue(target, value);
            } else {
                throw new ArgumentException("No field or property found with tag " + tag + " in " + typeof(T));
            }
        }
        return target;
    }

    private object HandleOverwriteListFlag(T target, Type memberType, object memberValue, object value, int tag) {
        if (!overwriteListFlags[tag] && memberType.IsAssignableTo(typeof(ICollection))) {
            if (memberType.IsAssignableTo(typeof(IList))) {
                IList memberList = (IList) memberValue;
                if (memberList == null) return value;
                WriteToList(memberList, value);
                return memberList;
            } else if (memberType.IsAssignableTo(typeof(IDictionary))) {
                IDictionary memberDictionary = (IDictionary) memberValue;
                if (memberDictionary == null) return value;
                WriteToDictionary(memberDictionary, value);
                return memberDictionary;
            } else {
                throw new ArgumentException($"CollectionType {memberType} is not supported for merge surrogate serialization. Only IList and IDictionary are supported.");
            }
        } else return value;
    }

    private void WriteToList(IList list, object data) {
        if (data is null) {
            list.Clear();
            return;
        }
        
        if (data is not IList newList) throw new ArgumentException("Data must be of type IList to write to a list.");
        list.Clear();
        foreach (object item in newList) {
            list.Add(item);
        }
    }
    
    private void WriteToDictionary(IDictionary dictionary, object data) {
        if (data is null) {
            dictionary.Clear();
            return;
        }
        
        if (data is not IDictionary newDictionary) throw new ArgumentException("Data must be of type IDictionary to write to a dictionary.");
        dictionary.Clear();
        foreach (DictionaryEntry entry in newDictionary) {
            dictionary[entry.Key] = entry.Value;
        }
    }

    protected abstract T GetDeserializationTarget();

}

[ProtoContract]
public class TypeMirror {
    
    public static int SubTypeFieldNumber = 100;
    
    [ProtoMember(99)] public int[] tags;

    public virtual void Set(params object[] values) { }

    public virtual object Get(int index) {
        throw new NotImplementedException("GenericWrapper has not values to get");
    }

    public override string ToString() {
        return "Empty TypeMirror";
    }
}

[ProtoContract]
public class TypeMirror<T1> : TypeMirror {
    [ProtoMember(1)] public T1 value1;
    
    public override void Set(params object[] values) {
        if (values.Length != 1) throw new ArgumentException("GenericWrapper<A> expects exactly one value.");
        value1 = (T1) values[0];
    }
    
    public override object Get(int index) {
        return index switch {
            0 => value1,
            _ => throw new ArgumentOutOfRangeException(nameof(index), "GenericWrapper<A> only has one value at index 0.")
        };
    }
    
    public override string ToString() {
        return $"{typeof(T1).Name}: {value1}";
    }
}

[ProtoContract]
public class TypeMirror<T1, T2> : TypeMirror {
    [ProtoMember(1)] public T1 value1;
    [ProtoMember(2)] public T2 value2;
    
    public override void Set(params object[] values) {
        if (values.Length != 2) throw new ArgumentException("GenericWrapper<A, B> expects exactly two values.");
        value1 = (T1) values[0];
        value2 = (T2) values[1];
    }
    
    public override object Get(int index) {
        return index switch {
            0 => value1,
            1 => value2,
            _ => throw new ArgumentOutOfRangeException(nameof(index), "GenericWrapper<A, B> only has two values at indices 0 and 1.")
        };
    }
    
    public override string ToString() {
        return $"{typeof(T1).Name}: {value1}\n{typeof(T2).Name}: {value2}";
    }
}

[ProtoContract]
public class TypeMirror<T1, T2, T3> : TypeMirror {
    [ProtoMember(1)] public T1 value1;
    [ProtoMember(2)] public T2 value2;
    [ProtoMember(3)] public T3 value3;
    
    public override void Set(params object[] values) {
        if (values.Length != 3) throw new ArgumentException("GenericWrapper<A, B, C> expects exactly three values.");
        value1 = (T1) values[0];
        value2 = (T2) values[1];
        value3 = (T3) values[2];
    }
    
    public override object Get(int index) {
        return index switch {
            0 => value1,
            1 => value2,
            2 => value3,
            _ => throw new ArgumentOutOfRangeException(nameof(index), "GenericWrapper<A, B, C> only has three values at indices 0, 1, and 2.")
        };
    }
    
    public override string ToString() {
        return $"{typeof(T1).Name}: {value1}\n{typeof(T2).Name}: {value2}\n{typeof(T3).Name}: {value3}";
    }
}

[ProtoContract]
public class TypeMirror<T1, T2, T3, T4> : TypeMirror {
    [ProtoMember(1)] public T1 value1;
    [ProtoMember(2)] public T2 value2;
    [ProtoMember(3)] public T3 value3;
    [ProtoMember(4)] public T4 value4;
    
    public override void Set(params object[] values) {
        if (values.Length != 4) throw new ArgumentException("GenericWrapper<A, B, C, D> expects exactly four values.");
        value1 = (T1) values[0];
        value2 = (T2) values[1];
        value3 = (T3) values[2];
        value4 = (T4) values[3];
    }
    
    public override object Get(int index) {
        return index switch {
            0 => value1,
            1 => value2,
            2 => value3,
            3 => value4,
            _ => throw new ArgumentOutOfRangeException(nameof(index), "GenericWrapper<A, B, C, D> only has four values at indices 0, 1, 2, and 3.")
        };
    }
    
    public override string ToString() {
        return $"{typeof(T1).Name}: {value1}\n{typeof(T2).Name}: {value2}\n{typeof(T3).Name}: {value3}\n{typeof(T4).Name}: {value4}";
    }
}

[ProtoContract]
public class TypeMirror<T1, T2, T3, T4, T5> : TypeMirror {
    [ProtoMember(1)] public T1 value1;
    [ProtoMember(2)] public T2 value2;
    [ProtoMember(3)] public T3 value3;
    [ProtoMember(4)] public T4 value4;
    [ProtoMember(5)] public T5 value5;

    public override void Set(params object[] values) {
        if (values.Length != 5) throw new ArgumentException("GenericWrapper<T1, T2, T3, T4, T5> expects exactly five values.");
        value1 = (T1) values[0];
        value2 = (T2) values[1];
        value3 = (T3) values[2];
        value4 = (T4) values[3];
        value5 = (T5) values[4];
    }

    public override object Get(int index) {
        return index switch {
            0 => value1,
            1 => value2,
            2 => value3,
            3 => value4,
            4 => value5,
            _ => throw new ArgumentOutOfRangeException(nameof(index), "GenericWrapper<T1, T2, T3, T4, T5> only has five values at indices 0 to 4.")
        };
    }
    
    public override string ToString() {
        return $"{typeof(T1).Name}: {value1}\n{typeof(T2).Name}: {value2}\n{typeof(T3).Name}: {value3}\n{typeof(T4).Name}: {value4}\n{typeof(T5).Name}: {value5}";
    }
}

[ProtoContract]
public class TypeMirror<T1, T2, T3, T4, T5, T6> : TypeMirror {
    [ProtoMember(1)] public T1 value1;
    [ProtoMember(2)] public T2 value2;
    [ProtoMember(3)] public T3 value3;
    [ProtoMember(4)] public T4 value4;
    [ProtoMember(5)] public T5 value5;
    [ProtoMember(6)] public T6 value6;

    public override void Set(params object[] values) {
        if (values.Length != 6) throw new ArgumentException("GenericWrapper<T1, T2, T3, T4, T5, T6> expects exactly six values.");
        value1 = (T1) values[0];
        value2 = (T2) values[1];
        value3 = (T3) values[2];
        value4 = (T4) values[3];
        value5 = (T5) values[4];
        value6 = (T6) values[5];
    }

    public override object Get(int index) {
        return index switch {
            0 => value1,
            1 => value2,
            2 => value3,
            3 => value4,
            4 => value5,
            5 => value6,
            _ => throw new ArgumentOutOfRangeException(nameof(index), "GenericWrapper<T1, T2, T3, T4, T5, T6> only has six values at indices 0 to 5.")
        };
    }
    
    public override string ToString() {
        return $"{typeof(T1).Name}: {value1}\n{typeof(T2).Name}: {value2}\n{typeof(T3).Name}: {value3}\n{typeof(T4).Name}: {value4}\n{typeof(T5).Name}: {value5}\n{typeof(T6).Name}: {value6}";
    }
}

[ProtoContract]
public class TypeMirror<T1, T2, T3, T4, T5, T6, T7> : TypeMirror {
    [ProtoMember(1)] public T1 value1;
    [ProtoMember(2)] public T2 value2;
    [ProtoMember(3)] public T3 value3;
    [ProtoMember(4)] public T4 value4;
    [ProtoMember(5)] public T5 value5;
    [ProtoMember(6)] public T6 value6;
    [ProtoMember(7)] public T7 value7;

    public override void Set(params object[] values) {
        if (values.Length != 7) throw new ArgumentException("GenericWrapper<T1, T2, T3, T4, T5, T6, T7> expects exactly seven values.");
        value1 = (T1) values[0];
        value2 = (T2) values[1];
        value3 = (T3) values[2];
        value4 = (T4) values[3];
        value5 = (T5) values[4];
        value6 = (T6) values[5];
        value7 = (T7) values[6];
    }

    public override object Get(int index) {
        return index switch {
            0 => value1,
            1 => value2,
            2 => value3,
            3 => value4,
            4 => value5,
            5 => value6,
            6 => value7,
            _ => throw new ArgumentOutOfRangeException(nameof(index), "GenericWrapper<T1, T2, T3, T4, T5, T6, T7> only has seven values at indices 0 to 6.")
        };
    }
    
    public override string ToString() {
        return $"{typeof(T1).Name}: {value1}\n{typeof(T2).Name}: {value2}\n{typeof(T3).Name}: {value3}\n{typeof(T4).Name}: {value4}\n{typeof(T5).Name}: {value5}\n{typeof(T6).Name}: {value6}\n{typeof(T7).Name}: {value7}";
    }
}