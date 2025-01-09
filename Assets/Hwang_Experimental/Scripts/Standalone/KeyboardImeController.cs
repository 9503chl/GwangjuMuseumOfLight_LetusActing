using System;
using UnityEngine;
using System.Runtime.InteropServices;

public class KeyboardImeController : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
    [DllImport("imm32.dll")]
    private static extern IntPtr ImmGetContext(IntPtr hWnd);
    [DllImport("imm32.dll")]
    private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
    [DllImport("imm32.dll")]
    private static extern bool ImmGetOpenStatus(IntPtr hIMC);
    [DllImport("imm32.dll")]
    private static extern bool ImmGetConversionStatus(IntPtr hIMC, ref int lpfdwConvertion, ref int lpfdwSentence);
    [DllImport("imm32.dll")]
    private static extern bool ImmSimulateHotKey(IntPtr hWnd, int dwHotKeyID);

    private const int IME_CMODE_HANGUL = 0x0001;
    private const int IME_CMODE_FULLSHAPE = 0x0008;

    private const int IME_KHOTKEY_SHAPE_TOGGLE = 0x50;
    private const int IME_KHOTKEY_ENGLISH = 0x52;

    private static IntPtr activeHandle;

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            activeHandle = GetActiveWindow();
        }
        else
        {
            activeHandle = IntPtr.Zero;
        }
    }

    public int GetConversionMode()
    {
        int iMode = 0;
        int iSentence = 0;
        if (activeHandle != IntPtr.Zero)
        {
            IntPtr hIMC = ImmGetContext(activeHandle);
            if (ImmGetOpenStatus(hIMC))
            {
                ImmGetConversionStatus(hIMC, ref iMode, ref iSentence);
            }
            ImmReleaseContext(activeHandle, hIMC);
        }
        return iMode;
    }

    public bool IsHangulMode()
    {
        return ((GetConversionMode() & IME_CMODE_HANGUL) > 0);
    }

    public bool IsFullShapeMode()
    {
        return ((GetConversionMode() & IME_CMODE_FULLSHAPE) > 0);
    }

    public bool ToggleHangulMode()
    {
        if (activeHandle != IntPtr.Zero)
        {
            return ImmSimulateHotKey(activeHandle, IME_KHOTKEY_ENGLISH);
        }
        return false;
    }

    public bool ToggleFullShapeMode()
    {
        if (activeHandle != IntPtr.Zero)
        {
            return ImmSimulateHotKey(activeHandle, IME_KHOTKEY_SHAPE_TOGGLE);
        }
        return false;
    }
#endif
}
