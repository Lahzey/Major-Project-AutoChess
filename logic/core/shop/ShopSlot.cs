using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Bridge;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;
using MPAutoChess.logic.util;

namespace MPAutoChess.logic.core.shop;

[Tool]
public partial class ShopSlot : Control {
    private static readonly Color NORMAL_MODULATE = new Color(1f, 1f, 1f);
    private static readonly Color HOVER_MODULATE = new Color(1.5f, 1.5f, 1.5f);
    private static readonly Color PRESSED_MODULATE = new Color(0.8f, 0.8f, 0.8f);
    private static readonly Color PURCHASED_MODULATE = new Color(0f, 0f, 0f, 0f);
    
    private Godot.Collections.Dictionary<int, Texture2D> rarityBorders = new Godot.Collections.Dictionary<int, Texture2D>();

    [Export] public TextureButton BuyButton { get; private set; }
    [Export] public TextureRect Texture { get; private set; }
    public ShopOffer ShopOffer { get; set; }
    
    private Texture2D originalTexture;
    private Texture2D grayScaleTexture;
    
    public void SetTexture(Texture2D texture) {
        originalTexture = texture;
        grayScaleTexture = TextureUtil.ToGrayScale(texture);
    }

    public void SetBorderForRarity(UnitRarity typeRarity) {
        BuyButton.TextureNormal = rarityBorders.GetValueOrDefault((int) typeRarity);
    }

    public override void _Ready() {
        if (BuyButton != null)
            BuyButton.Pressed += TryPurchase;
        else
            GD.PrintErr("BuyButton is not set for ShopSlot. Please assign it in the editor.");
    }

    public override void _Process(double delta) {
        if (BuyButton == null) return;
        
        bool hasOffer = ShopOffer != null && !ShopOffer.Purchased;
        Color colorModulate = !hasOffer ? PURCHASED_MODULATE : BuyButton.IsPressed() ? PRESSED_MODULATE : BuyButton.IsHovered() ? HOVER_MODULATE : NORMAL_MODULATE;
        bool enabled = hasOffer && ShopOffer.IsEnabled() && ShopOffer.CanAfford();

        BuyButton.Disabled = !enabled;
        BuyButton.Modulate = enabled ? colorModulate : colorModulate.Darkened(0.2f);
        Texture.Modulate = colorModulate;
        Texture.Texture = enabled ? originalTexture : grayScaleTexture;
    }

    private void TryPurchase() {
        if (ShopOffer == null || ShopOffer.Purchased) return;
        PlayerController.Current.BuyShopOffer(ShopOffer);
        if (ShopOffer.Purchased) {
            BuyButton.TextureNormal = null;
        }
    }

    public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList() {
        Godot.Collections.Array<Godot.Collections.Dictionary> list = new();
        foreach (UnitRarity rarity in Enum.GetValues(typeof(UnitRarity))) {
            list.Add(new Godot.Collections.Dictionary() {
                { "name", $"rarity_border/{rarity}" },
                { "type", (int) Variant.Type.Object },
                { "hint", (int) PropertyHint.ResourceType },
                { "hint_string", "Texture2D" }
            });
        }

        return list;
    }

    public override Variant _Get(StringName property) {
        if (property.ToString().StartsWith("rarity_border/")) {
            string rarityName = property.ToString().Substring("rarity_border/".Length);
            if (Enum.TryParse(rarityName, out UnitRarity rarity)) {
                return rarityBorders.GetValueOrDefault((int) rarity);
            }
        }

        return base._Get(property);
    }

    public override bool _Set(StringName property, Variant value) {
        if (property.ToString().StartsWith("rarity_border/")) {
            string rarityName = property.ToString().Substring("rarity_border/".Length);
            if (Enum.TryParse(rarityName, out UnitRarity rarity)) {
                rarityBorders[(int) rarity] = value.As<Texture2D>();
                return true;
            }
        }

        return base._Set(property, value);
    }
}