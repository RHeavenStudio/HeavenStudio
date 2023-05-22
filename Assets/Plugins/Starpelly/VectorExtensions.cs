using UnityEngine;

namespace Starpelly
{
    public static class VectorExtensions
    {
        public static Vector2 ModifyX(this Vector2 v, float newX)
        {
            return new Vector2(newX, v.y);
        }
        
        public static Vector2 ModifyY(this Vector2 v, float newY)
        {
            return new Vector2(v.x, newY);
        }
        
        public static Vector3 ModifyX(this Vector3 v, float newX)
        {
            return new Vector3(newX, v.y, v.z);
        }
        
        public static Vector3 ModifyY(this Vector3 v, float newY)
        {
            return new Vector3(v.x, newY, v.z);
        }
        
        public static Vector3 ModifyZ(this Vector3 v, float newZ)
        {
            return new Vector3(v.x, v.y, newZ);
        }
    }
}
