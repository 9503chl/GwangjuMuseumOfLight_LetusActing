using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MoneyChange
{
    public static float money = 0;

    /// <summary>
    /// 숫자 블렌딩해서 변하기
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    public static void ChangeMoney(long from, long to, float duration = 1f)
    {
        float elapsed = 0f;
        long changed = to - from;
        long current = from;
        money = current;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            current = from + (long)((double)changed * (elapsed / duration));
            money = current;
        }
        current = to;
        money = current;
    }
}

