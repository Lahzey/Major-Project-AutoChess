using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;
using UnitType = MPAutoChess.logic.core.unit.UnitType;

namespace MPAutoChess.logic.core.shop;

public abstract class ShopOffer {
    
    public bool Purchased { get; protected set; } = false;
    
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

public abstract class GoldCostingOffer : ShopOffer {

    public override bool CanAfford() {
        return PlayerController.Instance.CurrentPlayer.Gold >= GetCost();
    }

    public abstract int GetCost();

}

public class UnitOffer : GoldCostingOffer {
    
    [Export] public Unit Unit { get; set; }
    
    public override Texture2D GetTexture() {
        return Unit.Type.Icon;
    }

    public override bool IsEnabled() {
        return PlayerController.Instance.CurrentPlayer.Gold >= Unit.Type.Cost;
    }

    public override bool TryPurchase() {
        if (PlayerController.Instance.CurrentPlayer.TryPurchase(Unit)) {
            Purchased = true;
            return true;
        }
        return false;
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