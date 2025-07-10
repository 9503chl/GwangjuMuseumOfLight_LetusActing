using System;
using System.Collections.Generic;
using System.Text;

public static class HexConverter
{
    public static bool TryGetBytes(string text, out byte[] value)
    {
        bool success = !string.IsNullOrEmpty(text);
        List<byte> result = new List<byte>();
        if (success)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetterOrDigit(text[i]) && char.ToUpper(text[i]) <= 'F')
                {
                    sb.Append(text[i]);
                    if (sb.Length == 2)// || (sb.Length == 1 && i == text.Length - 1))
                    {
                        result.Add(Convert.ToByte(sb.ToString(), 16));
#if NET_2_0 || NET_2_0_SUBSET
                        sb.Length = 0;
#else
                        sb.Clear();
#endif
                    }
                }
                else
                {
                    if (sb.Length == 1 || !char.IsWhiteSpace(text[i]))
                    {
                        success = false;
                    }
#if NET_2_0 || NET_2_0_SUBSET
                    sb.Length = 0;
#else
                    sb.Clear();
#endif
                }
            }
            if (sb.Length == 1)
            {
                success = false;
            }
        }
        value = result.ToArray();
        return success && result.Count > 0;
    }

    public static byte[] GetBytes(string text)
    {
        List<byte> result = new List<byte>();
        if (!string.IsNullOrEmpty(text))
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetterOrDigit(text[i]) && char.ToUpper(text[i]) <= 'F')
                {
                    sb.Append(text[i]);
                    if (sb.Length == 2)// || (sb.Length == 1 && i == text.Length - 1))
                    {
                        result.Add(Convert.ToByte(sb.ToString(), 16));
#if NET_2_0 || NET_2_0_SUBSET
                        sb.Length = 0;
#else
                        sb.Clear();
#endif
                    }
                }
                else
                {
#if NET_2_0 || NET_2_0_SUBSET
                    sb.Length = 0;
#else
                    sb.Clear();
#endif
                }
            }
        }
        return result.ToArray();
    }

    public static string ToString(byte[] bytes, int spacePerChar)
    {
        StringBuilder sb = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++)
        {
            if (sb.Length > 0 && spacePerChar > 0 && (i % spacePerChar) == 0)
            {
                sb.Append(' ');
            }
            sb.Append(string.Format("{0:X2}", bytes[i]));
        }
        return sb.ToString();
    }

    public static string ToString(byte[] bytes)
    {
        return ToString(bytes, 0);
    }
}
