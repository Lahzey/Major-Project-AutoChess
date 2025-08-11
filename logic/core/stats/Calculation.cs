using System;
using System.Collections.Generic;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.menu;
using ProtoBuf;

namespace MPAutoChess.logic.core.stats;

[ProtoContract]
public class Calculation : IIdentifiable {
    public string Id { get; set; }
    
    private bool autoSendChanges = false;

    private Value baseValue;
    [ProtoMember(1)] public Value BaseValue {
        get => baseValue;
        set {
            baseValue = value;
            ValuesChanged();
        }
    }
    
    [ProtoMember(2)] private List<Value> preMultValues = new List<Value>();
    [ProtoMember(3)] private List<string> preMultIds = new List<string>();
    
    [ProtoMember(4)] private List<Value> flatValues = new List<Value>();
    [ProtoMember(5)] private List<string> flatIds = new List<string>();
    
    [ProtoMember(6)] private List<Value> postMultValues = new List<Value>();
    [ProtoMember(7)] private List<string> postMultIds = new List<string>();
    
    public Calculation() {} // empty constructor for MessagePack serialization
    
    public Calculation(float baseValue) {
        BaseValue = baseValue;
        ValuesChanged();
    }
    
    public Calculation(Value baseValue) {
        BaseValue = baseValue;
        ValuesChanged();
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
        ValuesChanged();
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
        ValuesChanged();
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
        ValuesChanged();
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
        ValuesChanged();
        return true;
    }
    
    public void AddPostMult(Value value, string id) {
        int index = postMultIds.IndexOf(id);
        if (index >= 0) {
            postMultValues[index] = value;
        } else {
            postMultValues.Add(value);
            postMultIds.Add(id);
        }
        ValuesChanged();
    }
    
    public Value? GetPostMult(string id) {
        int index = postMultIds.IndexOf(id);
        if (index >= 0) {
            return postMultValues[index];
        }
        return null;
    }
    
    public bool RemovePostMult(string id) {
        int index = postMultIds.IndexOf(id);
        if (index == -1) return false;
        postMultValues.RemoveAt(index);
        postMultIds.RemoveAt(index);
        ValuesChanged();
        return true;
    }

    private void ValuesChanged() {
        if (autoSendChanges) {
            ServerController.Instance.PublishChange(this);
        }
    }
    
    public void SetAutoSendChanges(bool autoSend) {
        if (!ServerController.Instance.IsServer) return; // clients should never send changes, their information is not trusted
        if (autoSend == autoSendChanges) return;
        autoSendChanges = autoSend;
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

    public List<ContextMenuItem> GenerateContextMenu(StatType statType) {
        List<ContextMenuItem> contextMenu = new  List<ContextMenuItem>();
        contextMenu.Add(ContextMenuItem.Label(statType.Name));
        contextMenu.Add(ContextMenuItem.Label(statType.Description));
        contextMenu.Add(ContextMenuItem.Separator("Base"));
        contextMenu.Add(ContextMenuItem.Label(statType.ToString(BaseValue.Get(), 2)));

        if (preMultValues.Count > 0) {
            contextMenu.Add(ContextMenuItem.Separator("Pre Multipliers"));
            for (int i = 0; i < preMultValues.Count; i++) {
                Value value = preMultValues[i];
                string id = preMultIds[i];
                contextMenu.Add(ContextMenuItem.Label($"{id}: {statType.ToString(value.Get(), 2)}"));
            }
        }
        if (flatValues.Count > 0) {
            contextMenu.Add(ContextMenuItem.Separator("Flat Adds"));
            for (int i = 0; i < flatValues.Count; i++) {
                Value value = flatValues[i];
                string id = flatIds[i];
                contextMenu.Add(ContextMenuItem.Label($"{id}: {statType.ToString(value.Get(), 2)}"));
            }
        }
        if (postMultValues.Count > 0) {
            contextMenu.Add(ContextMenuItem.Separator("Post Multipliers"));
            for (int i = 0; i < postMultValues.Count; i++) {
                Value value = postMultValues[i];
                string id = postMultIds[i];
                contextMenu.Add(ContextMenuItem.Label($"{id}: {statType.ToString(value.Get(), 2)}"));
            }
        }
        contextMenu.Add(ContextMenuItem.Separator("Total"));
        contextMenu.Add(ContextMenuItem.Label(statType.ToString(Evaluate(), 2)));
        
        return contextMenu;
    }
}