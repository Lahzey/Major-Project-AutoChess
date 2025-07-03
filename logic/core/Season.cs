using Godot;
using UnitCollection = MPAutoChess.logic.core.unit.UnitCollection;

namespace MPAutoChess.logic.core;

public abstract partial class Season : Node {
    
    [Export] public UnitCollection Units { get; set; }
}