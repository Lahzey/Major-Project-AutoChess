using System;
using Godot;
using MPAutoChess.logic.core.item;
using MPAutoChess.logic.core.networking;
using MPAutoChess.logic.core.placement;
using MPAutoChess.logic.core.player;
using MPAutoChess.logic.core.unit;
using MPAutoChess.logic.menu;

namespace MPAutoChess.logic.core.shop;

public partial class SellTarget : Area2D, IUnitDropTarget {
    
    private static readonly Texture2D IDLE_IMAGE = ResourceLoader.Load<Texture2D>("res://assets/ui/sell_target_off.png");
    private static readonly Texture2D DRAG_IMAGE = ResourceLoader.Load<Texture2D>("res://assets/ui/sell_target_on.png");
    
    [Export] public Sprite2D SellSprite { get; set; }
    
    private bool setupComplete = false;
    private bool dragging = false;
    
    public override void _Process(double delta) {
        if (setupComplete) return;
        if (PlayerController.Current == null) return;
        
        SellSprite.Texture = IDLE_IMAGE;
        PlayerController.Current.OnDragStart += _ => OnDragStart();
        PlayerController.Current.OnDragEnd += _ => OnDragEnd();
        
        WorldControls.Instance.AddControl(new ItemSellTarget(this), new WorldControls.PositioningInfo() {
            attachedTo = this,
            attachedToBounds = new Rect2(Vector2.One * -0.5f, Vector2.One),
            attachmentPoint = AttachmentPoint.CENTER,
            xGrowthDirection = GrowthDirection.BOTH,
            yGrowthDirection = GrowthDirection.BOTH,
            size =  Vector2.One
        });
        
        setupComplete = true;
    }

    private void OnDragStart() {
        SellSprite.Texture = DRAG_IMAGE;
        dragging = true;
    }
    
    private void OnDragEnd() {
        SellSprite.Texture = IDLE_IMAGE;
        dragging = false;
    }
    public Player GetPlayer() {
        return PlayerController.Current.Player;
    }
    public Vector2 ConvertToPlacement(Vector2 position, Unit forUnit) {
        return Vector2.Zero;
    }
    public bool IsValidDrop(Unit unit, Vector2 placement, Unit replacedUnit = null) {
        return true;
    }
    public void OnUnitDrop(Unit unit, Vector2 placement) {
        if (!ServerController.Instance.IsServer) throw new InvalidOperationException("SellTarget.OnDrop(unit, placement) should only be called on the server side.");
        if (unit.Container.GetPlayer() != PlayerController.Current.Player) return;

        unit.Sell();
    }
    
    public partial class ItemSellTarget : ItemDropTarget {

        private SellTarget parent;
        
        public ItemSellTarget() {}
        
        public ItemSellTarget(SellTarget parent) {
            this.parent = parent;
        }

        public override void _Notification(int what) {
            if (what == NotificationDragBegin) {
                bool? canDrop = CanDropCurrentData();
                if (canDrop.GetValueOrDefault()) parent.OnDragStart();
            } else if (what == NotificationDragEnd) {
                if (parent.dragging) parent.OnDragEnd();
            }
        }
        public override Player GetOwningPlayer() {
            return PlayerController.Current.Player;
        }
        public override bool CanDrop(Vector2 atPosition, ItemDragInfo dragInfo) {
            return true;
        }
        public override void OnDrop(Vector2 atPosition, ItemDragInfo dragInfo) {
            PlayerController.Current.SellItem(dragInfo.InventoryIndex);
        }
    }
}