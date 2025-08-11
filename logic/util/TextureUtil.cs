using Godot;
using Godot.Collections;

namespace MPAutoChess.logic.util;

public static class TextureUtil {
    private static Dictionary<string, Texture2D> grayScaleCache = new Dictionary<string, Texture2D>();


    public static Texture2D ToGrayScale(Texture2D texture) {
        if (texture == null) return null;

        string key = texture.ResourcePath;
        if (grayScaleCache.TryGetValue(key, out Texture2D cachedTexture)) {
            return cachedTexture;
        }

        // Get an editable copy of the texture's image
        Image img = texture.GetImage();

        for (int y = 0; y < img.GetHeight(); y++) {
            for (int x = 0; x < img.GetWidth(); x++) {
                Color color = img.GetPixel(x, y);
                // Calculate luminance using Rec. 709 weights
                float gray = color.R * 0.2126f + color.G * 0.7152f + color.B * 0.0722f;
                img.SetPixel(x, y, new Color(gray, gray, gray, color.A));
            }
        }

        // Create a new Texture2D from the modified image
        Texture2D grayScaleTexture = ImageTexture.CreateFromImage(img);

        grayScaleCache[key] = grayScaleTexture;

        return grayScaleTexture;
    }
}