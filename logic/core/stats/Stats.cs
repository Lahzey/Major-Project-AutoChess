using System.Collections.Generic;
using ProtoBuf;

namespace MPAutoChess.logic.core.stats;

[ProtoContract]
public class Stats {
    
    [ProtoMember(1)] private Dictionary<StatType, Calculation> values = new Dictionary<StatType, Calculation>();
    
    public IEnumerable<StatType> Types => values.Keys;
    
    public Calculation GetCalculation(StatType statType) {
        if (!values.ContainsKey(statType)) {
            values[statType] = new Calculation(0);
        }
        return values.GetValueOrDefault(statType);
    }
    
    public float GetValue(StatType statType) {
        return GetCalculation(statType).Evaluate();
    }

    public Stats Clone() {
        Stats clone = new Stats();
        foreach (KeyValuePair<StatType, Calculation> kvp in values) {
            clone.values[kvp.Key] = kvp.Value.Clone();
        }
        return clone;
    }
}