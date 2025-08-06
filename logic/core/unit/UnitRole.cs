using System;
using System.Collections.Generic;
using System.Linq;
using MPAutoChess.logic.core.placement;
using ProtoBuf;

namespace MPAutoChess.logic.core.unit;

[ProtoContract(Surrogate = typeof(UnitRoleSurrogate))]
public abstract class UnitRole {

    private static Dictionary<string, UnitRole> rolesByTypeName;

    public abstract void OnBoardUpdate(Board board);
    
    public abstract void OnCombatStart(Board board);
    
    public abstract void OnCombatEnd(Board board);

    public virtual string GetName() {
        return GetType().Name;
    }
    
    public abstract string GetDescription();

    public abstract int[] GetCountThresholds();

    public virtual bool IsActive(Board board) {
        return board.GetUnits().Count(unit => unit.Type.Roles.HasRole(this)) >= GetCountThresholds()[0];
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
        return new UnitRoleSurrogate { TypeName = role.GetTypeName() };
    }

    public static implicit operator UnitRole(UnitRoleSurrogate surrogate) {
        return UnitRole.GetByTypeName(surrogate.TypeName);
    }
    
}