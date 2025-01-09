using System;
using UnityEngine;

[Serializable]
public class DraggableObject : TouchableObject
{
    private bool isDragging;
    private Vector3 dragOffset;

    private Vector3 defPosition;
    private Vector3 defLocalPosition;
    private Quaternion defRotation;
    private Quaternion defLocalRotation;
    private Vector3 defLocalScale;

    private Vector3 minPosition;
    private Vector3 maxPosition;
    private Vector3 minLocalPosition;
    private Vector3 maxLocalPosition;

    private bool restrictRotation;
    private Vector3 minEulerAngles;
    private Vector3 maxEulerAngles;
    private Vector3 rotEulerAngles;
    private Vector3 minLocalEulerAngles;
    private Vector3 maxLocalEulerAngles;
    private Vector3 rotLocalEulerAngles;

    private Vector3 minLocalScale;
    private Vector3 maxLocalScale;

    private const float PI_MULTIPLY_2 = Mathf.PI * 2f;

    private void Awake()
    {
        ResetTranslationRange();
        ResetRotationRange();
        ResetScalingRange();
        Backup();
    }

    public void BeginDrag(Vector3 position)
    {
        Touch();
        dragOffset = transform.position - position;
        isDragging = true;
    }

    public void EndDrag()
    {
        isDragging = false;
    }

    public void Backup()
    {
        defPosition = transform.position;
        defLocalPosition = transform.localPosition;
        defRotation = transform.rotation;
        defLocalRotation = transform.localRotation;
        defLocalScale = transform.localScale;
        rotEulerAngles = Vector3.zero;
        rotLocalEulerAngles = Vector3.zero;
    }

    public void Restore()
    {
        isDragging = false;
        transform.localPosition = defLocalPosition;
        transform.localRotation = defLocalRotation;
        transform.localScale = defLocalScale;
        rotEulerAngles = Vector3.zero;
        rotLocalEulerAngles = Vector3.zero;
    }

    private static Vector3 DeltaAngles(Vector3 from, Vector3 to)
    {
        return new Vector3(Mathf.DeltaAngle(from.x, to.x), Mathf.DeltaAngle(from.y, to.y), Mathf.DeltaAngle(from.z, to.z));
    }

    private static Vector3 ClampVector3(Vector3 value, Vector3 min, Vector3 max)
    {
        return new Vector3(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y), Mathf.Clamp(value.z, min.z, max.z));
    }

    private static Vector3 ClampVector3(Vector3 value, Vector3 min, Vector3 max, Vector3 dir)
    {
        return new Vector3(Mathf.Approximately(dir.x, 0f) ? value.x : Mathf.Clamp(value.x, min.x, max.x), Mathf.Approximately(dir.y, 0f) ? value.y : Mathf.Clamp(value.y, min.y, max.y), Mathf.Approximately(dir.z, 0f) ? value.z : Mathf.Clamp(value.z, min.z, max.z));
    }

    private static float MinOrMax(Vector3 value)
    {
        float min = Mathf.Min(value.x, value.y, value.z);
        float max = Mathf.Max(value.x, value.y, value.z);
        if (Mathf.Abs(min) > Mathf.Abs(max))
        {
            return min;
        }
        return max;
    }

    public void SetTranslationRange(Vector3 min, Vector3 max, bool isRelative = true)
    {
        if (isRelative)
        {
            minPosition = min + defPosition;
            maxPosition = max + defPosition;
            minLocalPosition = min + defLocalPosition;
            maxLocalPosition = max + defLocalPosition;
        }
        else
        {
            minPosition = min;
            maxPosition = max;
            minLocalPosition = min;
            maxLocalPosition = max;
        }
    }

    public void ResetTranslationRange()
    {
        minPosition = Vector3.one * -100000;
        maxPosition = Vector3.one * 100000;
        minLocalPosition = Vector3.one * -100000;
        maxLocalPosition = Vector3.one * 100000;
    }

    public void SetRotationRange(Vector3 min, Vector3 max, bool isRelative = true)
    {
        restrictRotation = true;
        if (isRelative)
        {
            Vector3 defEulerAngles = DeltaAngles(Vector3.zero, defRotation.eulerAngles);
            Vector3 defLocalEulerAngles = DeltaAngles(Vector3.zero, defLocalRotation.eulerAngles);
            minEulerAngles = min + defEulerAngles;
            maxEulerAngles = max + defEulerAngles;
            minLocalEulerAngles = min + defLocalEulerAngles;
            maxLocalEulerAngles = max + defLocalEulerAngles;
        }
        else
        {
            minEulerAngles = min;
            maxEulerAngles = max;
            minLocalEulerAngles = min;
            maxLocalEulerAngles = max;
        }
        rotEulerAngles = DeltaAngles(defRotation.eulerAngles, transform.eulerAngles);
        rotLocalEulerAngles = DeltaAngles(Vector3.zero, transform.localEulerAngles);
    }

    public void ResetRotationRange()
    {
        restrictRotation = false;
        rotEulerAngles = Vector3.zero;
        rotLocalEulerAngles = Vector3.zero;
    }

    public void SetScalingRange(Vector3 min, Vector3 max, bool isRelative = true)
    {
        if (isRelative)
        {
            minLocalScale = Vector3.Scale(min, defLocalScale);
            maxLocalScale = Vector3.Scale(max, defLocalScale);
        }
        else
        {
            minLocalScale = min;
            maxLocalScale = max;
        }
    }

    public void ResetScalingRange()
    {
        minLocalScale = Vector3.one * int.MinValue;
        maxLocalScale = Vector3.one * int.MaxValue;
    }

    public void Translate(Vector3 position, Vector3 direction, bool worldSpace = true)
    {
        if (isDragging)
        {
            Vector3 movement = position - transform.position + dragOffset;
            Vector3 delta = Vector3.Scale(movement, direction);
            if (delta != Vector3.zero)
            {
                if (worldSpace)
                {
                    delta = ClampVector3(delta, minPosition - transform.position, maxPosition - transform.position, direction);
                }
                else
                {
                    delta = ClampVector3(delta, minLocalPosition - transform.localPosition, maxLocalPosition - transform.localPosition, direction);
                }
                transform.Translate(delta, worldSpace ? Space.World : Space.Self);
                dragOffset = transform.position - position;
            }
        }
    }

    public void Rotate(Vector3 position, Vector3 direction, bool worldSpace = true)
    {
        if (isDragging)
        {
            Vector3 movement = position - transform.position + dragOffset;
            Vector3 delta = Vector3.Scale(new Vector3(movement.y, -movement.x, movement.z), direction) * PI_MULTIPLY_2;
            if (delta != Vector3.zero)
            {
                if (restrictRotation)
                {
                    if (worldSpace)
                    {
                        delta = ClampVector3(delta, minEulerAngles - rotEulerAngles, maxEulerAngles - rotEulerAngles, direction);
                    }
                    else
                    {
                        delta = ClampVector3(delta, minLocalEulerAngles - rotLocalEulerAngles, maxLocalEulerAngles - rotLocalEulerAngles, direction);
                    }
                    rotEulerAngles += delta;
                    rotLocalEulerAngles += delta;
                }
                transform.Rotate(delta, worldSpace ? Space.World : Space.Self);
                dragOffset = transform.position - position;
            }
        }
    }

    public void Scale(Vector3 position, Vector3 direction, bool scaleEvenly = true)
    {
        if (isDragging)
        {
            Vector3 movement = position - transform.position + dragOffset;
            Vector3 delta = Vector3.Scale(new Vector3(movement.x, -movement.y, movement.z), direction) / PI_MULTIPLY_2;
            if (scaleEvenly)
            {
                delta = direction * MinOrMax(delta);
            }
            if (delta != Vector3.zero)
            {
                Vector3 localScale = Vector3.Scale(transform.localScale, Vector3.one + delta);
                float minOrMax = MinOrMax(localScale);
                delta /= Mathf.Sqrt(Mathf.Abs(minOrMax)) * Mathf.Sign(minOrMax);
                localScale = Vector3.Scale(transform.localScale, Vector3.one + delta);
                transform.localScale = ClampVector3(localScale, minLocalScale, maxLocalScale, direction);
                dragOffset = transform.position - position;
            }
        }
    }
}
