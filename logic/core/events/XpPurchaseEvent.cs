using MPAutoChess.logic.core.player;

namespace MPAutoChess.logic.core.events;

public class XpPurchaseEvent : CancellableEvent {

    public Player Player { get; private set; }
    public int Amount { get; set; }
    public int Cost { get; set; }

    public XpPurchaseEvent(Player player, int amount, int cost) {
        Player = player;
        Amount = amount;
        Cost = cost;
    }

}