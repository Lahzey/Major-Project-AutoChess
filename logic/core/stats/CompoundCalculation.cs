using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace MPAutoChess.logic.core.stats;

[ProtoContract]
public class CompoundCalculation : Calculation {
    
    [ProtoMember(50)] public Calculation BaseCalculation { get; private set; }

    public CompoundCalculation() { } // for Protobuf serialization
    
    public CompoundCalculation(Calculation baseCalculation) : base(0) {
        BaseCalculation = baseCalculation;
    }

    protected internal override float GetBaseValue() {
        return BaseCalculation.GetBaseValue() + BaseValue.Get();
    }
    
    protected internal override IEnumerable<Value> GetPreMultValues() {
        return BaseCalculation.GetPreMultValues().Concat(preMultValues);
    }
    
    protected internal override IEnumerable<string> GetPreMultIds() {
        return BaseCalculation.GetPreMultIds().Concat(preMultIds);
    }
    
    protected internal override IEnumerable<Value> GetFlatValues() {
        return BaseCalculation.GetFlatValues().Concat(flatValues);
    }
    
    protected internal override IEnumerable<string> GetFlatIds() {
        return BaseCalculation.GetFlatIds().Concat(flatIds);
    }
    
    protected internal override IEnumerable<Value> GetPostMultValues() {
        return BaseCalculation.GetPostMultValues().Concat(postMultValues);
    }
    
    protected internal override IEnumerable<string> GetPostMultIds() {
        return BaseCalculation.GetPostMultIds().Concat(postMultIds);
    }
    
    public override Calculation Clone() {
        throw new System.NotImplementedException("CompoundCalculation does not support cloning.");
    }
    
}