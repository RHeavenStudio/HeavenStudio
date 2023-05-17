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

        public void LateUpdate()
        {
            var pos = gameObject.transform.position;
            float songPos = Conductor.instance.GetBpmAtBeat(Conductor.instance.songPosition)/10000;
            float _x = songPos*XSpeed;
            float _y = songPos*YSpeed;
            if (AutoScroll && Conductor.instance.NotStopped()) {
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