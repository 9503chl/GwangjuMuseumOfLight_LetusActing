using System;

namespace UnityEngine
{
    [Serializable]
    public struct MinMaxValueInt
    {
        public int min;
        public int max;

        public int length
        {
            get { return Mathf.Abs(max - min); }
        }

        public float center
        {
            get { return Mathf.RoundToInt((min + max) / 2f); }
        }

        public MinMaxValueInt(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Contains(int value)
        {
            return (min > max) ? (value >= max && value <= min) : (value >= min && value <= max);
        }

        public int Clamp(int value)
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
