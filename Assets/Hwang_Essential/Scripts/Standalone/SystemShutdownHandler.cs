using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

public class SystemShutdownHandler : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowText);

    [DllImport("user32.dll", EntryPoint = "CallWindowProcA")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "DefWindowProcA")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8)
        {
            return SetWindowLong64(hWnd, nIndex, dwNewLong);
        }
        return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    }

    private const uint WM_QUERYENDSESSION = 0x0011;
    private const uint WM_ENDSESSION = 0x0016;
    private const int GWL_WNDPROC = -4;

    private delegate IntPtr WndProcHandler(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [NonSerialized]
    private static IntPtr hWndUnityMain = IntPtr.Zero;

    [NonSerialized]
    private static IntPtr pUnityWndProc = IntPtr.Zero;

    public static event UnityAction OnShutdown;

    private void OnEnable()
    {
        if (Application.isEditor || !isActiveAndEnabled)
        {
            return;
        }
        if (hWndUnityMain == IntPtr.Zero)
        {
            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                hWndUnityMain = currentProcess.MainWindowHandle;
                if (hWndUnityMain == IntPtr.Zero)
                {
                    hWndUnityMain = FindWindow("UnityWndClass", Application.productName);
                }
                ReplaceWndProc();
            }
            catch (Exception)
            {
            }
        }
    }

    private void OnDisable()
    {
        RestoreWndProc();
    }

    private static void ReplaceWndProc()
    {
        if (hWndUnityMain != IntPtr.Zero && pUnityWndProc == IntPtr.Zero)
        {
            IntPtr pCustomWndProc = Marshal.GetFunctionPointerForDelegate((WndProcHandler)WndProc);
            if (pCustomWndProc != IntPtr.Zero)
            {
                pUnityWndProc = SetWindowLong(hWndUnityMain, GWL_WNDPROC, pCustomWndProc);
                Debug.Log("ReplaceWndProc succeess!");
            }
        }
    }

    private static void RestoreWndProc()
    {
        if (hWndUnityMain != IntPtr.Zero && pUnityWndProc != IntPtr.Zero)
        {
            SetWindowLong(hWndUnityMain, GWL_WNDPROC, pUnityWndProc);
            pUnityWndProc = IntPtr.Zero;
            Debug.Log("RestoreWndProc succeess!");
        }
    }

    private static void Suicide()
    {
        try
        {
            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Kill();
        }
        catch (Exception)
        {
            Application.Quit();
        }
    }

    private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_QUERYENDSESSION:
                Debug.Log("WM_QUERYENDSESSION");
                if (OnShutdown != null)
                {
                    OnShutdown.Invoke();
                }
                return DefWindowProc(hWnd, msg, wParam, lParam);
            case WM_ENDSESSION:
                Debug.Log("WM_ENDSESSION");
                Suicide();
                return DefWindowProc(hWnd, msg, wParam, lParam);
        }
        return CallWindowProc(pUnityWndProc, hWnd, msg, wParam, lParam);
    }
#endif
}
