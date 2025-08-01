using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.session;
using MPAutoChess.logic.core.unit;
using ProtoBuf;

namespace MPAutoChess.logic.core.shop;

[ProtoContract]
public class Shop : IIdentifiable {
    public string Id { get; set; }

    [ProtoMember(1)] public int Size { get; set; } = 5;

    private List<ShopOffer> offers = new List<ShopOffer>();
    [ProtoMember(2, OverwriteList = true)] private List<ShopOffer> Offers {
        get => offers;
        set {
            offers = value;
            OnChange();
        }
    }

    public Shop() { } // For serialization only, do not use this constructor directly
    
    public ShopOffer GetOfferAt(int index) {
        if (index < 0 || index >= Offers.Count) {
            return null;
        }
        return Offers[index];
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
        Offers.Clear();
        Offers.AddRange(offers);
        OnChange();
    }

    private void OnChange() {
        if (ServerController.Instance.IsServer) {
            ServerController.Instance.OnShopChange(this); // forward to clients
        } else {
            if (PlayerController.Current?.Player.Shop == this) { // just to be sure, currently the server should only send shop rolls to the owning player
                PlayerController.Current.Player.UI.ShopUI.AddOffers(Offers.ToArray());
            }
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

    public int IndexOf(ShopOffer offer) {
        return Offers.IndexOf(offer);
    }
}