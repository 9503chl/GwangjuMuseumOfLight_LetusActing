using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezialCurve
{
    private static Transform _target;
    public static void CalculateBezialCurve(Transform t1, Transform t2, Transform t3, float time)
    {
        Vector3 LerpPoint = Vector3.Lerp(t1.position, t2.position, time);
        Vector3 LerpPoint2 = Vector3.Lerp(t2.position, t3.position, time);

        _target.position = Vector3.Lerp(LerpPoint, LerpPoint2, time);

        CalculateBezialCurve(t1.position, t2.position, t3.position, time);
    }

    public static Vector3 CalculateBezialCurve(Vector3 v1, Vector3 v2, Vector3 v3, float time)
    {
        Vector3 LerpPoint = Vector3.Lerp(v1, v2, time);
        Vector3 LerpPoint2 = Vector3.Lerp(v2, v3, time);

        _target.position = Vector3.Lerp(LerpPoint, LerpPoint2, time);

        return _target.position;
    }
}
