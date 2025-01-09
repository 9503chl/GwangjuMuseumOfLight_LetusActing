using System;

namespace UnityEngine
{
    [Serializable]
    public struct MinMaxValue
    {
        public float min;
        public float max;

        public float length
        {
            get { return Mathf.Abs(max - min); }
        }

        public float center
        {
            get { return (min + max) / 2f; }
        }

        public MinMaxValue(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Contains(float value)
        {
            return (min > max) ? (value >= max && value <= min) : (value >= min && value <= max);
        }

        public float Clamp(float value)
        {
            return (min > max) ? Mathf.Clamp(value, max, min) : Mathf.Clamp(value, min, max);
        }

        public float Lerp(float t)
        {
            return Mathf.Lerp(min, max, t);
        }

        public float Magnitude(float value)
        {
            return Mathf.Lerp(0f, 1f, Mathf.Abs(min - value) / Mathf.Abs(max - min));
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", min, max);
        }
    }
}
