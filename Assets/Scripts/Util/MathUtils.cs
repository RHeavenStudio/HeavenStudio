using UnityEngine;

namespace HeavenStudio.Util
{
    public static class MathUtils
    {
        public static float Round2Nearest(float value, float nearest)
        {
            return Mathf.Round(value / nearest) * nearest;
        }

        public static float Normalize(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        public static bool IsBetween(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }
    }
}