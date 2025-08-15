using System;
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
            offers = value ?? new List<ShopOffer>();
            OnChange();
        }
    }

    public Player Player { get; private set; }

    public Shop() { } // For Protobuf serialization

    public Shop(Player player) {
        Player = player;
    }
    
    public ShopOffer GetOfferAt(int index) {
        if (index < 0 || index >= Offers.Count) {
            return null;
        }
        return Offers[index];
    }

    public float[] GetNormalizedRarityOdds() {
        float[] odds = GetShopOdds(PlayerController.Current.Player.Level);

        float totalOdds = 0;
        foreach (float odd in odds) totalOdds += odd;
        // use linq to return an array containing each value divided by totalOdds
        return odds.Select(o => o / totalOdds).ToArray();
    }

    public ShopOffer[] GenerateShopOffers() {
        float[] odds = GetNormalizedRarityOdds();
        ShopOffer[] offers = new ShopOffer[Size];
        for (int i = 0; i < offers.Length; i++) {
            UnitPool pool = UnitPool.OfRarity(WeightedRandomIndex(odds) + 1); // rarity starts at 1, 0 means no rarity like for a free special unit
            UnitOffer offer = new UnitOffer();
            offer.Unit = pool.TakeRandomUnit(GameSession.Instance.Random);
            offers[i] = offer;
        }
        return offers;
    }
    
    public void SetOffers(IEnumerable<ShopOffer> offers) {
        foreach (ShopOffer offer in Offers) {
            offer.Dispose();
        }
        Offers.Clear();
        Offers.AddRange(offers);
        OnChange();
    }

    private void OnChange() {
        if (ServerController.Instance.IsServer) {
            ServerController.Instance.PublishChange(this, Player); // forward to clients
            return;
        }
        if (PlayerController.Current?.Player.Shop != this) return; // just to be sure, currently the server should only send shop rolls to the owning player
        
        if (PlayerUI.Instance == null) throw new InvalidOperationException("PlayerUI is not initialized, cannot update shop offers.");
        else if (PlayerUI.Instance.Shop == null) throw new InvalidOperationException("PlayerUI.Shop is not initialized, cannot update shop offers.");
        else if (Offers == null) throw new InvalidOperationException("Offers is null, cannot update shop offers.");
        
        PlayerUI.Instance.Shop.SetOffers(Offers.ToArray());
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


    private static float[] GetShopOdds(int playerLevel) {
        playerLevel = Math.Clamp(playerLevel, 1, 12);
        switch (playerLevel) {
            case 1:
                return new float[] { 90, 10, 0, 0, 0 };
            case 2:
                return new float[] { 75, 25, 0, 0, 0 };
            case 3:
                return new float[] { 60, 30, 10, 0, 0 };
            case 4:
                return new float[] { 55, 32, 13, 0, 0 };
            case 5:
                return new float[] { 40, 35, 20, 5, 0 };
            case 6:
                return new float[] { 35, 32, 23, 10, 0 };
            case 7:
                return new float[] { 22, 27, 35, 15, 1 };
            case 8:
                return new float[] { 18, 22, 30, 22, 8 };
            case 9:
                return new float[] { 10, 15, 25, 35, 15 };
            case 10:
                return new float[] { 8, 12, 20, 40, 20 };
            case 11:
                return new float[] { 7, 8, 15, 40, 30 };
            case 12:
                return new float[] { 7, 8, 10, 35, 40 };
        }

        return null;
    }
}