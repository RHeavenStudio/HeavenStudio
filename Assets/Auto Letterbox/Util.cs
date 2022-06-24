using UnityEngine;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System;

namespace AutoLetterbox {

    /* Util.cs
     * 
     * Utility Class with multiple use mathmatical functions
     * 
     * Written by Tom Elliott and Milo Keeble */

    public static class Util {

        public static string GrootWhatAreYouDoing() {
            return "I am Groot.";
        }

        /// <summary>
        /// Returns a value as a positive number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float AsPositive(float value) {
            return Mathf.Abs(value);
        }

        /// <summary>
        /// Returns a value as a negative number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float AsNegative(float value) {
            return -Mathf.Abs(value);
        }

        /// <summary>
        /// Bezier Interpolation between multiple points
        /// </summary>
        public static float BezierCurve(float[] p, float t) {
            if (p.Length > 2) {
                float[] newPoints = new float[p.Length - 1];
                for (int i = 0; i < newPoints.Length; i++) {
                    newPoints[i] = Lerp(p[i], p[i + 1], t);
                }
                return BezierCurve(newPoints, t);

            } else if (p.Length == 2) {
                return Lerp(p[0], p[1], t);

            } else {
                Debug.Log("WARNING: A class attempted to get a Bezier Curve with less than two points!");
                return 0;
            }
        }

        /// <summary>
        /// Bezier Interpolation between multiple points
        /// </summary>
        public static Vector3 BezierCurve(Vector3[] p, float t) {
            if (p.Length > 2) {
                Vector3[] newPoints = new Vector3[p.Length - 1];
                for (int i = 0; i < newPoints.Length; i++) {
                    newPoints[i] = Lerp(p[i], p[i + 1], t);
                }
                return BezierCurve(newPoints, t);

            } else if (p.Length == 2) {
                return Lerp(p[0], p[1], t);

            } else {
                //Debug.Log("WARNING: A class attempted to get a Bezier Curve with less than two points!");
                return Vector3.zero;
            }
        }

        /// <summary>
        /// Takes an existing velocity and a point of collision relative to the center of the object traveling
        /// Returns a new, reflected velocity
        /// (Rough estimate, should be considered inacurrate until a truer calculation is implemented)
        /// </summary>
        /// <param name="originalVelocity"></param>
        /// <param name="normalOfCollision"></param>
        /// <returns></returns>
        public static Vector3 CalculateReflectedVelocity(Vector3 originalVelocity, Vector3 normalOfCollision) {
            Vector3 hitDirection = -normalOfCollision;
            Vector3 newDirection = originalVelocity.normalized;

            newDirection = new Vector3(newDirection.x - hitDirection.x, newDirection.y - hitDirection.y, 0);
            if (newDirection.sqrMagnitude == 0f) {
                newDirection = -originalVelocity.normalized;
            }

            return newDirection;
        }

        /// <summary>
        /// Returns a value no greater than max and no less than min
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float Clamp(float min, float max, float value) {
            value = (value < min) ? min : value;
            value = (value > max) ? max : value;
            return value;
        }

        /// <summary>
        /// Returns a value no greater than max and no less than min
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Clamp(int min, int max, int value) {
            value = (value < min) ? min : value;
            value = (value > max) ? max : value;
            return value;
        }

        /// <summary>
        /// Converts a unity color to a hex string
        /// </summary>
        /// <param name="color">Color to convert, can be Color as well as Color32</param>
        /// <returns>A hex string of the passed in color</returns>
        public static string ColorToHex(Color32 color) {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }

        /// <summary>
        /// Converts an angle in Degrees to a Vector2
        /// </summary>
        /// <returns>The converted Vector2</returns>
        /// <param name="_angle">Angle in Degrees</param>
        public static Vector2 DegreesToVector(float _angle) {
            return new Vector2((float)Math.Cos(_angle * Mathf.Deg2Rad), (float)Math.Sin(_angle * Mathf.Deg2Rad));
        }

        /// <summary>
        /// Returns the float difference between a and b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Difference(float a, float b) {
            return (a > b) ? a - b : b - a;
        }

        /// <summary>
        /// Returns the integer difference between a and b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Difference(int a, int b) {
            return (a > b) ? a - b : b - a;
        }

        /// <summary>
        /// Returns a normalized direction vector pointing at target from origin
        /// </summary>
        public static Vector3 DirectionVector(Vector3 origin, Vector3 target) {
            return (target - origin).normalized;
        }

        /// <summary>
        /// Outputs a custom error message from sender
        /// Use this to keep error messages consistent
        /// </summary>
        public static void Error(object sender, string error) {
            if (sender != null) {
                Debug.Log(sender.GetType().ToString() + ": " + error);
            } else {
                Debug.Log("NULL SENDER: " + error);
            }
        }

        /// <summary>
        /// Returns the code given name of the variable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetMemberName<T>(Expression<Func<T>> expression) {
            MemberExpression body = (MemberExpression)expression.Body;
            return body.Member.Name;
        }

        /// <summary>
        /// Converts a hex string to a unity color
        /// </summary>
        /// <param name="hex">The hex string to convert</param>
        /// <returns>A Color of the passed in Hex String</returns>
        public static Color HexToColor(string hex) {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }

        /// <summary>
        /// Returns the flag number for an int
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public static int IndexToFlags(int _index) {
            switch (_index) {
                case 0: return 1;
                case 1: return 2;
                case 2: return 4;
                case 3: return 8;
                case 4: return 16;
                case 5: return 32;
                case 6: return 64;
                case 7: return 128;
                case 8: return 256;
                case 9: return 512;
                default: return -1;
            }
        }

        /// <summary>
        /// Returns true if the given integer is odd
        /// Returns false if the given integer is even
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsOdd(int value) {
            return value % 2 != 0;
        }

        /// <summary>
        /// Linear Interpolation between two points
        /// </summary>
        public static Vector3 Lerp(Vector3 p1, Vector3 p2, float t) {
            return p1 + (p2 - p1) * t;
        }

        /// <summary>
        /// Linear Interpolation between two points
        /// </summary>
        public static float Lerp(float p1, float p2, float t) {
            return p1 + (p2 - p1) * t;
        }

        /// <summary>
        /// Outputs an error message from sender stating that variableName is null
        /// Use this to keep error messages consistent
        /// </summary>
        public static void NullError(object sender, string variableName, string extraNotes = "") {
            if (sender == null) {
                Debug.Log("NULL SENDER: " + variableName + " is null! " + extraNotes);
            } else {
                Debug.Log(sender.GetType().ToString() + ": " + variableName + " is null! " + extraNotes);
            }

        }

        /// <summary>
        /// Returns value as a percentage of max - min clamped between 0 and 1
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float Percent(float min, float max, float value) {
            if (max - min == 0) {
                //Debug.Log("WARNING: A class attempted to find a percentage of 0!");
                return 0f;
            }
            return Clamp(0f, 1f, (value - min) / (max - min));
        }

        /// <summary>
        /// Returns value as an unclamped percentage of max - min 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float PercentUnclampled(float min, float max, float value) {
            if (max - min == 0) {
                Debug.Log("WARNING: A class attempted to find an unclamped percentage of 0!");
                return 0f;
            }
            return (value - min) / (max - min);
        }

        /// <summary>
        /// Takes 2 rectangles and tests if they intersect
        /// </summary>
        /// <param name="extremeMinA"></param>
        /// <param name="extremeMaxA"></param>
        /// <param name="extremeMinB"></param>
        /// <param name="extremeMaxB"></param>
        /// <returns></returns>
        public static bool RectContainsRect(Vector2 extremeMinA, Vector2 extremeMaxA, Vector2 extremeMinB, Vector2 extremeMaxB) {
            if (extremeMinA.y > extremeMaxB.y || extremeMaxA.y < extremeMinB.y ||
                extremeMinA.x > extremeMaxB.x || extremeMaxA.x < extremeMinB.x) {
                return false;
            } else {
                return true;
            }
        }

        /// <summary>
        /// Returns a vector3 reflected on the X axis
        /// </summary>
        /// <param name="_vector"></param>
        /// <returns></returns>
        public static Vector3 ReflectOnXAxis(Vector3 _vector) {
            _vector.x = -_vector.x;
            return _vector;
        }

        /// <summary>
        /// Returns a vector3 reflected on the X and Y axis
        /// </summary>
        /// <param name="_vector"></param>
        /// <returns></returns>
        public static Vector3 ReflectOnXandYAxis(Vector3 _vector) {
            _vector.x = -_vector.x;
            _vector.y = -_vector.y;
            return _vector;
        }

        /// <summary>
        /// Returns a vector3 reflected on the Y axis
        /// </summary>
        /// <param name="_vector"></param>
        /// <returns></returns>
        public static Vector3 ReflectOnYAxis(Vector3 _vector) {
            _vector.y = -_vector.y;
            return _vector;
        }

        /// <summary>
        /// Converts a Vector2 to an angle in Degrees
        /// </summary>
        /// <returns>The angle in degrees.</returns>
        /// <param name="_vector">A vector2 to convert</param>
        public static float VectorToDegrees(Vector2 _vector) {
            float deg = (float)Mathf.Atan2(_vector.y, _vector.x) * Mathf.Rad2Deg;
            return (deg > 0) ? deg : deg + 360f;
        }

        /// <summary>
        /// Returns 'up' as if the X and Y axis where ground plane (Rather than X and Z)
        /// </summary>
        /// <returns></returns>
        public static Vector3 XYplaneUpDirection() {
            return new Vector3(0, 0, -1);
        }

        /// <summary>
        /// Returns true if the given point is on screen via the main Camera
        /// </summary>
        /// <returns></returns>
        public static bool IsPointOnMainCamera(Vector3 _point) {
            if (Camera.main == null) { return false; }

            Vector3 screenPoint = Camera.main.WorldToScreenPoint(_point);
            if (screenPoint.x < 0 || screenPoint.y > Screen.width || screenPoint.y < 0 || screenPoint.y > Screen.height) {
                return false;
            } else {
                return true;
            }
        }

        //HexToColor & ColorToHex Shamelessly taken from wiki.unity3d.com/index.php?title=HexConverter, Written by mvi?
    }
}