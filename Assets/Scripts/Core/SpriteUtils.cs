using UnityEngine;

namespace DragonCeltas
{
    public static class SpriteUtils
    {
        public static Sprite CreatePixelSprite(Vector2 pivot)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), pivot, 1f);
        }
    }
}