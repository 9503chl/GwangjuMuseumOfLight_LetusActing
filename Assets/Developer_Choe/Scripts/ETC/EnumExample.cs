using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Enum
{
    A,
    B
}

public static class EnumExtension //Ȯ����, �̷����ϸ� ��������.GetEnumName ��밡��.
{
    public static string GetEnumName(this Enum @enum)
    {
        switch (@enum)//default�� ������ ����.
        {
            case Enum.A: return "";
            case Enum.B: return "";
            default: return string.Empty;
        }
    }
}