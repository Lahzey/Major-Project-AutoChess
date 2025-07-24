using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;
using ProtoBuf;
using UnitType = MPAutoChess.logic.core.unit.UnitType;

namespace MPAutoChess.logic.core.shop;

[ProtoContract]
[ProtoInclude(100, typeof(UnitOffer))]
public abstract class ShopOffer {

    private bool purchased = false;

    [ProtoMember(1)]
    public bool Purchased {
        get => purchased;
        protected set {
            purchased = value;
            if (ServerController.Instance.IsServer)
                ServerController.Instance.OnShopChange(PlayerController.Current.Player.Shop);
        }
    }
    
    public abstract Texture2D GetTexture();
    
    public abstract bool IsEnabled();

    public abstract bool CanAfford();
    
    public abstract bool TryPurchase();
    
    public virtual void SerializeData(Dictionary<string, object> data) {
        data["purchased"] = Purchased;
    }
    
    public virtual void DeserializeData(Dictionary<string, object> data) {
        if (data.TryGetValue("purchased", out object purchased)) {
            Purchased = (bool) purchased;
        }
    }
    
    public virtual void FillShopSlot(ShopSlot slot) {
        slot.ShopOffer = this;
        slot.Texture.Texture = GetTexture();
    }
}

[ProtoContract]
public abstract class GoldCostingOffer : ShopOffer {

    public override bool CanAfford() {
        return PlayerController.Current.Player.Gold >= GetCost();
    }

    public abstract int GetCost();

}

[ProtoContract]
public class UnitOffer : GoldCostingOffer {
    
    [Export] [ProtoMember(2)] public Unit Unit { get; set; }
    
    public override Texture2D GetTexture() {
        return Unit.Type.Icon;
    }

    public override bool IsEnabled() {
        return PlayerController.Current.Player.Gold >= Unit.Type.Cost;
    }

    public override bool TryPurchase() {
        SingleUnitSlot benchSlot = PlayerController.Current.Player.Bench.GetFirstFreeSlot();
        GD.Print($"Trying to purchase unit {Unit.Type.ResourcePath} for {Unit.Type.Cost} gold, bench slot: {benchSlot?.GetPath() ?? "null"}");
        if (benchSlot == null) return false;
        bool success = PlayerController.Current.Player.TryPurchase(Unit.Type.Cost, () => benchSlot.AddUnit(Unit, Vector2.Zero));
        if (success) Purchased = true;
        return success;
    }
    
    public override void SerializeData(Dictionary<string, object> data) {
        base.SerializeData(data);
        data["unit_type_path"] = Unit.Type.ResourcePath;
    }
    
    public override void DeserializeData(Dictionary<string, object> data) {
        base.DeserializeData(data);
        if (data.TryGetValue("unit_type_path", out object unitTypePath)) {
            UnitType type = ResourceLoader.Load<UnitType>((string)unitTypePath);
            Unit = UnitPool.For(type).TryTakeUnit(type);
        }
    }

    public override int GetCost() {
        return Unit.Type.Cost;
    }

    public override void FillShopSlot(ShopSlot slot) {
        base.FillShopSlot(slot);
        slot.SetBorderForRarity(Unit.Type.Rarity);
    }
}