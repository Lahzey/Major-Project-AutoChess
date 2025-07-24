using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.player;
using ProtoBuf;

namespace MPAutoChess.logic.core.placement;

[ProtoContract]
public partial class Bench : Node {
    
    private Player player;
    public Player Player {
        get => player;
        set {
            player = value;
            foreach (SingleUnitSlot slot in slots) { // in case player is set after _Ready
                slot.Player = player;
            }
        }
    }

    [Export] public Node SlotContainer { get; set; }
    
    [ProtoMember(1)] private List<SingleUnitSlot> slots = new List<SingleUnitSlot>();

    public override void _Ready() {
        slots.Clear();
        foreach (Node child in SlotContainer.GetChildren()) {
            if (child is SingleUnitSlot slot) {
                slots.Add(slot);
                slot.Player = Player;
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
    
    public IEnumerable<SingleUnitSlot> GetSlots() {
        return slots;
    }
}