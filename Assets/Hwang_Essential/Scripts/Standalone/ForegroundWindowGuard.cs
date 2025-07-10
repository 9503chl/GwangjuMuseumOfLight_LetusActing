using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Threading;
using Debug = UnityEngine.Debug;

public class ForegroundWindowGuard : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    [Tooltip("Allow popup windows when 'Start Menu' is activated. (Windows key or Ctrl+Esc key).")]
    [SerializeField]
    private bool allowWinStartMenu = true;

    [Tooltip("Allow popup windows when task manager is activated. (Ctrl+Shift+Esc key)")]
    [SerializeField]
    private bool allowTaskManager = true;

    [Tooltip("Allow popup windows when task switching is activated. (Windows+Tab key or Alt+Tab key)")]
    [SerializeField]
    private bool allowTaskSwitching = true;

    [Tooltip("Activates if previous window is found, and close current window.")]
    [SerializeField]
    private bool useSingleInstance = false;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern bool SetActiveWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(IntPtr hwnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowText);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsIconic(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint dwFlags);

    private const int HWND_TOPMOST = -1;
    private const int HWND_NOTOPMOST = -2;
    private const int SW_MINIMIZE = 6;
    private const int SW_RESTORE = 9;
    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOACTIVATE = 0x0010;

    [NonSerialized]
    private static IntPtr hWndUnityMain = IntPtr.Zero;

    [NonSerialized]
    private List<IntPtr> hWndUnitySubs = new List<IntPtr>();

    [NonSerialized]
    private bool keepForeground = false;

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
                Debug.Log(string.Format("Unity main window : {0}", (int)hWndUnityMain));
                SetForegroundWindow(hWndUnityMain);
            }
            catch (Exception)
            {
            }
            StartCoroutine(SingleInstance());
            StartCoroutine(KeepForeground());
        }
    }

    private bool ForceSetForegroundWindow(IntPtr hWnd)
    {
        uint dwProcessId;
        uint dwThreadId = GetWindowThreadProcessId(hWnd, out dwProcessId);
        uint dwForegroundThreadId = GetWindowThreadProcessId(GetForegroundWindow(), out dwProcessId);
        if (dwThreadId == dwForegroundThreadId)
        {
            SetActiveWindow(hWnd);
            return BringWindowToTop(hWnd);
        }
        else
        {
            bool result = false;
            if (AttachThreadInput(dwThreadId, dwForegroundThreadId, true))
            {
                SetActiveWindow(hWnd);
                result = BringWindowToTop(hWnd);
                AttachThreadInput(dwThreadId, dwForegroundThreadId, false);
            }
            return result;
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (Application.isEditor || !isActiveAndEnabled)
        {
            return;
        }
        if (focus)
        {
            keepForeground = false;
            IntPtr hWnd = GetActiveWindow();
            if (hWnd == hWndUnityMain)
            {
                if (hWndUnitySubs.Contains(hWnd))
                {
                    hWndUnitySubs.Remove(hWnd);
                }
            }
            else
            {
                if (!hWndUnitySubs.Contains(hWnd))
                {
                    hWndUnitySubs.Add(hWnd);
                }
            }
            Debug.Log(string.Format("Unity window got focus : {0}", (int)hWnd));
            foreach (IntPtr hWndUnitySub in hWndUnitySubs)
            {
                SetWindowPos(hWndUnitySub, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            }
            SetWindowPos(hWndUnityMain, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }
        else if (hWndUnityMain != IntPtr.Zero)
        {
            bool allowed = false;
            IntPtr hWnd = GetForegroundWindow();
            int retry = 10;
            while (hWnd == IntPtr.Zero && retry > 0)
            {
                retry--;
                Thread.Sleep(100);
                hWnd = GetForegroundWindow();
            }
            if (hWnd == IntPtr.Zero)
            {
                keepForeground = true;
                return;
            }
            foreach (IntPtr hWndUnitySub in hWndUnitySubs)
            {
                if (hWnd == hWndUnitySub)
                {
                    return;
                }
            }
            if (hWnd == hWndUnityMain)
            {
                return;
            }
            Debug.Log("Unity window lost focus");
            if (allowWinStartMenu && !allowed)
            {
                if (hWnd == FindWindow("Windows.UI.Core.CoreWindow", "시작") || hWnd == FindWindow("Windows.UI.Core.CoreWindow", "Start") ||
                    hWnd == FindWindow("Windows.UI.Core.CoreWindow", "검색") || hWnd == FindWindow("Windows.UI.Core.CoreWindow", "Search"))
                {
                    allowed = true;
                    Debug.Log("WinStartMenu is allowed!");
                }
            }
            if (allowTaskManager && !allowed)
            {
                if (hWnd == FindWindow("TaskManagerWindow", null))
                {
                    allowed = true;
                    Debug.Log("TaskManager is allowed!");
                }
            }
            if (allowTaskSwitching && !allowed)
            {
                if (hWnd == FindWindow("ThumbnailDeviceHelperWnd", null) || hWnd == FindWindow("MultitaskingViewFrame", null) ||
                    hWnd == FindWindow("XamlExplorerHostIslandWindow", "작업 전환") || hWnd == FindWindow("XamlExplorerHostIslandWindow", "Task Switching") ||
                    hWnd == FindWindow("ForegroundStaging", null))
                {
                    allowed = true;
                    Debug.Log("TaskSwitching is allowed!");
                }
            }
            if (allowed)
            {
                keepForeground = false;
                foreach (IntPtr hWndUnitySub in hWndUnitySubs)
                {
                    SetWindowPos(hWndUnitySub, (IntPtr)HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                }
                SetWindowPos(hWndUnityMain, (IntPtr)HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            }
            else
            {
                keepForeground = true;
            }
        }
    }

    private IEnumerator KeepForeground()
    {
        while (enabled)
        {
            if (keepForeground)
            {
                if (hWndUnityMain != IntPtr.Zero && !IsIconic(hWndUnityMain))
                {
                    ForceSetForegroundWindow(hWndUnityMain);
                }
            }
            yield return null;
        }
    }

    private IEnumerator SingleInstance()
    {
        yield return new WaitForSeconds(2f);
        IntPtr hWndPrevious = (IntPtr)PlayerPrefs.GetInt("WindowHandle", 0);
        if (hWndPrevious != IntPtr.Zero && IsWindow(hWndPrevious))
        {
            if (useSingleInstance)
            {
                StringBuilder sb = new StringBuilder();
                sb.Length = GetClassName(hWndPrevious, sb, 256);
                if (string.Compare(sb.ToString(), "UnityWndClass") == 0)
                {
                    ForceSetForegroundWindow(hWndPrevious);
                    Debug.Log(string.Format("Unity window is already existing : {0}", (int)hWndPrevious));
                    yield return null;

                    Application.Quit();
                    yield break;
                }
            }
        }
        if (hWndUnityMain != IntPtr.Zero)
        {
            PlayerPrefs.SetInt("WindowHandle", (int)hWndUnityMain);
            PlayerPrefs.Save();
        }
    }

    public bool MinimizeWindow()
    {
        if (hWndUnityMain != IntPtr.Zero)
        {
            Debug.Log("Minimize Unity window");
            foreach (IntPtr hWndUnitySub in hWndUnitySubs)
            {
                ShowWindowAsync(hWndUnitySub, SW_MINIMIZE);
            }
            return ShowWindowAsync(hWndUnityMain, SW_MINIMIZE);
        }
        return false;
    }

    public bool RestoreWindow()
    {
        if (hWndUnityMain != IntPtr.Zero)
        {
            Debug.Log("Restore Unity window");
            foreach (IntPtr hWndUnitySub in hWndUnitySubs)
            {
                ShowWindowAsync(hWndUnitySub, SW_RESTORE);
            }
            return ShowWindowAsync(hWndUnityMain, SW_RESTORE);
        }
        return false;
    }
#endif
}
