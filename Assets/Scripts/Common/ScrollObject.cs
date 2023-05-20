using UnityEngine;
using System;

namespace HeavenStudio.Common
{
    public class ScrollObject : MonoBehaviour
    {
        public bool AutoScroll;
        public float XSpeed = 1.0f;
        public float YSpeed = 1.0f;
        public Vector2 NegativeBounds;
        public Vector2 PositiveBounds;
        [System.NonSerialized] public float SpeedMod = 5;

        public void LateUpdate()
        {
            var pos = gameObject.transform.position;
            float _x = Time.deltaTime*SpeedMod*XSpeed;
            float _y = Time.deltaTime*SpeedMod*YSpeed;
            if (AutoScroll && Conductor.instance.isPlaying) {
                gameObject.transform.position += new Vector3(_x, _y);

                if (XSpeed > 0 && pos.x >= PositiveBounds.x) {
                    SetPos(NegativeBounds.x, pos.y);
                }
                if (XSpeed < 0 && pos.x <= NegativeBounds.x) {
                    SetPos(PositiveBounds.x, pos.y);
                }

                if (YSpeed > 0 && pos.y >= PositiveBounds.y) {
                    SetPos(pos.x, NegativeBounds.y);
                }
                if (YSpeed < 0 && pos.y <= NegativeBounds.y) {
                    SetPos(pos.x, PositiveBounds.y);
                }
            }
        }

        public void SetPos(float x, float y)
        {
            gameObject.transform.position = new Vector3(x, y);
        }
    }
}