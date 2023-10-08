using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Starpelly
{
    public static class Mathp
    {
        /// <summary>
        /// Rounds float to nearest interval.
        /// </summary>
        public static float Round2Nearest(float a, float interval)
        {
            return a = a - (a % interval);
        }

        /// <summary>
        /// Gets the difference between two floats.
        /// </summary>
        public static float Difference(float num1, float num2)
        {
            float cout;
            cout = Mathf.Max(num2, num1) - Mathf.Min(num1, num2);
            return cout;
        }

        /// <summary>
        /// Returns the closest value in a list compared to value given.
        /// </summary>
        public static float GetClosestInList(List<float> list, float compareTo)
        {
            if (list.Count > 0)
                return list.Aggregate((x, y) => Mathf.Abs(x - compareTo) < Mathf.Abs(y - compareTo) ? x : y);
            else
                return -40;
        }

        /// <summary>
        /// Get the numbers after a decimal.
        /// </summary>
        public static float GetDecimalFromFloat(float number)
        {
            return number % 1; // this is simple as fuck, but i'm dumb and forget this all the time
        }

        /// <summary>
        /// Converts two numbers to a range of 0 - 1
        /// </summary>
        /// <param name="val">The input value.</param>
        /// <param name="min">The min input.</param>
        /// <param name="max">The max input.</param>
        public static float Normalize(float val, float min, float max)
        {
            return (val - min) / (max - min);
        }

        /// <summary>
        /// Converts a normalized value to a normal float.
        /// </summary>
        /// <param name="val">The normalized value.</param>
        /// <param name="min">The min input.</param>
        /// <param name="max">The max input.</param>
        public static float DeNormalize(float val, float min, float max)
        {
            return (val * (max - min) + min);
        }

        /// <summary>
        /// Returns true if a value is within a certain range.
        /// </summary>
        public static bool IsWithin(this float val, float min, float max)
        {
            return val >= min && val <= max;
        }

        /// <summary>
        /// Returns true if a value is within a certain range.
        /// </summary>
        public static bool IsWithin(this Vector2 val, Vector2 min, Vector2 max)
        {
            return val.x.IsWithin(min.x, max.x) && val.y.IsWithin(min.y, max.y);
        }

        /// <summary>
        /// Returns true if value is between two numbers.
        /// </summary>
        public static bool IsBetween<T>(this T item, T start, T end)
        {
            return Comparer<T>.Default.Compare(item, start) >= 0
                && Comparer<T>.Default.Compare(item, end) <= 0;
        }
    }
}
