using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.shop;

namespace MPAutoChess.logic.core.events;

public class ShopRollEvent : CancellableEvent {
    
    public Player Player { get; private set; }
    public ShopOffer[] Offers { get; set; }
    
}