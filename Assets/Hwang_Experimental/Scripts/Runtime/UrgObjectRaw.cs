using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UrgObjectRaw
{
    public int MedianStep { get { return stepList[stepList.Count / 2]; } }
    public int MedianDist { get { return distList[distList.Count / 2]; } }
    public Vector2 Position { get; private set; }
    public float Degree { get; private set; }

    public float Size
    {
        get
        {
            Vector2 pointA = CalculatePosition(cachedDirections[stepList[0]], distList[0]);
            Vector2 pointB = CalculatePosition(cachedDirections[stepList[stepList.Count - 1]], distList[distList.Count - 1]);
            return Vector2.Distance(pointA, pointB);
        }
    }

    public readonly List<int> stepList = new List<int>();
    public readonly List<int> distList = new List<int>();

    private readonly Vector2[] cachedDirections;

    public UrgObjectRaw(Vector2[] directions, int step, int dist)
    {
        cachedDirections = directions;
        stepList.Add(step);
        distList.Add(dist);
    }

    public void Add(int step, int dist)
    {
        stepList.Add(step);
        distList.Add(dist);
    }

    private Vector2 CalculatePosition(Vector2 dir, int dist)
    {
        float angle = -Vector2.SignedAngle(dir, Vector2.right) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
    }

    public void UpdatePosition()
    {
        Position = CalculatePosition(cachedDirections[MedianStep], MedianDist);
    }

    public void UpdateDegree(float rotateDegree, bool isFlipped)
    {
        Degree = Mathf.RoundToInt(Mathf.DeltaAngle(rotateDegree, Vector2.SignedAngle(cachedDirections[MedianStep], Vector2.up)) * (isFlipped ? 1f : -1f) * 1000f) / 1000f;
    }

    public override string ToString()
    {
        return string.Format("[{0} to {1}] {2} ({3:F2}, {4:F2})", stepList[0], stepList[stepList.Count - 1], MedianDist, Position.x, Position.y);
    }
}
