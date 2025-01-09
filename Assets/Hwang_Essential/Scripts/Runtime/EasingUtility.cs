using System;

namespace UnityEngine
{
    public static class EasingUtility
    {
        public static float AnimationCurve(float t, AnimationCurve a, float d)
        {
            if (a.length == 0 || d == 0f)
                return a.Evaluate(0f);

            t /= (d / a.keys[a.length - 1].time);

            return a.Evaluate(t);
        }

        public static float Linear(float t, float b, float c, float d)
        {
            return c * t / d + b;
        }

        public static float BackEaseIn(float t, float b, float c, float d)
        {
            float s = 1.70158f;

            t /= d;

            return c * t * t * ((s + 1) * t - s) + b;
        }

        public static float BackEaseOut(float t, float b, float c, float d)
        {
            float s = 1.70158f;

            t /= d;
            t -= 1f;

            return c * (t * t * ((s + 1) * t + s) + 1) + b;
        }

        public static float BackEaseInOut(float t, float b, float c, float d)
        {
            float s = 1.70158f * 1.525f;

            t /= (d / 2);

            if (t < 1f)
                return c / 2 * (t * t * (((s) + 1) * t - s)) + b;

            t -= 2;

            return c / 2 * ((t) * t * (((s) + 1) * t + s) + 2) + b;
        }

        public static float BounceEaseIn(float t, float b, float c, float d)
        {
            return c - BounceEaseOut(d - t, 0f, c, d) + b;
        }

        public static float BounceEaseOut(float t, float b, float c, float d)
        {
            t /= d;

            if (t < (1f / 2.75f))
            {
                return c * (7.5625f * t * t) + b;
            }
            else if (t < (2 / 2.75f))
            {
                t -= (1.5f / 2.75f);

                return c * (7.5625f * t * t + 0.75f) + b;
            }
            else if (t < (2.5f / 2.75f))
            {
                t -= (2.25f / 2.75f);

                return c * (7.5625f * t * t + 0.9375f) + b;
            }

            t -= (2.625f / 2.75f);

            return c * (7.5625f * t * t + 0.984375f) + b;
        }

        public static float BounceEaseInOut(float t, float b, float c, float d)
        {
            if (t < (d / 2))
                return BounceEaseIn(t * 2, 0f, c, d) / 2 + b;

            return BounceEaseOut(t * 2 - d, 0f, c, d) / 2 + c / 2 + b;
        }

        public static float CircleEaseIn(float t, float b, float c, float d)
        {
            t = (t > d) ? 1f : t / d;

            return -c * (Mathf.Sqrt(1 - t * t) - 1) + b;
        }

        public static float CircleEaseOut(float t, float b, float c, float d)
        {
            t = (t > d) ? 1f : t / d;
            t--;

            return c * Mathf.Sqrt(1 - t * t) + b;
        }

        public static float CircleEaseInOut(float t, float b, float c, float d)
        {
            t = (t > d) ? 2f : t / (d / 2);

            if (t < 1f)
                return -c / 2 * (Mathf.Sqrt(1 - t * t) - 1) + b;

            t -= 2;

            return c / 2 * (Mathf.Sqrt(1 - t * t) + 1) + b;
        }

        public static float CubicEaseIn(float t, float b, float c, float d)
        {
            t /= d;

            return c * t * t * t + b;
        }

        public static float CubicEaseOut(float t, float b, float c, float d)
        {
            t = t / d - 1;

            return c * (t * t * t + 1) + b;
        }

        public static float CubicEaseInOut(float t, float b, float c, float d)
        {
            t /= (d / 2);

            if (t < 1f)
                return c / 2 * t * t * t + b;

            t -= 2;

            return c / 2 * (t * t * t + 2) + b;
        }

        public static float ElasticEaseIn(float t, float b, float c, float d)
        {
            if (t == 0f)
                return b;

            t /= d;

            if (t == 1f)
                return b + c;

            float p = d * 0.3f;
            float e = c;
            float s = p / 4;
            float f = e * Mathf.Pow(2, 10 * --t);

            return -(f * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p)) + b;
        }

        public static float ElasticEaseOut(float t, float b, float c, float d)
        {
            if (t == 0f)
                return b;

            t /= d;

            if (t == 1f)
                return b + c;

            float p = d * 0.3f;
            float e = c;
            float s = p / 4;
            float f = e * Mathf.Pow(2, -10 * t);

            return f * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + c + b;
        }

        public static float ElasticEaseInOut(float t, float b, float c, float d)
        {
            if (t == 0f)
                return b;

            t /= (d / 2);

            if (t == 2f)
                return b + c;

            float p = d * (0.3f * 1.5f);
            float e = c;
            float s = p / 4f;
            float f = 0f;

            if (t < 1f)
            {
                f = e * Mathf.Pow(2, 10 * --t);

                return -(f * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p)) / 2 + b;
            }

            f = e * Mathf.Pow(2, -10 * --t);

            return f * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) / 2 + c + b;
        }

        public static float ExpoEaseIn(float t, float b, float c, float d)
        {
            if (t == 0f)
                return b;

            return c * Mathf.Pow(2, 10 * (t / d - 1)) + b;
        }

        public static float ExpoEaseOut(float t, float b, float c, float d)
        {
            if (t == d)
                return b + c;

            return c * (-Mathf.Pow(2, -10 * t / d) + 1) + b;
        }

        public static float ExpoEaseInOut(float t, float b, float c, float d)
        {
            if (t == 0f)
                return b;

            if (t == d)
                return b + c;

            t /= (d / 2);

            if (t < 1f)
                return c / 2 * Mathf.Pow(2, 10 * (t - 1)) + b;

            t--;

            return c / 2 * (-Mathf.Pow(2, -10 * t) + 2) + b;
        }

        public static float QuadEaseIn(float t, float b, float c, float d)
        {
            t = (t > d) ? 1f : t / d;

            return c * t * t + b;
        }

        public static float QuadEaseOut(float t, float b, float c, float d)
        {
            t = (t > d) ? 1f : t / d;

            return -c * t * (t - 2) + b;
        }

        public static float QuadEaseInOut(float t, float b, float c, float d)
        {
            t = (t > d) ? 2f : t / (d / 2);

            if (t < 1f)
                return c / 2 * t * t + b;

            t--;

            return -c / 2 * (t * (t - 2) - 1) + b;
        }

        public static float QuartEaseIn(float t, float b, float c, float d)
        {
            t = (t > d) ? 1f : t / d;

            return c * t * t * t * t + b;
        }

        public static float QuartEaseOut(float t, float b, float c, float d)
        {
            t = (t > d) ? 1f : t / d;
            t--;

            return -c * (t * t * t * t - 1) + b;
        }

        public static float QuartEaseInOut(float t, float b, float c, float d)
        {
            t = (t > d) ? 2f : t / (d / 2);

            if (t < 1f)
                return c / 2 * t * t * t * t + b;

            t -= 2;

            return -c / 2 * (t * t * t * t - 2) + b;
        }

        public static float QuintEaseIn(float t, float b, float c, float d)
        {
            t = (t > d) ? 1f : t / d;

            return c * t * t * t * t * t + b;
        }

        public static float QuintEaseOut(float t, float b, float c, float d)
        {
            t = (t > d) ? 1f : t / d;
            t--;

            return c * (t * t * t * t * t + 1) + b;
        }

        public static float QuintEaseInOut(float t, float b, float c, float d)
        {
            t = (t > d) ? 2f : t / (d / 2);

            if (t < 1f)
                return c / 2 * t * t * t * t * t + b;

            t -= 2;

            return c / 2 * (t * t * t * t * t + 2) + b;
        }

        public static float SineEaseIn(float t, float b, float c, float d)
        {
            return -c * Mathf.Cos(t / d * (Mathf.PI / 2)) + c + b;
        }

        public static float SineEaseOut(float t, float b, float c, float d)
        {
            return c * Mathf.Sin(t / d * (Mathf.PI / 2)) + b;
        }

        public static float SineEaseInOut(float t, float b, float c, float d)
        {
            return -c / 2 * (Mathf.Cos(Mathf.PI * t / d) - 1) + b;
        }
    }
}
