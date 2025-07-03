using Godot;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.shop;
using MPAutoChess.logic.core.stats;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.player;

public partial class Player : Node {

    public Account Account { get; set; }

    public int Health { get; private set; } = 1000;

    public int Experience { get; private set; } = 0;

    public int Gold { get; private set; } = 100;
    
    public Shop Shop { get; private set; }
    
    [Export] public Board Board { get; private set; }
    
    [Export] public Bench Bench { get; private set; }
    
    [Export] public PlayerUI UI { get; private set; }
    
    public Calculation BoardSize { get; private set; } = new Calculation(5);

    public Player() {
        Shop = new Shop(this);
    }

    public override void _Ready() {
        Board.Player = this;
        Bench.Player = this;
    }

    public bool TryPurchase(Unit unit) {
        SingleUnitSlot benchSlot = Bench.GetFirstFreeSlot();
        if (benchSlot == null) return false;
        if (Gold >= unit.Type.Cost) {
            Gold -= unit.Type.Cost;
            benchSlot.AddUnit(unit, Vector2.Zero);
            return true;
        }
        return false;
    }
    
    public void MoveToTemporaryBench(Unit unit) {
        // TODO: Implement logic to move the unit to a temporary bench
    }

}