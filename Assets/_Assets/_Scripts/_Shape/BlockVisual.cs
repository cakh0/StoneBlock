using UnityEngine;

// Neutral bevel sprite: unlike the red source texture, it supports every block tint.
public static class BlockVisual
{
    private static Sprite sprite;
    public static Sprite Sprite
    {
        get
        {
            if (sprite) return sprite;
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            texture.name = "BlockBevel";
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                float shade = y < 3 || x > 28 ? 0.65f : y > 28 || x < 3 ? 1 : 0.88f;
                bool corner = (x == 0 || x == 31) && (y == 0 || y == 31);
                texture.SetPixel(x,y,new Color(shade,shade,shade,corner ? 0 : 1));
            }
            texture.Apply(false, true);
            sprite = UnityEngine.Sprite.Create(texture, new Rect(0,0,32,32), Vector2.one * 0.5f, 32);
            sprite.name = "BlockBevel";
            return sprite;
        }
    }
}
