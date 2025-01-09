using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureNSprite
{
    public static Sprite Texture2DToSprite(Texture2D texture)
    {
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }
    public static Texture2D SpriteToTexture2D(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D newTexture2D = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                         (int)sprite.textureRect.y,
                                                         (int)sprite.textureRect.width,
                                                         (int)sprite.textureRect.height);
            newTexture2D.SetPixels(newColors);
            newTexture2D.Apply();
            return newTexture2D;
        }
        else
            return sprite.texture;
    }
}
