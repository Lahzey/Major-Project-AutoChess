using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.shop;

namespace MPAutoChess.logic.core.events;

public class ShopRollEvent : CancellableEvent {

    public Player Player { get; set; }
    public int Cost { get; set; }
    private ShopOffer[] Offers { get; set; }
    
    // only available in NotifyAfter
    public bool Success { get; set; }
    
    public ShopRollEvent(Player player, int cost, ShopOffer[] offers) {
        Player = player;
        Cost = cost;
        Offers = offers;
    }
    
    public IReadOnlyList<ShopOffer> GetOffers() {
        return Offers;
    }
    
    public void SetOffers(ShopOffer[] offers) {
        foreach (ShopOffer offer in Offers) {
            if (!offers.Contains(offer)) offer.Dispose();
        }
        Offers = offers;
    }
    
    public void ReplaceOffer(int index, ShopOffer offer) {
        if (Offers[index] == offer) return; // Otherwise replacing an offer with itself would dispose it
        Offers[index].Dispose(); // Dispose the old offer
        Offers[index] = offer; // Replace with the new offer
    }

    public void DisposeOffers() {
        foreach (ShopOffer offer in Offers) {
            offer.Dispose();
        }
        Offers = Array.Empty<ShopOffer>();
    }
    
    // these methods are inefficient because we are recreating a new array every time, but are there for completeness (will probably never be used)
    public void AddOffer(ShopOffer offer) {
        Offers = Offers.Append(offer).ToArray();
    }
    public void RemoveOffer(int index) {
        Offers[index].Dispose();
        Offers = Offers.Where((_, i) => i != index).ToArray(); // Remove the offer at the specified index
    }
}