using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.shop;

namespace MPAutoChess.logic.core.events;

public class ShopPurchaseEvent  : CancellableEvent {
    
    public Player Player { get; private set; }
    public ShopOffer[] Offers { get; set; }

}