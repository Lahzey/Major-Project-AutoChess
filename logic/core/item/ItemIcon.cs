using Godot;
using MPAutoChess.logic.util;

namespace MPAutoChess.logic.core.item;

public partial class ItemIcon : TextureRect {
    
    private static readonly Texture2D NO_ICON = ResourceLoader.Load<Texture2D>("res://assets/transparent.png"); // to keep the 1:1 aspect ratio, which influences sizing of parent controls
    private static readonly Texture2D STAR_ICON = ResourceLoader.Load<Texture2D>("res://assets/basic_star.png");
    private static readonly Color STAR_COLOR = new Color(1f, 0.694f, 0.141f);
    
    private static readonly Texture2D BORDER_TEXTURE = ResourceLoader.Load<Texture2D>("res://assets/ui/border.svg");
    private static readonly Color COMPONENT_COLOR = new Color("#384039");
    private static readonly Color ITEM_COLOR = new Color("#193b1d");
    private static readonly Color MYTHICAL_COMPONENT_COLOR = new Color("#47302a");
    private static readonly Color MYTHICAL_ITEM_COLOR = new Color("#541c11");
    
    private Item? item;
    public Item? Item {
        get => item;
        set {
            if (item != value) {
                item = value;
                SetTexture(item?.Type.Icon??NO_ICON);
                QueueRedraw();
            }
        }
    }

    private TextureRect border;
    private HBoxContainer starsContainer;
    private int starCount = 0;

    public override void _Ready() {
        border = new  TextureRect();
        border.SetTexture(BORDER_TEXTURE);
        border.ExpandMode = ExpandModeEnum.IgnoreSize;
        border.AnchorLeft = 0f;
        border.AnchorTop = 0f;
        border.AnchorRight = 1f;
        border.AnchorBottom = 1f;
        border.OffsetLeft = 0;
        border.OffsetTop = 0;
        border.OffsetRight = 0;
        border.OffsetBottom = 0;
        AddChild(border);
        
        starsContainer = new HBoxContainer();
        starsContainer.Alignment = BoxContainer.AlignmentMode.Center;
        starsContainer.AnchorLeft = 0f;
        starsContainer.AnchorTop = 0.8f;
        starsContainer.AnchorRight = 1f;
        starsContainer.AnchorBottom = 1f;
        starsContainer.OffsetLeft = 0;
        starsContainer.OffsetTop = 0;
        starsContainer.OffsetRight = 0;
        starsContainer.OffsetBottom = 0;
        starsContainer.SetHGrowDirection(GrowDirection.Both);
        starsContainer.Modulate = STAR_COLOR;
        AddChild(starsContainer);
    }

    public override void _Process(double delta) {
        // set border color (it is white, so modulate = color)
        border.Modulate = item == null ? Colors.Transparent : item.Type.Category switch {
            ItemCategory.COMPONENT => COMPONENT_COLOR,
            ItemCategory.ITEM => ITEM_COLOR,
            ItemCategory.MYTHICAL_COMPONENT => MYTHICAL_COMPONENT_COLOR,
            ItemCategory.MYTHICAL_ITEM => MYTHICAL_ITEM_COLOR,
            _ => Colors.Transparent
        };
        
        // ensure we have a star for each level of the item
        int desiredStarCount = item?.Level ?? 0;
        while (starCount < desiredStarCount) {
            TextureRect star = new TextureRect();
            star.SetTexture(STAR_ICON);
            star.ExpandMode = ExpandModeEnum.FitWidthProportional;
            starsContainer.AddChild(star);
            starCount++;
        }

        while (starCount > desiredStarCount) {
            starsContainer.GetChild(starCount - 1).QueueFree();
            starCount--;
        }
    }
}