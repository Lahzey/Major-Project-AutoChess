using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.util;
using ProtoBuf;

namespace MPAutoChess.logic.core.unit.role;

[ProtoContract(Surrogate = typeof(UnitRoleSurrogate))]
public abstract class UnitRole {

    private static Dictionary<string, UnitRole> rolesByTypeName;

    public abstract void Apply(IEnumerable<UnitInstance> units);

    public virtual string GetName() {
        return StringUtil.PascalToReadable(GetType().Name);
    }
    
    public abstract string GetDescription();
    
    public abstract Texture2D GetIcon();

    public virtual int GetLevel(int count) {
        int level = 0;
        foreach (int threshold in GetCountThresholds()) {
            if (count >= threshold) {
                level++;
            } else {
                break;
            }
        }

        return level;
    }

    public virtual int GetCurrentThreshold(int count) {
        int level = GetLevel(count);
        return level == 0 ? 0 : GetCountThresholds()[level - 1];
    }

    public abstract int[] GetCountThresholds();

    public virtual bool IsActive(Board board) {
        return board.GetUnits().Count(unit => unit.Type.RoleSet.HasRole(this)) >= GetCountThresholds()[0];
    }
    
    public string GetTypeName() {
        return GetTypeName(GetType());
    }

    public static string GetTypeName(Type type) {
        return type.Name + " (in " + type.Namespace + ")";
    }

    public static UnitRole GetByTypeName(string typeName) {
        if (rolesByTypeName == null) LoadAllRoles();
        return rolesByTypeName.GetValueOrDefault(typeName);
    }
    
    public static IEnumerable<UnitRole> GetAllRoles() {
        if (rolesByTypeName == null) LoadAllRoles();
        return rolesByTypeName.Values;
    }
    
    private static void LoadAllRoles() {
        rolesByTypeName = new Dictionary<string, UnitRole>();
        foreach (Type type in AppDomain.CurrentDomain.GetAssemblies()
                     .SelectMany(assembly => assembly.GetTypes())
                     .Where(t => t.IsSubclassOf(typeof(UnitRole)) && !t.IsAbstract)) {
            UnitRole role = (UnitRole)Activator.CreateInstance(type);
            rolesByTypeName[GetTypeName(type)] = role;
        }
    }
}

[ProtoContract]
public class UnitRoleSurrogate {
    
    [ProtoMember(1)] public string TypeName { get; set; }

    public static implicit operator UnitRoleSurrogate(UnitRole role) {
        if (role == null) return null;
        return new UnitRoleSurrogate { TypeName = role.GetTypeName() };
    }

    public static implicit operator UnitRole(UnitRoleSurrogate surrogate) {
        if (surrogate == null) return null;
        return UnitRole.GetByTypeName(surrogate.TypeName);
    }
    
}