using System;
using System.IO;

namespace UnityEngine
{
    public static class Texture2DUtility
    {
        public static Texture2D Flip(Texture2D source, bool flipX, bool flipY)
        {
            if (source == null)
            {
                return null;
            }
            int sourceWidth = source.width;
            int sourceHeight = source.height;
            Color[] sourcePixels = source.GetPixels();
            Color[] resultPixels = new Color[sourceWidth * sourceHeight];
            for (int y = 0; y < sourceHeight; y++)
            {
                for (int x = 0; x < sourceWidth; x++)
                {
                    resultPixels[x + y * sourceWidth] = sourcePixels[(flipX ? sourceWidth - x - 1 : x) + (flipY ? sourceHeight - y - 1 : y) * sourceWidth];
                }
            }
            bool mipmap = source.mipmapCount > 1;
            Texture2D result = new Texture2D(sourceWidth, sourceHeight, source.format, mipmap);
            result.SetPixels(resultPixels);
            result.Apply(mipmap);
            return result;
        }

        public static Texture2D FlipX(Texture2D source)
        {
            return Flip(source, true, false);
        }

        public static Texture2D FlipY(Texture2D source)
        {
            return Flip(source, false, true);
        }

        public static Texture2D Rotate180(Texture2D source)
        {
            return Flip(source, true, true);
        }

        public static Texture2D Rotate90(Texture2D source, bool clockwise = true)
        {
            if (source == null)
            {
                return null;
            }
            int sourceWidth = source.height;
            int sourceHeight = source.width;
            Color[] sourcePixels = source.GetPixels();
            Color[] resultPixels = new Color[sourceWidth * sourceHeight];
            for (int y = 0; y < sourceHeight; y++)
            {
                for (int x = 0; x < sourceWidth; x++)
                {
                    resultPixels[x + y * sourceWidth] = sourcePixels[clockwise ? (sourceHeight - y - 1) + x * sourceHeight : y + (sourceWidth - x - 1) * sourceHeight];
                }
            }
            bool mipmap = source.mipmapCount > 1;
            Texture2D result = new Texture2D(sourceWidth, sourceHeight, source.format, mipmap);
            result.SetPixels(resultPixels);
            result.Apply(mipmap);
            return result;
        }

        public static Texture2D Rotate270(Texture2D source)
        {
            return Rotate90(source, false);
        }

        public static Texture2D ResizeTo(Texture2D source, int targetWidth, int targetHeight, bool keepAspect = false)
        {
            if (source == null)
            {
                return null;
            }
            int sourceWidth = source.width;
            int sourceHeight = source.height;
            float sourceAspect = (float)sourceWidth / sourceHeight;
            if (targetWidth <= 0)
            {
                targetWidth = Mathf.RoundToInt(targetHeight * sourceAspect);
            }
            else if (targetHeight <= 0)
            {
                targetHeight = Mathf.RoundToInt(targetWidth / sourceAspect);
            }
            if (targetWidth == 0 || targetHeight == 0)
            {
                return null;
            }
            if (keepAspect)
            {
                float targetAspect = (float)targetWidth / targetHeight;
                if (sourceAspect < targetAspect)
                {
                    targetWidth = Mathf.RoundToInt(targetWidth * sourceAspect / targetAspect);
                }
                else if (sourceAspect > targetAspect)
                {
                    targetHeight = Mathf.RoundToInt(targetHeight / sourceAspect * targetAspect);
                }
            }
            float factorX = (float)targetWidth / sourceWidth;
            float factorY = (float)targetHeight / sourceHeight;
            Color[] sourcePixels = source.GetPixels();
            Color[] resultPixels = new Color[targetWidth * targetHeight];
            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    Vector2 p = new Vector2(Mathf.Clamp(x / factorX, 0, sourceWidth - 1), Mathf.Clamp(y / factorY, 0, sourceHeight - 1));
                    Color c11 = sourcePixels[Mathf.FloorToInt(p.x) + sourceWidth * Mathf.FloorToInt(p.y)];
                    Color c12 = sourcePixels[Mathf.FloorToInt(p.x) + sourceWidth * Mathf.CeilToInt(p.y)];
                    Color c21 = sourcePixels[Mathf.CeilToInt(p.x) + sourceWidth * Mathf.FloorToInt(p.y)];
                    Color c22 = sourcePixels[Mathf.CeilToInt(p.x) + sourceWidth * Mathf.CeilToInt(p.y)];
                    resultPixels[x + y * targetWidth] = Color.Lerp(Color.Lerp(c11, c12, p.y), Color.Lerp(c21, c22, p.y), p.x);
                }
            }
            bool mipmap = source.mipmapCount > 1;
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, mipmap);
            result.SetPixels(resultPixels);
            result.Apply(mipmap);
            return result;
        }

        public static Texture2D ResizeTo(Texture2D source, Vector2Int targetSize, bool keepAspect = false)
        {
            return ResizeTo(source, targetSize.x, targetSize.y, keepAspect);
        }

        public static Texture2D ResizeBy(Texture2D source, float targetScaleX, float targetScaleY)
        {
            if (source == null)
            {
                return null;
            }
            return ResizeTo(source, Mathf.RoundToInt(source.width * targetScaleX), Mathf.RoundToInt(source.height * targetScaleY));
        }

        public static Texture2D ResizeBy(Texture2D source, Vector2 targetScale)
        {
            if (source == null)
            {
                return null;
            }
            return ResizeTo(source, Mathf.RoundToInt(source.width * targetScale.x), Mathf.RoundToInt(source.height * targetScale.y));
        }

        public static Texture2D Crop(Texture2D source, int targetLeft, int targetTop, int targetWidth, int targetHeight)
        {
            if (source == null)
            {
                return null;
            }
            int sourceWidth = source.width;
            int sourceHeight = source.height;
            if (targetLeft < 0 || targetLeft > sourceWidth)
            {
                targetLeft = 0;
            }
            if (targetTop < 0 || targetTop > sourceHeight)
            {
                targetTop = 0;
            }
            if (targetLeft + targetWidth > sourceWidth)
            {
                targetWidth = sourceWidth - targetLeft;
            }
            if (targetTop + targetHeight > sourceHeight)
            {
                targetHeight = sourceHeight - targetTop;
            }
            if (targetWidth == 0 || targetHeight == 0)
            {
                return null;
            }
            Color[] sourcePixels = source.GetPixels();
            Color[] resultPixels = new Color[targetWidth * targetHeight];
            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    resultPixels[x + y * targetWidth] = sourcePixels[(x + targetLeft) + (y + (sourceHeight - targetHeight - targetTop)) * sourceWidth];
                }
            }
            bool mipmap = source.mipmapCount > 1;
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, mipmap);
            result.SetPixels(resultPixels);
            result.Apply(mipmap);
            return result;
        }

        public static Texture2D Crop(Texture2D source, RectInt targetRect)
        {
            return Crop(source, targetRect.x, targetRect.y, targetRect.width, targetRect.height);
        }

        public static Texture2D Cutout(Texture2D source, int targetLeft, int targetTop, int targetRight, int targetBottom)
        {
            if (source == null)
            {
                return null;
            }
            return Crop(source, targetLeft, targetTop, source.width - targetRight, source.height - targetBottom);
        }

        public static Texture2D Cutout(Texture2D source, RectOffset targetOffset)
        {
            if (source == null)
            {
                return null;
            }
            return Crop(source, targetOffset.left, targetOffset.top, source.width - targetOffset.right, source.height - targetOffset.bottom);
        }

        public static Texture2D FillRect(Texture2D source, int fillLeft, int fillTop, int fillWidth, int fillHeight, Color fillColor)
        {
            if (source == null)
            {
                return null;
            }
            int sourceWidth = source.width;
            int sourceHeight = source.height;
            Color[] sourcePixels = source.GetPixels();
            Color[] resultPixels = new Color[sourceWidth * sourceHeight];
            for (int y = 0; y < sourceHeight; y++)
            {
                for (int x = 0; x < sourceWidth; x++)
                {
                    if (x >= fillLeft && x < fillLeft + fillWidth && y >= (sourceHeight - fillHeight - fillTop) && y < sourceHeight - fillTop)
                    {
                        resultPixels[x + y * sourceWidth] = fillColor;
                    }
                    else
                    {
                        resultPixels[x + y * sourceWidth] = sourcePixels[x + y * sourceWidth];
                    }
                }
            }
            bool mipmap = source.mipmapCount > 1;
            Texture2D result = new Texture2D(sourceWidth, sourceHeight, source.format, mipmap);
            result.SetPixels(resultPixels);
            result.Apply(mipmap);
            return result;
        }

        public static Texture2D FillRect(Texture2D source, RectInt fillRect, Color fillColor)
        {
            return FillRect(source, fillRect.x, fillRect.y, fillRect.width, fillRect.height, fillColor);
        }

        public static Texture2D DrawTexture(Texture2D source, int drawLeft, int drawTop, Texture2D target)
        {
            if (source == null)
            {
                return null;
            }
            int sourceWidth = source.width;
            int sourceHeight = source.height;
            int targetWidth = target.width;
            int targetHeight = target.height;
            Color[] sourcePixels = source.GetPixels();
            Color[] targetPixels = target.GetPixels();
            Color[] resultPixels = new Color[sourceWidth * sourceHeight];
            Color pixel;
            for (int y = 0; y < sourceHeight; y++)
            {
                for (int x = 0; x < sourceWidth; x++)
                {
                    if (x >= drawLeft && x < drawLeft + targetWidth && y >= (sourceHeight - targetHeight - drawTop) && y < sourceHeight - drawTop)
                    {
                        pixel = targetPixels[(x - drawLeft) + (y - (sourceHeight - targetHeight - drawTop)) * targetWidth];
                        resultPixels[x + y * sourceWidth] = Color.Lerp(sourcePixels[x + y * sourceWidth], pixel, pixel.a);
                    }
                    else
                    {
                        resultPixels[x + y * sourceWidth] = sourcePixels[x + y * sourceWidth];
                    }
                }
            }
            bool mipmap = source.mipmapCount > 1;
            Texture2D result = new Texture2D(sourceWidth, sourceHeight, source.format, mipmap);
            result.SetPixels(resultPixels);
            result.Apply(mipmap);
            return result;
        }

        public static Texture2D DrawTexture(Texture2D source, Vector2Int drawPosition, Texture2D target)
        {
            return DrawTexture(source, drawPosition.x, drawPosition.y, target);
        }

        public static Texture2D Expand(Texture2D source, int borderLeft, int borderTop, int borderRight, int borderBottom, Color borderColor)
        {
            if (source == null)
            {
                return null;
            }
            int sourceWidth = source.width;
            int sourceHeight = source.height;
            int targetWidth = sourceWidth + borderLeft + borderRight;
            int targetHeight = sourceHeight + borderTop + borderBottom;
            Color[] sourcePixels = source.GetPixels();
            Color[] resultPixels = new Color[targetWidth * targetHeight];
            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    if (x < borderLeft || y < borderBottom || x >= sourceWidth + borderLeft || y >= sourceHeight + borderBottom)
                    {
                        resultPixels[x + y * targetWidth] = borderColor;
                    }
                    else
                    {
                        resultPixels[x + y * targetWidth] = sourcePixels[(x - borderLeft) + (y - borderBottom) * sourceWidth];
                    }
                }
            }
            bool mipmap = source.mipmapCount > 1;
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, mipmap);
            result.SetPixels(resultPixels);
            result.Apply(mipmap);
            return result;
        }

        public static Texture2D Expand(Texture2D source, RectOffset borderSize, Color borderColor)
        {
            return Expand(source, borderSize.left, borderSize.top, borderSize.right, borderSize.bottom, borderColor);
        }

        public static Texture2D Create(int textureWidth, int textureHeight, Color fillColor, TextureFormat format = TextureFormat.RGBA32, bool mipmap = true)
        {
            if (textureWidth < 2)
            {
                textureWidth = 2;
            }
            if (textureHeight < 2)
            {
                textureHeight = 2;
            }
            Texture2D result = new Texture2D(textureWidth, textureHeight, format, mipmap);
            Color[] resultPixels = new Color[textureWidth * textureHeight];
            for (int i = 0; i < resultPixels.Length; i++)
            {
                resultPixels[i] = fillColor;
            }
            result.SetPixels(resultPixels);
            result.Apply(mipmap);
            return result;
        }

        public static Texture2D Duplicate(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }
            byte[] rawData = texture.GetRawTextureData();
            Texture2D result = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);
            result.LoadRawTextureData(rawData);
            result.Apply(true);
            return result;
        }

        public static Texture2D CopyFrom(RenderTexture renderTexture)
        {
            if (renderTexture == null)
            {
                return null;
            }
            RenderTexture activeTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Texture2D result = new Texture2D(renderTexture.width, renderTexture.height);
            result.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            result.Apply(true);
            RenderTexture.active = activeTexture;
            return result;
        }

        public static Texture2D CopyFrom(Texture texture)
        {
            if (texture == null)
            {
                return null;
            }
            RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height);
            RenderTexture activeTexture = RenderTexture.active;
            Graphics.Blit(texture, renderTexture);
            RenderTexture.active = renderTexture;
            Texture2D result = new Texture2D(renderTexture.width, renderTexture.height);
            result.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            result.Apply(true);
            RenderTexture.active = activeTexture;
            RenderTexture.ReleaseTemporary(renderTexture);
            return result;
        }

        public static Texture2D CaptureFromCamera(Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }
            if (camera != null)
            {
                RenderTexture renderTexture = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight);
                RenderTexture targetTexture = camera.targetTexture;
                RenderTexture activeTexture = RenderTexture.active;
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                Texture2D result = new Texture2D(renderTexture.width, renderTexture.height);
                result.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                result.Apply(true);
                camera.targetTexture = targetTexture;
                RenderTexture.active = activeTexture;
                RenderTexture.ReleaseTemporary(renderTexture);
                return result;
            }
            return null;
        }

        public static Texture2D LoadFromFile(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = string.Format("{0}/image.jpg", Application.persistentDataPath);
            }
            string ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
            {
                path = string.Format("{0}.jpg", path);
            }
            string dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir))
            {
                path = string.Format("{0}/{1}", Application.persistentDataPath, path);
            }
            try
            {
                Texture2D result = new Texture2D(2, 2);
                if (result.LoadImage(File.ReadAllBytes(path)))
                {
                    return result;
                }
                Object.Destroy(result);
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static bool SaveToFile(Texture2D source, string path = null, int quality_JPG = 0)
        {
            if (source == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(path))
            {
                path = string.Format("{0}/image.jpg", Application.persistentDataPath);
            }
            string ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
            {
                path = string.Format("{0}.jpg", path);
            }
            string dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir))
            {
                path = string.Format("{0}/{1}", Application.persistentDataPath, path);
                dir = Application.persistentDataPath;
            }
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                if (string.Compare(ext, ".png", true) == 0)
                {
                    File.WriteAllBytes(path, source.EncodeToPNG());
                }
#if UNITY_2018_3_OR_NEWER
                else if (string.Compare(ext, ".tga", true) == 0)
                {
                    File.WriteAllBytes(path, source.EncodeToTGA());
                }
#endif
                else if (quality_JPG == 0)
                {
                    File.WriteAllBytes(path, source.EncodeToJPG());
                }
                else
                {
                    File.WriteAllBytes(path, source.EncodeToJPG(quality_JPG));
                }
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
