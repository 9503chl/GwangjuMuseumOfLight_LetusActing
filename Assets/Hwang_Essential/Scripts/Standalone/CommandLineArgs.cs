using System;

public class CommandLineArgs
{
    public static string GetString(string key, string defValue)
    {
        string value = GetString(key);
        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }
        return defValue;
    }

    public static string GetString(string key)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Compare(args[i], key, true) == 0)
            {
                if (args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
        }
        return null;
    }

    public static bool GetBool(string key, bool defValue)
    {
        string value = GetString(key);
        if (!string.IsNullOrEmpty(value))
        {
            try
            {
                return Convert.ToBoolean(value);
            }
            catch (Exception)
            {
            }
        }
        return defValue;
    }

    public static bool GetBool(string key)
    {
        return GetBool(key, false);
    }

    public static int GetInt(string key, int defValue)
    {
        string value = GetString(key);
        if (!string.IsNullOrEmpty(value))
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception)
            {
            }
        }
        return defValue;
    }

    public static int GetInt(string key)
    {
        return GetInt(key, 0);
    }

    public static float GetFloat(string key, float defValue)
    {
        string value = GetString(key);
        if (!string.IsNullOrEmpty(value))
        {
            try
            {
                return Convert.ToSingle(value);
            }
            catch (Exception)
            {
            }
        }
        return defValue;
    }

    public static float GetFloat(string key)
    {
        return GetFloat(key, 0f);
    }

    public static bool KeyExists(string key)
    {
        return !string.IsNullOrEmpty(GetString(key));
    }
}
