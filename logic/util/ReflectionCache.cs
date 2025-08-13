using System;
using System.Collections.Generic;
using System.Reflection;

namespace MPAutoChess.logic.util;

public static class ReflectionCache {
    
    private static Dictionary<Type, ConstructorInfo> emptyConstructors = new Dictionary<Type, ConstructorInfo>();

    public static ConstructorInfo GetEmptyConstructor(Type type) {
        if (emptyConstructors.TryGetValue(type, out ConstructorInfo constructor)) return constructor;
        
        constructor = type.GetConstructor(Type.EmptyTypes);
        emptyConstructors.Add(type, constructor);
        return constructor;
    }
    
}