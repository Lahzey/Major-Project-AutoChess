using System.Linq;
using System.Reflection;
using Godot;
using Godot.Collections;

namespace MPAutoChess.logic.core.stats;

[GlobalClass, Tool]
public partial class StatValue : Resource {
    
    private const string TYPE_FIELD_NAME = "StatType";
    private const string VALUE_FIELD_NAME = "StatValue";
    
    public StatType Type { get; set; }
    public float Value { get; set; }

    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> list = new Array<Dictionary>();

        // Dropdown for StatType
        var typeNames = string.Join(",", StatType.GetAllValues().Select(t => t.Name));
        list.Add(new Dictionary() {
            { "name", TYPE_FIELD_NAME },
            { "type", (int) Variant.Type.String },
            { "usage", (int) (PropertyUsageFlags.Default | PropertyUsageFlags.Editor) },
            { "hint", (int) PropertyHint.Enum },
            { "hint_string", typeNames }
        });

        // Float input for value
        list.Add(new Dictionary() {
            { "name", VALUE_FIELD_NAME },
            { "type", (int) Variant.Type.Float },
            { "usage", (int) (PropertyUsageFlags.Default | PropertyUsageFlags.Editor) }
        });

        return list;
    }

    public override Variant _Get(StringName property) {
        return property.ToString() switch {
            TYPE_FIELD_NAME => Type?.Name ?? null,
            VALUE_FIELD_NAME => Value,
            _ => base._Get(property)
        };
    }

    public override bool _Set(StringName property, Variant value) {
        switch (property.ToString()) {
            case TYPE_FIELD_NAME:
                string name = value.AsString();
                StatType found = StatType.GetAllValues().FirstOrDefault(t => t.Name == name);
                Type = found;
                return true;
            case VALUE_FIELD_NAME:
                Value = value.AsSingle();
                return true;
            default:
                return base._Set(property, value);
        }
    }
}