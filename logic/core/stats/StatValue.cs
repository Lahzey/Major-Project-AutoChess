using System.Linq;
using System.Reflection;
using Godot;
using Godot.Collections;

namespace MPAutoChess.logic.core.stats;

[GlobalClass, Tool]
public partial class StatValue : Resource {
    
    [Export] public string Type { get; set; } = StatType.STRENGTH.Name;
    [Export] public float Value { get; set; }

    public StatType StatType => StatType.Parse(Type);

    public override void _ValidateProperty(Dictionary property) {
        StringName propertyName = (StringName)property["name"];
        if (propertyName == PropertyName.Type) {
            string[] names = StatType.GetAllValues().Select(st => st.Name).ToArray();
            property["hint"] = (int)PropertyHint.Enum;
            property["hint_string"] = string.Join(",", names);
        }
    }
}