using Godot;

namespace MPAutoChess.logic.core.item;

public partial class ItemIcon : TextureRect {
    
    private static readonly Texture2D NO_ICON = ResourceLoader.Load<Texture2D>("res://assets/transparent.png"); // to keep the 1:1 aspect ratio, which influences sizing of parent controls
    private static readonly Texture2D STAR_ICON = ResourceLoader.Load<Texture2D>("res://assets/basic_star.png");
    private static readonly Color STAR_COLOR = new Color(1f, 0.694f, 0.141f);
    
    private Item? item;
    public Item? Item {
        get => item;
        set {
            if (item != value) {
                item = value;
                SetTexture(item?.Type.Icon??NO_ICON);
            }
        }
    }

    private HBoxContainer starsContainer;
    private int starCount = 0;

    public override void _Ready() {
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
        // ensure we have a star for each level of the item above 1 (level 1 -> no stars, level 2 -> 1 star etc.)
        int desiredStarCount = item != null ? (item.Level - 1) : 0;
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