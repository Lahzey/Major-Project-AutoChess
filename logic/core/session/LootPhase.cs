using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.shop;
using MPAutoChess.logic.util;
using ProtoBuf;

namespace MPAutoChess.logic.core.session;

[ProtoContract]
public partial class LootPhase : GamePhase, Choosable {

    private const double ALL_COMPLETE_COUNTDOWN = 3.0;
    
    private static readonly Texture2D ICON = ResourceLoader.Load<Texture2D>("res://assets/ui/phases/loot.png");
    private static readonly PackedScene LOOT_UI_SCENE = ResourceLoader.Load<PackedScene>("res://ui/LootPhaseUI.tscn");

    public const int POWER_PER_GOLD = 1;
    public const int POWER_PER_COMPONENT = 5;
    public const int POWER_PER_ITEM = 9;

    [ProtoMember(1)] private Dictionary<long, LootOption[]> options = new Dictionary<long, LootOption[]>(); // account ID to loot options mapping (saves us from serializing a duplicate of each Player since reference tracking is not turned on due to reliability issues)
    [ProtoMember(2)] private int powerLevel;
    
    private LootPhaseUI lootPhaseUI;
    private List<long> completedPlayers = new List<long>(); // used to track which players have completed the loot phase

    public static LootPhase Random() {
        LootPhase lootPhase = new LootPhase();
        lootPhase.powerLevel = GameSession.Instance.Random.Next(5, 51); // Random power level between 1 and 100
        foreach (Player player in GameSession.Instance.Players) {
            LootOption[] lootOptions = new LootOption[] {
                new GoldOffer(lootPhase.powerLevel),
                new ItemOffer(lootPhase.powerLevel)
            };
            lootPhase.options[player.Account.Id] = lootOptions;
        }
        return lootPhase;
    }

    public override void _Process(double delta) {
        RemainingTime -= delta;
    }

    public override string GetTitle(Player forPlayer) {
        return "Loot";
    }

    public override Texture2D GetIcon(Player forPlayer, out Color modulate) {
        modulate = new Color("#ad964b");
        return ICON;
    }

    public override int GetPowerLevel() {
        return powerLevel;
    }
    
    public override void Start() {
        if (ServerController.Instance.IsServer) return;
        
        lootPhaseUI = (LootPhaseUI) LOOT_UI_SCENE.Instantiate();
        
        LootOption[] lootOptions = options[PlayerController.Current.Player.Account.Id];
        for (int i = 0; i < lootOptions.Length; i++) {
            LootOption option = lootOptions[i];
            int index = i; // capture the current index for the callback
            Action chooseCallback = () => PlayerController.Current.MakeChoice(this, index);
            lootPhaseUI.AddLootOption(option.GetTexture(), option.GetName(), option.GetDescription(), chooseCallback, option.IsEnabledFunc());
        }

        PlayerUI.Instance.GamePhaseControls.SetPhaseControls(lootPhaseUI);
    }

    public void Choose(int choice) {
        ChooseLootOption(choice, true);
    }

    public void ChooseLootOption(int index, bool respond) {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("ChooseLootOption can only be called on the server.");
        if (completedPlayers.Contains(PlayerController.Current.Player.Account.Id)) {
            GD.PrintErr("Player " + PlayerController.Current.Player.Name + " has already completed the loot phase.");
            return;
        }
        
        Player player = PlayerController.Current.Player;
        if (!options.TryGetValue(player.Account.Id, out LootOption[] lootOptions)) throw new InvalidOperationException("No loot options found for player " + player.Name);
        if (index < 0 || index >= lootOptions.Length) {
            GD.PrintErr("Invalid loot option index: " + index); // only print error because this can happen if a malicious client sends an invalid index
            return;
        }
        lootOptions[index].Choose();
        if (respond) this.Respond(MethodName.AfterLootChoose);
        
        completedPlayers.Add(player.Account.Id);
        if (completedPlayers.Count == options.Count) {
            RemainingTime = Math.Min(RemainingTime, ALL_COMPLETE_COUNTDOWN);
            ServerController.Instance.PublishChange(this);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void AfterLootChoose() {
        if (ServerController.Instance.IsServer) throw new InvalidOperationException("AfterLootChoose can only be called on the client.");
        lootPhaseUI.LootOptionsContainer.SetVisible(false);
        lootPhaseUI.TitleLabel.Text = "Waiting for other players...";
        PlayerUI.Instance.GamePhaseControls.SetSemiTransparent(true);
    }
    
    public override void End() {
        if (ServerController.Instance.IsServer) {
            // Automatically choose the first loot option for players who have not yet made a choice
            foreach (long accountId in options.Keys) {
                if (!completedPlayers.Contains(accountId)) {
                    Player player = GameSession.Instance.Players.FirstOrDefault(p => p.Account.Id == accountId);
                    if (player == null) {
                        GD.PrintErr("Player with account ID " + accountId + " not found, but is in loot options.");
                        continue;
                    }
                    PlayerController.GetForPlayer(player).RunInContext(() => {
                        ChooseLootOption(0, false);
                    });
                }
            }
        } else {
            lootPhaseUI.QueueFree();
            lootPhaseUI = null;
        }
    }
}

[ProtoContract]
[ProtoInclude(100, typeof(GoldOffer))]
[ProtoInclude(101, typeof(ItemOffer))]
public abstract class LootOption {

    public virtual Func<bool> IsEnabledFunc() {
        return null; // null means always enabled, overriding this allows for custom enabled condition (called every frame)
    }

    public abstract void SetPowerLevel(int powerLevel);
    public abstract Texture2D GetTexture();
    public abstract string GetName();
    public abstract string GetDescription();
    public abstract void Choose();
    
}

[ProtoContract]
public class GoldOffer : LootOption {
    
    private static readonly Texture2D GOLD_TEXTURE = ResourceLoader.Load<Texture2D>("res://assets/ui/gold_icon.png");

    [ProtoMember(1)] private int goldAmount;
    
    private GoldOffer() { } // For ProtoBuf
    
    public GoldOffer(int powerLevel) {
        SetPowerLevel(powerLevel);
    }

    public sealed override void SetPowerLevel(int powerLevel) {
        goldAmount = powerLevel / LootPhase.POWER_PER_GOLD;
    }

    public override Texture2D GetTexture() {
        return GOLD_TEXTURE;
    }
    
    public override string GetName() {
        return "Gold";
    }
    
    public override string GetDescription() {
        return "Gain " + goldAmount + " gold.";
    }
    
    public override void Choose() {
        PlayerController.Current.Player.AddGold(goldAmount);
    }
}

[ProtoContract]
public class ItemOffer : LootOption {
    
    private static readonly Texture2D ANY_ITEM_TEXTURE = ResourceLoader.Load<Texture2D>("res://assets/ui/any_item_icon.png");
    private static readonly Texture2D ANY_ITEM_AND_GOLD_TEXTURE = ResourceLoader.Load<Texture2D>("res://assets/ui/any_item_and_gold_icon.png");

    [ProtoMember(1)] private int itemCount;
    [ProtoMember(2)] private int componentCount;
    [ProtoMember(3)] private int leftOverGold; // any power level that is left over after choosing items is given as gold

    private ItemOffer() { } // For ProtoBuf
    
    public ItemOffer(int powerLevel) {
        SetPowerLevel(powerLevel);
    }

    public sealed override void SetPowerLevel(int powerLevel) {
        itemCount = powerLevel / LootPhase.POWER_PER_ITEM;
        int leftOver = powerLevel % LootPhase.POWER_PER_ITEM;
        componentCount = leftOver / LootPhase.POWER_PER_COMPONENT;
        leftOver = leftOver % LootPhase.POWER_PER_COMPONENT;
        leftOverGold = leftOver / LootPhase.POWER_PER_GOLD;
    }

    public override Texture2D GetTexture() {
        return leftOverGold > 0 ? ANY_ITEM_AND_GOLD_TEXTURE : ANY_ITEM_TEXTURE;
    }
    
    public override string GetName() {
        return "Items";
    }
    
    public override string GetDescription() {
        // The following code is only executed once per offer, so no need to worry about performance (with string builders and such)
        string description = "Gain";
        if (itemCount > 0) {
            description = itemCount == 1 ? " an item" : $" {itemCount} items";
        }
        if (componentCount > 0) {
            description += itemCount > 0 ? (leftOverGold > 0 ? "," : " and") : "";
            description += componentCount == 1 ? " a component" : $" {componentCount} components";
        }
        if (leftOverGold > 0) {
            description += itemCount > 0 || componentCount > 0 ? " and" : "";
            description += $" {leftOverGold} gold";
        }
        if (description == "Gain") {
            description = "Gain nothing"; // should not happen, but would be funny if it did
        }

        return description + ".";
    }
    
    public override void Choose() {
        for (int i = 0; i < itemCount; i++) {
            ItemType itemType = GameSession.Instance.GetItemConfig().GetRandomItemType(ItemCategory.ITEM);
            PlayerController.Current.Player.Inventory.AddItem(new Item(itemType));
        }
        for (int i = 0; i < componentCount; i++) {
            ItemType itemType = GameSession.Instance.GetItemConfig().GetRandomItemType(ItemCategory.COMPONENT);
            PlayerController.Current.Player.Inventory.AddItem(new Item(itemType));
        }
        PlayerController.Current.Player.AddGold(leftOverGold);
        
        // for testing
        PlayerController.Current.Player.Inventory.AddItem(new Item(GD.Load<ItemType>("res://assets/items/heart_of_grease.tres")));
        PlayerController.Current.Player.Inventory.AddItem(new Item(GD.Load<ItemType>("res://assets/items/absorption_guard.tres")));
        PlayerController.Current.Player.Inventory.AddItem(new Item(GameSession.Instance.GetItemConfig().GetRandomItemType(ItemCategory.COMPONENT)));
        PlayerController.Current.Player.Inventory.AddItem(new Item(GameSession.Instance.GetItemConfig().GetRandomItemType(ItemCategory.COMPONENT)));
    }
}