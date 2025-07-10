using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public class SpriteLoader : AssetLoader
    {
        protected override Type assetType
        {
            get { return typeof(Sprite); }
        }

        protected override Type[] componentTypes
        {
            get { return new Type[] { typeof(Image), typeof(RawImage), typeof(SpriteRenderer) }; }
        }

        public Sprite sprite
        {
            get { return asset as Sprite; }
        }

        [SerializeField]
        [Tooltip("Set native size on apply")]
        private bool autoSize;
        public bool AutoSize
        {
            get { return autoSize; }
            set { if (autoSize != value) { autoSize = value; } }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            Image image = GetComponent<Image>();
            if (image != null)
            {
                FindPathFromResources(image.sprite);
            }
            else
            {
                RawImage rawImage = GetComponent<RawImage>();
                if (rawImage != null)
                {
                    FindPathFromResources(rawImage.texture);
                }
                else
                {
                    SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        FindPathFromResources(spriteRenderer.sprite);
                    }
                }
            }
            Load();
        }
#endif

        protected override void ApplyAsset()
        {
            if (sprite != null)
            {
                Image image = GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = sprite;
                    if (autoSize)
                    {
                        image.SetNativeSize();
                    }
                }
                else
                {
                    RawImage rawImage = GetComponent<RawImage>();
                    if (rawImage != null)
                    {
                        rawImage.texture = sprite.texture;
                        if (autoSize)
                        {
                            rawImage.SetNativeSize();
                        }
                    }
                    else
                    {
                        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                        {
                            spriteRenderer.sprite = sprite;
                            if (autoSize)
                            {
                                spriteRenderer.size = new Vector2(sprite.rect.width / sprite.pixelsPerUnit, sprite.rect.height / sprite.pixelsPerUnit);
                            }
                        }
                    }
                }
            }
        }
    }
}
