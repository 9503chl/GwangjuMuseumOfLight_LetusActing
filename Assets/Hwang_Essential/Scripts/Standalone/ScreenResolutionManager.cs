using System;
using System.Collections;
using UnityEngine;

public class ScreenResolutionManager : MonoBehaviour
{
    public int Width = 1920;
    public int Height = 1080;
    public bool FullScreen = true;
    public bool KeepAspect = true;
    [Range(0f, 60f)]
    public float AutoChangeDelay = 1f;

    private void Start()
    {
        StartCoroutine(DelayedChange());
    }

    private IEnumerator DelayedChange()
    {
        yield return new WaitForSecondsRealtime(AutoChangeDelay);
        ChangeResolution(Width, Height, FullScreen, KeepAspect);
    }

    public static void ChangeResolution(int width, int height, bool fullScreen = true, bool keepAspect = true)
    {
        if (width > 0 && height > 0)
        {
            if (keepAspect)
            {
                if (Screen.currentResolution.width == width && Screen.currentResolution.height == height)
                {
                    Screen.SetResolution(width, height, fullScreen);
                }
                else
                {
                    float aspect;
                    if (Screen.currentResolution.width > Screen.currentResolution.height)
                    {
                        aspect = (float)Screen.currentResolution.width / width;
                    }
                    else
                    {
                        aspect = (float)Screen.currentResolution.height / height;
                    }
                    Screen.SetResolution(Mathf.RoundToInt(width * aspect), Mathf.RoundToInt(height * aspect), fullScreen);
                }
            }
            else
            {
                Screen.SetResolution(width, height, fullScreen);
            }
        }
    }
}
