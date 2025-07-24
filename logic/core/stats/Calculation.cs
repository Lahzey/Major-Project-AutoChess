using System.Collections.Generic;
using MPAutoChess.logic.core.networking;
using ProtoBuf;

namespace MPAutoChess.logic.core.stats;

[ProtoContract]
public class Calculation : IIdentifiable {
    public string Id { get; set; }

    [ProtoMember(1)] private Value baseValue;
    public Value BaseValue { get => baseValue; set { baseValue = value; Invalidate(); } }
    
    [ProtoMember(2)] private List<Value> preMults = new List<Value>();
    [ProtoMember(3)] private List<string> preMultIds = new List<string>();
    
    [ProtoMember(4)] private List<Value> adds = new List<Value>();
    [ProtoMember(5)] private List<string> addIds = new List<string>();
    
    [ProtoMember(6)] private List<Value> postMults = new List<Value>();
    [ProtoMember(7)] private List<string> postMultIds = new List<string>();
    
    public Calculation() {} // empty constructor for MessagePack serialization
    
    public Calculation(float baseValue) {
        BaseValue = new ConstantValue(baseValue);
    }
    
    public Calculation(Value baseValue) {
        BaseValue = baseValue;
    }
    
    public float Evaluate() {
        float result = baseValue.Get();
        float preMult = 1;
        foreach (Value value in preMults) {
            preMult += value.Get();
        }
        result *= preMult;
        foreach (Value value in adds) {
            result += value.Get();
        }
        float postMult = 1;
        foreach (Value value in postMults) {
            postMult += value.Get();
        }
        result *= postMult;

        return result;
    }
    
    public void AddPreMult(Value value, string id) {
        int index = preMultIds.IndexOf(id);
        if (index >= 0) {
            preMults[index] = value;
        } else {
            preMults.Add(value);
            preMultIds.Add(id);
        }
        Invalidate();
    }
    
    public bool RemovePreMult(string id) {
        int index = preMultIds.IndexOf(id);
        if (index == -1) return false;
        preMults.RemoveAt(index);
        preMultIds.RemoveAt(index);
        Invalidate();
        return true;
    }


    private void Invalidate() {
        
    }

    public Calculation Clone() {
        Calculation clone = new Calculation(baseValue.Clone());
        
        foreach (Value preMult in preMults) {
            clone.preMults.Add(preMult.Clone());
        }
        clone.preMultIds = new List<string>(preMultIds);
        
        foreach (Value add in adds) {
            clone.adds.Add(add.Clone());
        }
        clone.addIds = new List<string>(addIds);
        
        foreach (Value postMult in postMults) {
            clone.postMults.Add(postMult.Clone());
        }
        clone.postMultIds = new List<string>(postMultIds);
        
        return clone;
    }

    public override string ToString() {
        return $"Base: {BaseValue}\nPreMults: [{string.Join(", ", preMults)}]\nAdds: [{string.Join(", ", adds)}]\nPostMults: [{string.Join(", ", postMults)}]";
    }
}