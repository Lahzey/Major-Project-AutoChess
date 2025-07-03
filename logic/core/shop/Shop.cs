using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.unit;

namespace MPAutoChess.logic.core.shop;

public class Shop {
    
    public Player player { get; private set; }

    public int Size { get; set; } = 5;

    private List<ShopOffer> offers = new List<ShopOffer>();
    
    public Shop(Player player) {
        this.player = player;
    }

    public float[] GetNormalizedRarityOdds() {
        // TODO calculate from player level and fire event
        float[] odds = { 40, 30, 15, 10, 5 };

        float totalOdds = 0;
        foreach (float odd in odds) totalOdds += odd;
        // use linq to return an array containing each value divided by totalOdds
        return odds.Select(o => o / totalOdds).ToArray();
    }

    public void Reroll() {
        float[] odds = GetNormalizedRarityOdds();
        ShopOffer[] offers = new ShopOffer[Size];
        for (int i = 0; i < offers.Length; i++) {
            UnitPool pool = UnitPool.OfRarity(WeightedRandomIndex(odds) + 1); // rarity starts at 1, 0 means no rarity like for a free special unit
            UnitOffer offer = new UnitOffer();
            offer.Unit = pool.TakeRandomUnit(GameSession.Instance.Random);
            offers[i] = offer;
        }
        // TODO fire ShopRollEvent
        AddOffers(offers);
    }
    
    private void AddOffers(ShopOffer[] offers) {
        this.offers.Clear();
        this.offers.AddRange(offers);

        if (PlayerController.Instance.CurrentPlayer == player) { // just to be sure, currently the server should only send shop rolls to the owning player
            player.UI.ShopUI.AddOffers(offers);
        }
    }

    public static int WeightedRandomIndex(float[] odds) {
        float roll = GameSession.Instance.Random.NextSingle();
        float cumulative = 0f;
        for (int i = 0; i < odds.Length; i++) {
            cumulative += odds[i];
            if (roll < cumulative)
                return i;
        }
        return odds.Length - 1; // fallback (in case of rounding errors)
    }

}