using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigonometrics
{
    public static float GetSine(float angle)
    {
        return Mathf.Sin(angle * Mathf.Deg2Rad);
    }
    public static float GetCosine(float angle)
    {
        return Mathf.Cos(angle * Mathf.Deg2Rad);
    }
    public static float GetTangent(float angle)
    {
        return Mathf.Tan(angle * Mathf.Deg2Rad);
    }

    /// <summary>
    /// 부채꼴 만들기
    /// </summary>
    /// <param name="direction">방향</param>
    /// <param name="angle">각도</param>
    /// <param name="radius">반지름</param>
    /// <returns></returns>
    public static List<Vector2> CreateSectorForm(Vector2 direction, int angle, float radius)
    {
        List <Vector2> tempList = new List<Vector2>();

        direction *= radius;

        tempList.Add(Vector2.zero);

        for (int i = -angle / 2; i <= angle / 2; i++)
        {
            tempList.Add(Quaternion.AngleAxis(i, Vector3.forward) * direction);
        }
        tempList.Add(Vector2.zero);

        return tempList;
    }

    /// <summary>
    /// Vector3에서 각도 구하기
    /// </summary>
    /// <param name="vStart">시작 벡터</param>
    /// <param name="vEnd">목표 벡터</param>
    /// <returns></returns>
    public static float GetAngle(Vector3 vStart, Vector3 vEnd)
    {
        Vector3 v = vEnd - vStart;

        return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
    }

}
