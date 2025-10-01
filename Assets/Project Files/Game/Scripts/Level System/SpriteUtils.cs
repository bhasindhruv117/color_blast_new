#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    public static class SpriteUtils
    {
        public static Sprite CombineSprites(Sprite backgroundSprite, Sprite foregroundSprite)
        {
            // Get the textures of the sprites
            Texture2D backgroundTexture = backgroundSprite.texture;
            Texture2D foregroundTexture = foregroundSprite.texture;

            // Create a new texture with the same dimensions as the background texture
            Texture2D combinedTexture = new Texture2D(backgroundTexture.width, backgroundTexture.height, TextureFormat.ARGB32, false);

            // Copy the background texture pixels to the combined texture
            Color[] backgroundPixels = backgroundTexture.GetPixels();
            combinedTexture.SetPixels(backgroundPixels);

            // Calculate the position to place the foreground sprite in the center of the background sprite
            int startX = (backgroundTexture.width - foregroundTexture.width) / 2;
            int startY = (backgroundTexture.height - foregroundTexture.height) / 2;

            // Copy the foreground texture pixels to the combined texture
            Color[] foregroundPixels = foregroundTexture.GetPixels();
            for (int y = 0; y < foregroundTexture.height; y++)
            {
                for (int x = 0; x < foregroundTexture.width; x++)
                {
                    Color bgColor = combinedTexture.GetPixel(startX + x, startY + y);
                    Color fgColor = foregroundPixels[y * foregroundTexture.width + x];
                    Color finalColor = Color.Lerp(bgColor, fgColor, fgColor.a / 1.0f);
                    combinedTexture.SetPixel(startX + x, startY + y, finalColor);
                }
            }

            // Apply the changes to the combined texture
            combinedTexture.Apply();

            // Create a new sprite from the combined texture
            return Sprite.Create(combinedTexture, new Rect(0, 0, combinedTexture.width, combinedTexture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
