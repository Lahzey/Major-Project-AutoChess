
using ProtoBuf;

namespace MPAutoChess.logic.core.stats;

[ProtoContract]
public partial class ConstantValue : Value {
    
    [ProtoMember(1)] private float value;

    private ConstantValue() { } // for ProtoBuf serialization
    
    public ConstantValue(float value) {
        this.value = value;
    }
    
    public override float Get() {
        return value;
    }

    public override Value Clone() {
        return new ConstantValue(value);
    }
}