using Godot;
using MPAutoChess.logic.core.shop;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
[ProtoInclude(1, typeof(EchoMode))]
public abstract class GameMode {

    public abstract void CreateUserInterface(Node2D parent);
    
    public abstract void Tick(double delta);

    public abstract int GetPhase();

    public abstract string GetPhaseName();

}