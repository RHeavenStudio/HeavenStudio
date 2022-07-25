using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace HeavenStudio.Util
{
    public static class BitwiseUtils
    {
        /// <summary>
        /// Returns the value of the lowest set bit in the given integer.
        /// </summary>
        /// <param name="num">The integer to check.</param>
        public static int FirstSetBit(int num)
        {
            return num & (-num);
        }

        /// <summary>
        /// Returns true if the wanted bit is set in the given integer.
        /// </summary>
        /// <param name="num">The integer to check.</param>
        /// <param name="want">The bit(s) to check for.</param>
        public static bool WantCurrent(int num, int want)
        {
            if (want <= 0) return false;
            return (num & want) == want;
        }

        /// <summary>
        /// Returns true if the wanted bit is set in the first integer, and not in the second.
        /// </summary>
        /// <param name="num1">The first integer to check.</param>
        /// <param name="num2">The second integer to check.</param>
        /// <param name="want">The bit(s) to check for.</param>
        public static bool WantCurrentAndNotLast(int num1, int num2, int want)
        {
            if (want <= 0) return false;
            return ((num1 & want) == want) && ((num2 & want) != want);
        }

        /// <summary>
        /// Returns true if the wanted bit is not set in the first integer, but set in the second.
        /// </summary>
        /// <param name="num1">The first integer to check.</param>
        /// <param name="num2">The second integer to check.</param>
        /// <param name="want">The bit(s) to check for.</param>
        public static bool WantNotCurrentAndLast(int num1, int num2, int want)
        {
            if (want <= 0) return false;
            return ((num1 & want) != want) && ((num2 & want) == want);
        }

        public static Color IntToRgb(int value)
        {
            var red =   ( value >>  16 ) & 255;
            var green = ( value >>  8  ) & 255;
            var blue =  ( value >>  0  ) & 255;
            return new Color(red/255f, green/255f, blue/255f); 
        }
    }
}