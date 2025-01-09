using System;
using UnityEngine;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(ForegroundWindowGuard))]
    public class ScreenResolutionManagerExtension : PivotalManager
    {
        public override void OnStart()
        {
            base.OnStart();
        }


        public static void ChangeResolution(int width, int height, bool fullScreen)
        {
            if (width > 0 && height > 0)
            {
                Screen.SetResolution(width, height, fullScreen);
            }
        }
    }
}
