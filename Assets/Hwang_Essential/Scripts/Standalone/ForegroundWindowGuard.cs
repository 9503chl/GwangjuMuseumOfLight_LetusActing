using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

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
    private bool checkInstance = true;

    [NonSerialized]
    private IntPtr hWndPrevious = IntPtr.Zero;

    [NonSerialized]
    private List<IntPtr> hWndDisplays = new List<IntPtr>();

    [NonSerialized]
    private bool delayedTopMost = false;

    [NonSerialized]
    private bool keepForeground = false;

    private void OnEnable()
    {
        if (!Application.isEditor)
        {
            if (hWndDisplays.Count == 0)
            {
                IntPtr hWnd = GetForegroundWindow();
                IntPtr hWndDisplay = FindWindow("UnityWndClass", Application.productName);
                if (hWndDisplay != hWnd)
                {
                    Debug.Log(string.Format("Unity window found : {0}", (int)hWndDisplay));
                    SetForegroundWindow(hWndDisplay);
                }
            }
            StartCoroutine(Foreground());
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
            if (!hWndDisplays.Contains(hWnd))
            {
                if (hWndDisplays.Count == 0)
                {
                    ForceSetForegroundWindow(hWnd);
                    if (checkInstance)
                    {
                        checkInstance = false;
                        StartCoroutine(SingleInstance());
                    }
                }
                else
                {
                    delayedTopMost = true;
                }
                hWndDisplays.Add(hWnd);
            }
            foreach (IntPtr hWndDisplay in hWndDisplays)
            {
                SetWindowPos(hWndDisplay, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            }
            Debug.Log(string.Format("Unity window got focus : {0}", (int)hWnd));
        }
        else if (hWndDisplays.Count > 0)
        {
            IntPtr hWnd = GetForegroundWindow();
            while (hWnd == IntPtr.Zero)
            {
                hWnd = GetForegroundWindow();
            }
            foreach (IntPtr hWndDisplay in hWndDisplays)
            {
                if (hWnd == hWndDisplay)
                {
                    keepForeground = true;
                    return;
                }
            }
            Debug.Log("Unity window lost focus.");
            bool allowed = false;
            if (allowWinStartMenu)
            {
                if (hWnd == FindWindow("Windows.UI.Core.CoreWindow", "시작") || hWnd == FindWindow("Windows.UI.Core.CoreWindow", "Start") ||
                    hWnd == FindWindow("Windows.UI.Core.CoreWindow", "검색") || hWnd == FindWindow("Windows.UI.Core.CoreWindow", "Search"))
                {
                    allowed = true;
                    Debug.Log("WinStartMenu is allowed!");
                }
            }
            if (allowTaskManager && hWnd == FindWindow("TaskManagerWindow", null))
            {
                allowed = true;
                Debug.Log("TaskManager is allowed!");
            }
            if (allowTaskSwitching && hWnd == FindWindow("ForegroundStaging", null))
            {
                allowed = true;
                Debug.Log("TaskSwitching is allowed!");
            }
            if (allowed)
            {
                keepForeground = false;
                foreach (IntPtr hWndDisplay in hWndDisplays)
                {
                    SetWindowPos(hWndDisplay, (IntPtr)HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                }
            }
            else
            {
                keepForeground = true;
            }
        }
    }

    private IEnumerator Foreground()
    {
        while (enabled)
        {
            if (hWndDisplays.Count > 0 && !IsIconic(hWndDisplays[0]))
            {
                if (delayedTopMost)
                {
                    delayedTopMost = false;
                    yield return new WaitForSeconds(1f);
                    SetWindowPos(hWndDisplays[0], (IntPtr)HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                }
                if (keepForeground)
                {
                    ForceSetForegroundWindow(hWndDisplays[0]);
                }
            }
            yield return null;
        }
    }

    private IEnumerator SingleInstance()
    {
        yield return new WaitForSeconds(2f);

        hWndPrevious = (IntPtr)PlayerPrefs.GetInt("WindowHandle", 0);
        if (hWndPrevious != IntPtr.Zero && IsWindow(hWndPrevious))
        {
            if (useSingleInstance)
            {
                StringBuilder sb = new StringBuilder();
                GetClassName(hWndPrevious, sb, 256);
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
        if (hWndDisplays.Count > 0)
        {
            PlayerPrefs.SetInt("WindowHandle", (int)hWndDisplays[0]);
            PlayerPrefs.Save();
        }
    }

    public bool MinimizeWindow()
    {
        if (hWndDisplays.Count > 0 && !IsIconic(hWndDisplays[0]))
        {
            Debug.Log("Minimize Unity window.");
            return ShowWindowAsync(hWndDisplays[0], SW_MINIMIZE);
        }
        return false;
    }

    public bool RestoreWindow()
    {
        if (hWndDisplays.Count > 0 && IsIconic(hWndDisplays[0]))
        {
            Debug.Log("Restore Unity window.");
            return ShowWindowAsync(hWndDisplays[0], SW_RESTORE);
        }
        return false;
    }
#endif
}
