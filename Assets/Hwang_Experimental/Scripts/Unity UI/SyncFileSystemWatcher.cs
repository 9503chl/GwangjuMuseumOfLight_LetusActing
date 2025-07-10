using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SyncFileSystemWatcher : MonoBehaviour
{
    [Tooltip("Target directory to watch for changes in the system\n(excludes changes to target directory itself)")]
    public string TargetDirectory = string.Empty;

    [Tooltip("Include subdirectories and their files recursively")]
    public bool IncludeSubdirs = false;

    [Tooltip("Filters for file name using wildcards, multiple filters are possible, such as \"*.png|*.jpg|*.jpeg\"")]
    public string FileNameFilters = string.Empty;

    [Tooltip("Wait until file is accessible after creating or changing")]
    public bool WaitFileAccess = false;

    [Tooltip("Integrate sequential events (for example, merge 'Created' and 'Deleted' into 'Renamed')")]
    public bool IntegrateEvents = false;

    public bool WatchOnEnable = true;

    public event Action<string> OnDirCreated;
    public event Action<string> OnDirDeleted;
    public event Action<string> OnDirChanged;
    public event Action<string, string> OnDirRenamed;

    public event Action<string> OnFileCreated;
    public event Action<string> OnFileDeleted;
    public event Action<string> OnFileChanged;
    public event Action<string, string> OnFileRenamed;

    private class NotifyChange
    {
        public WatcherChangeTypes ChangeType;
        public DateTime ChangeTime;
        public string OldFullPath;
        public string FullPath;
        public bool IsDirectory;

        public NotifyChange(FileSystemEventArgs e, bool isDirectory = false)
        {
            if (e != null)
            {
                ChangeType = e.ChangeType;
                ChangeTime = DateTime.Now;
                if (e.FullPath != null)
                {
                    FullPath = e.FullPath.Replace('\\', '/');
                }
            }
            IsDirectory = isDirectory;
        }

        public NotifyChange(RenamedEventArgs e, bool isDirectory = false)
        {
            if (e != null)
            {
                ChangeType = e.ChangeType;
                ChangeTime = DateTime.Now;
                if (e.OldFullPath != null)
                {
                    OldFullPath = e.OldFullPath.Replace('\\', '/');
                }
                if (e.FullPath != null)
                {
                    FullPath = e.FullPath.Replace('\\', '/');
                }
            }
            IsDirectory = isDirectory;
        }
    }

    [NonSerialized]
    private int listingDepth = 0;

    [NonSerialized]
    private float listingTime = 0f;

    [NonSerialized]
    private bool listingCompleted = false;

    [NonSerialized]
    private readonly List<string> subDirectories = new List<string>();

    [NonSerialized]
    private readonly Queue<NotifyChange> changes = new Queue<NotifyChange>();

    [NonSerialized]
    private FileSystemWatcher watcher;

    [NonSerialized]
    private Coroutine listingRoutine;

    [NonSerialized]
    private Coroutine checkingRoutine;

    private void OnEnable()
    {
        if (WatchOnEnable)
        {
            Watch();
        }
    }

    private void OnDisable()
    {
        Stop();
    }

    public bool Watch()
    {
        if (watcher != null)
        {
            return false;
        }
        string path = TargetDirectory;
        if (string.IsNullOrEmpty(path))
        {
            path = Path.GetDirectoryName(Application.dataPath);
        }
        else if (!path.Contains(":"))
        {
            path = Path.Combine(Path.GetDirectoryName(Application.dataPath), path);
        }
        path = path.Replace('\\', '/');
        try
        {
            watcher = new FileSystemWatcher(path);
            watcher.IncludeSubdirectories = IncludeSubdirs;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastWrite;
            watcher.EnableRaisingEvents = true;
            watcher.Created += Watcher_Created;
            watcher.Deleted += Watcher_Deleted;
            watcher.Changed += Watcher_Changed;
            watcher.Renamed += Watcher_Renamed;
            watcher.Error += Watcher_Error;
            Debug.Log(string.Format("FileSystemWatcher : Start watching {0}", path));
            StartListDirectories(path);
            checkingRoutine = StartCoroutine(Checking());
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(string.Format("FileSystemWatcher : {0}", ex.Message));
        }
        return false;
    }

    public void Stop()
    {
        if (listingRoutine != null)
        {
            StopCoroutine(listingRoutine);
            listingRoutine = null;
        }
        if (checkingRoutine != null)
        {
            StopCoroutine(checkingRoutine);
            checkingRoutine = null;
        }
        changes.Clear();
        if (watcher != null)
        {
            Debug.Log(string.Format("FileSystemWatcher : Stop watching {0}", watcher.Path));
            watcher.Created -= Watcher_Created;
            watcher.Deleted -= Watcher_Deleted;
            watcher.Changed -= Watcher_Changed;
            watcher.Renamed -= Watcher_Renamed;
            watcher.Error -= Watcher_Error;
            watcher.Dispose();
            watcher = null;
        }
        subDirectories.Clear();
        listingDepth = 0;
        listingCompleted = false;
    }

    private IEnumerator ListDirectories(string path)
    {
        string[] directories = Directory.GetDirectories(path);
        foreach (string directory in directories)
        {
            subDirectories.Add(directory.Replace('\\', '/'));
            if (IncludeSubdirs)
            {
                listingDepth++;
                yield return StartCoroutine(ListDirectories(directory));
                listingDepth--;
            }
        }
        if (listingDepth == 0)
        {
            listingCompleted = true;
            float elapsedTime = Time.realtimeSinceStartup - listingTime;
            Debug.Log(string.Format("FileSystemWatcher : Directory listing completed in {0} seconds", elapsedTime));
        }
        listingRoutine = null;
    }

    private void StartListDirectories(string path)
    {
        subDirectories.Clear();
        listingDepth = 0;
        listingCompleted = false;
        listingTime = Time.realtimeSinceStartup;
        listingRoutine = StartCoroutine(ListDirectories(path));
    }

    private int IndexOfDirectory(string path)
    {
        for (int i = 0; i < subDirectories.Count; i++)
        {
            if (string.Compare(subDirectories[i], path, true) == 0)
            {
                return i;
            }
        }
        return -1;
    }

    private void Watcher_Created(object sender, FileSystemEventArgs e)
    {
        string path = e.FullPath.Replace("\\", "/");
        int index = IndexOfDirectory(path);
        if (index == -1 && Directory.Exists(path))
        {
            subDirectories.Add(path);
            index = subDirectories.Count - 1;
        }
        NotifyChange change = new NotifyChange(e, index != -1);
        if (IntegrateEvents)
        {
            NotifyChange lastChange = changes.Count > 0 ? changes.Peek() : null;
            if (lastChange != null)
            {
                if (lastChange.ChangeType == WatcherChangeTypes.Deleted && (lastChange.ChangeTime - change.ChangeTime).TotalSeconds <= 0.2)
                {
                    lastChange.ChangeType = WatcherChangeTypes.Renamed;
                    lastChange.OldFullPath = lastChange.FullPath;
                    lastChange.FullPath = change.FullPath;
                    lastChange.ChangeTime = change.ChangeTime;
                    return;
                }
            }
        }
        changes.Enqueue(change);
    }

    private void Watcher_Deleted(object sender, FileSystemEventArgs e)
    {
        string path = e.FullPath.Replace("\\", "/");
        int index = IndexOfDirectory(path);
        if (index != -1)
        {
            subDirectories.RemoveAt(index);
        }
        NotifyChange change = new NotifyChange(e, index != -1);
        if (IntegrateEvents)
        {
            NotifyChange lastChange = changes.Count > 0 ? changes.Peek() : null;
            if (lastChange != null)
            {
                if (lastChange.ChangeType == WatcherChangeTypes.Created && (lastChange.ChangeTime - change.ChangeTime).TotalSeconds <= 0.2)
                {
                    lastChange.ChangeType = WatcherChangeTypes.Renamed;
                    lastChange.OldFullPath = change.FullPath;
                    lastChange.ChangeTime = change.ChangeTime;
                    if (index != -1)
                    {
                        subDirectories.Add(change.FullPath);
                    }
                    return;
                }
                else if (lastChange.ChangeType == WatcherChangeTypes.Changed && string.Compare(lastChange.FullPath, change.FullPath) == 0)
                {
                    lastChange.ChangeType = WatcherChangeTypes.Deleted;
                    lastChange.ChangeTime = change.ChangeTime;
                    return;
                }
            }
        }
        changes.Enqueue(change);
    }

    private void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
        string path = e.FullPath.Replace("\\", "/");
        int index = IndexOfDirectory(path);
        if ((index == -1 && !File.Exists(path)) || (index != -1 && !Directory.Exists(path)))
        {
            return;
        }
        NotifyChange change = new NotifyChange(e, index != -1);
        if (IntegrateEvents)
        {
            NotifyChange lastChange = changes.Count > 0 ? changes.Peek() : null;
            if (lastChange != null)
            {
                if (lastChange.ChangeType == WatcherChangeTypes.Changed && string.Compare(lastChange.FullPath, change.FullPath) == 0)
                {
                    lastChange.ChangeTime = change.ChangeTime;
                    return;
                }
                else if (lastChange.ChangeType == WatcherChangeTypes.Deleted && string.Compare(lastChange.FullPath, change.FullPath) == 0)
                {
                    return;
                }
            }
        }
        changes.Enqueue(change);
    }

    private void Watcher_Renamed(object sender, RenamedEventArgs e)
    {
        int index = IndexOfDirectory(e.OldFullPath.Replace("\\", "/"));
        if (index != -1)
        {
            subDirectories[index] = e.FullPath.Replace("\\", "/");
        }
        changes.Enqueue(new NotifyChange(e, index != -1));
    }

    private void Watcher_Error(object sender, ErrorEventArgs e)
    {
        Debug.LogError(string.Format("FileSystemWatcher : {0}", e.GetException()));
    }

    private static bool IsFileLocked(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }
        return false;
    }

    private static bool WildcardMatches(string fileName, string filter)
    {
        string normalized = Regex.Replace(filter, @"\.+$", "");
        int endWeight = 0;
        if (normalized.Length != filter.Length)
        {
            int lastNonWildcard = normalized.Length - 1;
            for (; lastNonWildcard >= 0; lastNonWildcard--)
            {
                char c = normalized[lastNonWildcard];
                if (c == '*')
                {
                    endWeight += short.MaxValue;
                }
                else if (c == '?')
                {
                    endWeight += 1;
                }
                else
                {
                    break;
                }
            }
            if (endWeight > 0)
            {
                normalized = normalized.Substring(0, lastNonWildcard + 1);
            }
        }
        bool endsWithWildcardDot = endWeight > 0;
        bool endsWithDotWildcardDot = endsWithWildcardDot && normalized.EndsWith(".");
        if (endsWithDotWildcardDot)
        {
            normalized = normalized.Substring(0, normalized.Length - 1);
        }
        normalized = Regex.Replace(normalized, @"(?!^)(\.\*)+$", @".*");
        string escaped = Regex.Escape(normalized);
        string head, tail;
        if (endsWithDotWildcardDot)
        {
            head = "^" + escaped;
            tail = @"(\.[^.]{0," + endWeight + "})?$";
        }
        else if (endsWithWildcardDot)
        {
            head = "^" + escaped;
            tail = "[^.]{0," + endWeight + "}$";
        }
        else
        {
            head = "^" + escaped;
            tail = "$";
        }
        if (head.EndsWith(@"\.\*") && head.Length > 5)
        {
            head = head.Substring(0, head.Length - 4);
            tail = @"(\..*)?" + tail;
        }
        string regex = head.Replace(@"\*", ".*").Replace(@"\?", "[^.]?") + tail;
        return Regex.IsMatch(fileName, regex, RegexOptions.IgnoreCase);
    }

    private bool IsFileExtension(string path)
    {
        string fileName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(FileNameFilters))
        {
            string[] parts = FileNameFilters.Split('|');
            foreach (string part in parts)
            {
                if (WildcardMatches(fileName, part))
                {
                    return true;
                }
            }
            return false;
        }
        return true;
    }

    private IEnumerator Notifying(NotifyChange change)
    {
        while (checkingRoutine != null && change != null)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (change == null || change.IsDirectory || !WaitFileAccess || !IsFileLocked(change.FullPath))
            {
                break;
            }
        }
        yield return null;
        if (checkingRoutine != null && change != null)
        {
            if (change.IsDirectory ||
                (change.ChangeType != WatcherChangeTypes.Renamed && IsFileExtension(change.FullPath)) ||
                (change.ChangeType == WatcherChangeTypes.Renamed && IsFileExtension(change.OldFullPath)))
            {
                CallEvents(change);
            }
        }
    }

    private IEnumerator Checking()
    {
        NotifyChange change;
        while (!listingCompleted)
        {
            yield return null;
        }
        while (isActiveAndEnabled)
        {
            if (changes.Count > 0)
            {
                change = changes.Dequeue();
                yield return new WaitForSecondsRealtime(0.1f);
                StartCoroutine(Notifying(change));
            }
            yield return null;
        }
        checkingRoutine = null;
    }

    private void CallEvents(NotifyChange change)
    {
        if (change != null)
        {
#if UNITY_EDITOR
            if (change.ChangeType == WatcherChangeTypes.Renamed)
            {
                Debug.Log(string.Format("FileSystemWatcher : [{0}] {1} to {2} ({3})", change.ChangeType, change.OldFullPath, change.FullPath, change.IsDirectory ? "Directory" : "File"));
            }
            else
            {
                Debug.Log(string.Format("FileSystemWatcher : [{0}] {1} ({2})", change.ChangeType, change.FullPath, change.IsDirectory ? "Directory" : "File"));
            }
#endif
            switch (change.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    if (change.IsDirectory)
                    {
                        if (OnDirCreated != null)
                        {
                            OnDirCreated(change.FullPath);
                        }
                    }
                    else
                    {
                        if (OnFileCreated != null)
                        {
                            OnFileCreated(change.FullPath);
                        }
                    }
                    break;
                case WatcherChangeTypes.Deleted:
                    if (change.IsDirectory)
                    {
                        if (OnDirDeleted != null)
                        {
                            OnDirDeleted(change.FullPath);
                        }
                    }
                    else
                    {
                        if (OnFileDeleted != null)
                        {
                            OnFileDeleted(change.FullPath);
                        }
                    }
                    break;
                case WatcherChangeTypes.Changed:
                    if (change.IsDirectory)
                    {
                        if (OnDirChanged != null)
                        {
                            OnDirChanged(change.FullPath);
                        }
                    }
                    else
                    {
                        if (OnFileChanged != null)
                        {
                            OnFileChanged(change.FullPath);
                        }
                    }
                    break;
                case WatcherChangeTypes.Renamed:
                    if (change.IsDirectory)
                    {
                        if (OnDirRenamed != null)
                        {
                            OnDirRenamed(change.OldFullPath, change.FullPath);
                        }
                    }
                    else
                    {
                        if (OnFileRenamed != null)
                        {
                            OnFileRenamed(change.OldFullPath, change.FullPath);
                        }
                    }
                    break;
            }
        }
    }
}
