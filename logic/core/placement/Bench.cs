using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.placement;

public partial class Bench : Node {

    public Player Player { get; set; }
    
    [Export] public Node SlotContainer { get; set; }
    
    private readonly List<SingleUnitSlot> slots = new List<SingleUnitSlot>();

    public override void _Ready() {
        slots.Clear();
        foreach (Node child in SlotContainer.GetChildren()) {
            if (child is SingleUnitSlot slot) {
                slots.Add(slot);
            }
        }
    }

    public SingleUnitSlot? GetFirstFreeSlot() {
        foreach (SingleUnitSlot slot in slots) {
            if (slot.Unit == null) {
                return slot;
            }
        }
        return null;
    }
}