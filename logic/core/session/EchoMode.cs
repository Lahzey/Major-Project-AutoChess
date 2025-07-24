using Godot;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
public class EchoMode : GameMode {
    public override void CreateUserInterface(Node2D parent) {
        
    }
    public override void Tick(double delta) {
        
    }
    public override int GetPhase() {
        return 0;
    }
    public override string GetPhaseName() {
        return "";
    }
}