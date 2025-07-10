using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

[DisallowMultipleComponent]
public class WindowsFirewallManager : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    public bool AllowOnAwake = true;

    private static WindowsFirewallManager instance;
    public static WindowsFirewallManager Instance
    {
        get
        {
            if (instance == null)
            {
#if UNITY_2022_2_OR_NEWER || UNITY_2021_3
                WindowsFirewallManager[] templates = FindObjectsByType<WindowsFirewallManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#elif UNITY_2020_1_OR_NEWER
                WindowsFirewallManager[] templates = FindObjectsOfType<WindowsFirewallManager>(true);
#else
                WindowsFirewallManager[] templates = FindObjectsOfType<WindowsFirewallManager>();
#endif
                if (templates.Length > 0)
                {
                    instance = templates[0];
                    instance.enabled = true;
                    instance.gameObject.SetActive(true);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
        if (!Application.isEditor && AllowOnAwake)
        {
            string programName = Application.productName;
            string programPath = Process.GetCurrentProcess().MainModule.FileName;
            AddAllowRule(programName, programPath);
        }
    }

    public bool RuleExists(string ruleName, string programPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = string.Format("advfirewall firewall show rule name=\"{0}\" dir=in verbose", ruleName),
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using (var process = Process.Start(psi))
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                if (output.IndexOf(programPath, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Failed to check firewall rule : {0}", ex.Message));
        }
        return false;
    }

    public bool RuleExists(string programPath)
    {
        return RuleExists(Path.GetFileNameWithoutExtension(programPath), programPath);
    }

    private bool CreateRule(string ruleName, string action, string programPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = string.Format("advfirewall firewall add rule name=\"{0}\" dir=in action={1} program=\"{2}\" enable=yes", ruleName, action, programPath),
            Verb = "runas",
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        try
        {
            using (var process = Process.Start(psi))
            {
                process.WaitForExit();
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Failed to add firewall rule : {0}", ex.Message));
        }
        return false;
    }

    public bool AddAllowRule(string ruleName, string programPath)
    {
        if (!RuleExists(ruleName, programPath))
        {
            return CreateRule(ruleName, "allow", programPath);
        }
        return false;
    }

    public bool AddAllowRule(string programPath)
    {
        return AddAllowRule(Path.GetFileNameWithoutExtension(programPath), programPath);
    }

    public bool AddBlockRule(string ruleName, string programPath)
    {
        if (!RuleExists(ruleName, programPath))
        {
            return CreateRule(ruleName, "block", programPath);
        }
        return false;
    }

    public bool AddBlockRule(string programPath)
    {
        return AddAllowRule(Path.GetFileNameWithoutExtension(programPath), programPath);
    }

    public bool DeleteRule(string ruleName, string programPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = string.Format("advfirewall firewall delete rule name=\"{0}\" dir=in program=\"{1}\"", ruleName, programPath),
            Verb = "runas",
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        try
        {
            using (var process = Process.Start(psi))
            {
                process.WaitForExit();
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Failed to delete firewall rule : {0}", ex.Message));
        }
        return false;
    }

    public bool DeleteRule(string programPath)
    {
        return DeleteRule(Path.GetFileNameWithoutExtension(programPath), programPath);
    }
#endif
}
