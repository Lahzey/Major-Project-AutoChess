using System;
using System.Collections.Generic;
using MPAutoChess.logic.core.networking;
using ProtoBuf;

namespace MPAutoChess.logic.core.stats;

[ProtoContract]
public class CompoundStats : IIdentifiable {
    public string Id { get; set; }
    
    [ProtoMember(1)] private Stats BaseStats { get; set; } = new Stats();
    [ProtoMember(2)] private Dictionary<StatType, CompoundCalculation> values = new Dictionary<StatType, CompoundCalculation>();
    private bool autoSendChanges = false;
    
    public IEnumerable<StatType> Types => values.Keys;

    public CompoundStats() { } // for protobuf serialization

    public CompoundStats(Stats baseStats) {
        BaseStats = baseStats;
        
        foreach (StatType statType in baseStats.Types) {
            values[statType] = new CompoundCalculation(baseStats.GetCalculation(statType));
        }
    }
    
    public virtual Calculation GetCalculation(StatType statType) {
        if (!values.ContainsKey(statType)) {
            values[statType] = new CompoundCalculation(BaseStats.GetCalculation(statType));
            if (autoSendChanges) {
                values[statType].SetAutoSendChanges(true);
                SendChanges();
            }
        }
        return values.GetValueOrDefault(statType);
    }
    
    public float GetValue(StatType statType) {
        return GetCalculation(statType).Evaluate();
    }
    
    public void SetAutoSendChanges(bool autoSend) {
        if (!ServerController.Instance.IsServer) return; // clients should never send changes, their information is not trusted
        if (autoSend == autoSendChanges) return;
        autoSendChanges = autoSend;
        foreach (CompoundCalculation calculation in values.Values) {
            calculation.SetAutoSendChanges(autoSend);
        }
    }

    protected void SendChanges() {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("Only the server can send changes to stats.");
        ServerController.Instance.PublishChange(this);
    }
}