using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace MPAutoChess.logic.core.unit;

[GlobalClass, Tool]
public partial class UnitRoleSet : Resource {
    private const string ROLE_COUNT_PROPERTY_NAME = "Role Count";
    private const string ROLE_PROPERTY_PREFIX = "Role ";

    public Array<string> roleNames = new Array<string>();
    
    public HashSet<UnitRole> Roles => roleNames.Select(UnitRole.GetByTypeName).Where(role => role != null).ToHashSet();

    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> list = new Array<Dictionary>();
    
        // Add the RoleCount field
        list.Add(new Dictionary() {
            { "name", ROLE_COUNT_PROPERTY_NAME },
            { "type", (int) Variant.Type.Int },
            { "usage", (int) (PropertyUsageFlags.Default | PropertyUsageFlags.Editor) }
        });
    
        // Add individual role selector fields
        string allRolesHint = string.Join(",", UnitRole.GetAllRoles().OrderBy(role => role.GetType().FullName).Select(role => role.GetTypeName())); // ordering by full name should order by namespace > class name
        for (int i = 0; i < roleNames.Count; i++) {
            list.Add(new Dictionary() {
                { "name", $"{ROLE_PROPERTY_PREFIX}{i + 1}" },
                { "type", (int) Variant.Type.String },
                { "usage", (int) (PropertyUsageFlags.Default | PropertyUsageFlags.Editor) },
                { "hint", (int) PropertyHint.Enum },
                { "hint_string", allRolesHint }
            });
        }
    
        return list;
    }
    
    public override Variant _Get(StringName property) {
        if (property == ROLE_COUNT_PROPERTY_NAME)
            return roleNames.Count;
    
        if (property.ToString().StartsWith(ROLE_PROPERTY_PREFIX)) {
            if (TryGetRoleIndex(property.ToString(), out int index) && index < roleNames.Count)
                return roleNames[index];
        }
    
        return base._Get(property);
    }
    
    public override bool _Set(StringName property, Variant value) {
        if (property == ROLE_COUNT_PROPERTY_NAME) {
            uint roleLength = (uint)value.AsInt32();
            while (roleNames.Count < roleLength)
                roleNames.Add("");
            while (roleNames.Count > roleLength)
                roleNames.RemoveAt(roleNames.Count - 1);
            NotifyPropertyListChanged();
            GD.Print("Set role count to " + roleLength);
            return true;
        }
    
        if (property.ToString().StartsWith(ROLE_PROPERTY_PREFIX)) {
            if (TryGetRoleIndex(property.ToString(), out int index)) {
                string val = value.AsString();
                
                while (roleNames.Count <= index)
                    roleNames.Add("");
                roleNames[index] = val;
                return true;
            }
        }
    
        return base._Set(property, value);
    }
    
    private bool TryGetRoleIndex(string name, out int index) {
        index = -1;
        var suffix = name.Substring(ROLE_PROPERTY_PREFIX.Length);
        return int.TryParse(suffix, out index) && index > 0 ? --index >= 0 : false;
    }

    public bool HasRole(UnitRole unitRole) {
        return Roles.Contains(unitRole);
    }
}