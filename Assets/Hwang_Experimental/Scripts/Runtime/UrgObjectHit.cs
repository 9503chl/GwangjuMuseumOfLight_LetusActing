using System;
using UnityEngine;

[Serializable]
public class UrgObjectHit
{
    public int PointerId { get; private set; }
    public int Step { get; private set; }
    public float Degree { get; private set; }
    public int Distance { get; private set; }
    public float Size { get; private set; }
    public Vector2 Position { get; private set; }
    public Vector2 Delta { get; private set; }
    public Vector2 ScreenPosition { get; private set; }
    public Vector2 ScreenDelta { get; private set; }
    public float Age { get { return Time.time - birthTime; } }
    public bool Pressed { get; private set; }
    public bool Expired { get; private set; }
    public bool IsNoise { get; set; }

    private int startStep = 0;
    private float birthTime = 0f;
    private Vector2 velocity = Vector2.zero;
    private Vector2 oldPosition = Vector2.zero;
    private Vector2 oldScreenPosition = Vector2.zero;
    private int keepingCount = 0;
    private int missingCount = 0;

    private const int KEEPING_LIMIT = 8;
    private const int MISSING_LIMIT = 4;
    private const int MIN_POINTER_ID = 1000;

    private static int lastPointerId = MIN_POINTER_ID;

    public UrgObjectHit(UrgObjectRaw rawObj, int startStep)
    {
        PointerId = lastPointerId < MIN_POINTER_ID ? MIN_POINTER_ID : lastPointerId++;
        this.startStep = startStep;
        birthTime = Time.time;
        if (rawObj != null)
        {
            Position = rawObj.Position;
            UpdateObject(rawObj, 0f);
        }
        keepingCount = 0;
    }

    public bool UpdateObject(UrgObjectRaw rawObj, float smoothTime, int pressDelay = KEEPING_LIMIT, int updateDelay = 0)
    {
        if (rawObj != null)
        {
            keepingCount++;
            missingCount = 0;
            oldPosition = Position;
            Step = startStep + rawObj.MedianStep;
            Degree = rawObj.Degree;
            Distance = rawObj.MedianDist;
            Size = rawObj.Size;
            if (smoothTime < 0.001f)
            {
                Position = rawObj.Position;
            }
            else
            {
#if UNITY_2018_1_OR_NEWER
                Position = Vector2.SmoothDamp(Position, rawObj.Position, ref velocity, Mathf.Clamp01(smoothTime) / 10f);
#else
                Position = Vector2.SmoothDamp(Position, rawObj.Position, ref velocity, Mathf.Clamp01(smoothTime) / 10f, float.PositiveInfinity, Time.deltaTime);
#endif
            }
            Delta = Position - oldPosition;
            if (Pressed && ((keepingCount + pressDelay) % (updateDelay + 1) == 0))
            {
                return true;
            }
            if (keepingCount > pressDelay)
            {
                Pressed = true;
            }
        }
        return false;
    }

    public void UpdateObject(int releaseDelay = 0, int removeDelay = MISSING_LIMIT)
    {
        missingCount++;
        if (Pressed && missingCount > releaseDelay)
        {
            keepingCount = 0;
            Pressed = false;
        }
        if (missingCount > releaseDelay + removeDelay)
        {
            keepingCount = 0;
            Pressed = false;
            Expired = true;
        }
    }

    public void UpdateScreenPosition(Rect detectionArea, Rect screenArea)
    {
        oldScreenPosition = ScreenPosition;
#if UNITY_2018_1_OR_NEWER
        ScreenPosition = screenArea.position + screenArea.size * (Position - detectionArea.position) / detectionArea.size;
#else
        ScreenPosition = screenArea.position + new Vector2(screenArea.width * (Position.x - detectionArea.x) / detectionArea.width, screenArea.height * (Position.y - detectionArea.y) / detectionArea.height);
#endif
        ScreenDelta = ScreenPosition - oldScreenPosition;
    }

    public override string ToString()
    {
        return string.Format("[{0}] {1} ({2:F2}, {3:F2})", Step, Distance, Position.x, Position.y);
    }
}
