using System.Collections.Generic;
using MPAutoChess.logic.core.networking;
using ProtoBuf;

namespace MPAutoChess.logic.core.stats;

[ProtoContract]
public class Calculation : IIdentifiable {
    public string Id { get; set; }

    [ProtoMember(1)] private Value baseValue;
    public Value BaseValue { get => baseValue; set { baseValue = value; Invalidate(); } }
    
    [ProtoMember(2)] private List<Value> preMultValues = new List<Value>();
    [ProtoMember(3)] private List<string> preMultIds = new List<string>();
    
    [ProtoMember(4)] private List<Value> flatValues = new List<Value>();
    [ProtoMember(5)] private List<string> flatIds = new List<string>();
    
    [ProtoMember(6)] private List<Value> postMultValues = new List<Value>();
    [ProtoMember(7)] private List<string> postMultIds = new List<string>();
    
    public Calculation() {} // empty constructor for MessagePack serialization
    
    public Calculation(float baseValue) {
        BaseValue = baseValue;
    }
    
    public Calculation(Value baseValue) {
        BaseValue = baseValue;
    }
    
    public float Evaluate() {
        float result = baseValue.Get();
        float preMult = 1;
        foreach (Value value in preMultValues) {
            preMult += value.Get();
        }
        result *= preMult;
        foreach (Value value in flatValues) {
            result += value.Get();
        }
        float postMult = 1;
        foreach (Value value in postMultValues) {
            postMult += value.Get();
        }
        result *= postMult;

        return result;
    }
    
    public void AddPreMult(Value value, string id) {
        int index = preMultIds.IndexOf(id);
        if (index >= 0) {
            preMultValues[index] = value;
        } else {
            preMultValues.Add(value);
            preMultIds.Add(id);
        }
        Invalidate();
    }

    public Value? GetPreMult(string id) {
        int index = preMultIds.IndexOf(id);
        if (index >= 0) {
            return preMultValues[index];
        }
        return null;
    }
    
    public bool RemovePreMult(string id) {
        int index = preMultIds.IndexOf(id);
        if (index == -1) return false;
        preMultValues.RemoveAt(index);
        preMultIds.RemoveAt(index);
        Invalidate();
        return true;
    }
    
    public void AddFlat(Value value, string id) {
        int index = flatIds.IndexOf(id);
        if (index >= 0) {
            flatValues[index] = value;
        } else {
            flatValues.Add(value);
            flatIds.Add(id);
        }
        Invalidate();
    }
    
    public Value? GetFlat(string id) {
        int index = flatIds.IndexOf(id);
        if (index >= 0) {
            return flatValues[index];
        }
        return null;
    }
    
    public bool RemoveFlat(string id) {
        int index = flatIds.IndexOf(id);
        if (index == -1) return false;
        flatValues.RemoveAt(index);
        flatIds.RemoveAt(index);
        Invalidate();
        return true;
    }


    private void Invalidate() {
        
    }

    public Calculation Clone() {
        Calculation clone = new Calculation(baseValue.Clone());
        
        foreach (Value preMult in preMultValues) {
            clone.preMultValues.Add(preMult.Clone());
        }
        clone.preMultIds = new List<string>(preMultIds);
        
        foreach (Value add in flatValues) {
            clone.flatValues.Add(add.Clone());
        }
        clone.flatIds = new List<string>(flatIds);
        
        foreach (Value postMult in postMultValues) {
            clone.postMultValues.Add(postMult.Clone());
        }
        clone.postMultIds = new List<string>(postMultIds);
        
        return clone;
    }

    public override string ToString() {
        return $"Base: {BaseValue}\nPreMults: [{string.Join(", ", preMultValues)}]\nAdds: [{string.Join(", ", flatValues)}]\nPostMults: [{string.Join(", ", postMultValues)}]";
    }
}