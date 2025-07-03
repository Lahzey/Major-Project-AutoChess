using MPAutoChess.logic.core.shop;

namespace MPAutoChess.logic.core.@event;

public class ShopRollEvent : Event {
    
    public ShopOffer[] Offers { get; set; }
    
}