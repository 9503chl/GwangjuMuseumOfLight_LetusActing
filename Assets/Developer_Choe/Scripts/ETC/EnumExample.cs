using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Enum
{
    A,
    B
}

public static class EnumExtension //확장자, 이렇게하면 지역변수.GetEnumName 사용가능.
{
    public static string GetEnumName(this Enum @enum)
    {
        switch (@enum)//default로 오류를 막자.
        {
            case Enum.A: return "";
            case Enum.B: return "";
            default: return string.Empty;
        }
    }
}