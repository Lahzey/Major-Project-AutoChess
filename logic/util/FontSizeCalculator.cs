using System;
using Godot;

namespace MPAutoChess.logic.util;

public static class FontSizeCalculator {
    
    
    public static int GetFontSize(FontSizeType type, Viewport viewport) {
        return type switch {
            FontSizeType.SMALL => (int) (GetNormalFontSize(viewport) * 0.8f),
            FontSizeType.NORMAL => (int) GetNormalFontSize(viewport),
            FontSizeType.SUBTITLE => (int) (GetNormalFontSize(viewport) * 1.5f),
            FontSizeType.TITLE => (int) (GetNormalFontSize(viewport) * 2.5f),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid font size type")
        };
    }
    
    private static float GetNormalFontSize(Viewport viewport) {
        float standardAspectRatio = 1920f / 1080f;
        float baseSize = 16f; // Base font size for 1920x1080 resolution
        float viewportSize = Math.Min(viewport.GetVisibleRect().Size.X, viewport.GetVisibleRect().Size.Y * standardAspectRatio); // in case of non-standard aspect ratio, use the smaller dimension to avoid overflow
        float scaleFactor = viewportSize / 1920f; // Scale based on the width of the viewport
        return baseSize * scaleFactor;
    }
    
}

public enum FontSizeType {
    SMALL,
    NORMAL,
    SUBTITLE,
    TITLE
}