using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public class SpriteDownloader : AssetDownloader
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
            set { if (autoSize != value) { autoSize = value; ApplyAsset(); } }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            const string assetExtension = ".png";
            Image image = GetComponent<Image>();
            if (image != null)
            {
                FindPathFromAssets(image.sprite, assetExtension);
            }
            else
            {
                RawImage rawImage = GetComponent<RawImage>();
                if (rawImage != null)
                {
                    FindPathFromAssets(rawImage.texture, assetExtension);
                }
                else
                {
                    SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        FindPathFromAssets(spriteRenderer.sprite, assetExtension);
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
