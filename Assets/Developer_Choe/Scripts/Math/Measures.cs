using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Measures 
{
    public static int Count; 

    /// <summary>
    /// ��� ���ϱ�
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static int GetMeasureCnt(int target)
    {
        Count = 0;

        for (int i = 1; i <= target / 2; i++)
        {
            if (target % i == 0) Count += 2;
        }

        return Count;
    }
}
