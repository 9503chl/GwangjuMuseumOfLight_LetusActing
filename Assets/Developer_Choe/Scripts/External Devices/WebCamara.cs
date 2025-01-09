using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class WebCamara : MonoBehaviour
{
    private static WebCamTexture camTexture;

    private static RenderTexture _rt;

    public static RawImage WebCamInitialize(RawImage rawImage, int index)
    {
        camTexture = null;

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length != 0)
        {
            if (camTexture != null)
            {
                rawImage.texture = null;
                camTexture.Stop();
                camTexture = null;
            }
            WebCamDevice device = WebCamTexture.devices[index];
            camTexture = new WebCamTexture(device.name, (int)rawImage.rectTransform.rect.width, (int)rawImage.rectTransform.rect.height, 60);
            rawImage.texture = camTexture;
            camTexture.Play();

        }
        _rt = new RenderTexture((int)rawImage.rectTransform.rect.width, (int)rawImage.rectTransform.rect.height, 0);

        return rawImage;
    }

    public static Texture2D WebCamCapture(int width, int height)
    {
        WebCameraTarget[] webCameraTargets = FindObjectsOfType<WebCameraTarget>();

        if (webCameraTargets.Length == 0)
        {
            return null;
        }
        RawImage rawImage = webCameraTargets[0]._RawImage;

        return WebCamCapture(rawImage, width, height);
    }

    public static Texture2D WebCamCapture(RawImage rawImage, int width, int height)
    {
        Texture texture = rawImage.texture;
        return WebCamCapture(texture, width, height);
    }

    public static Texture2D WebCamCapture(Texture texture, int width, int height)
    {
        Graphics.Blit(texture, _rt);

        Texture2D texture2D = new Texture2D(_rt.width, _rt.height, TextureFormat.RGBA32, false);

        var old_rt = RenderTexture.active;
        RenderTexture.active = _rt;

        texture2D.ReadPixels(new Rect(0, 0, _rt.width, _rt.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = old_rt;

        Texture2D Temptexture = Texture2DUtility.Crop(texture2D, (_rt.width - width) / 2, (_rt.height - height) / 2, width, height);

        return Temptexture;
    }
    private void OnApplicationQuit()
    {
        camTexture.Stop();
        camTexture = null;
    }
}
