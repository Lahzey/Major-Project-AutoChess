using Godot;

namespace MPAutoChess.logic.util;

public static class ColorExtensions {

    private const float ONE_THIRD = 1f / 3f; // precompute division for performance
    
    public static Color ToGrayScale(this Color color) {
        float gray = (color.R + color.G + color.B) * ONE_THIRD;
        return new Color(gray, gray, gray, color.A);
    }
    
}