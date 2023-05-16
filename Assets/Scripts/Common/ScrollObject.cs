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
            float songPos = Conductor.instance.GetBpmAtBeat(Conductor.instance.songPosition)/10000;
            float _x = songPos*XSpeed;
            float _y = songPos*YSpeed;
            if (AutoScroll && Conductor.instance.NotStopped()) {
                gameObject.transform.position += new Vector3(_x, _y);
                
                if (XSpeed != 0) {
                    if (XSpeed > 0) {
                        if (gameObject.transform.position.x >= PositiveBounds.x) {
                            SetPos(NegativeBounds.x, gameObject.transform.position.y);
                        }
                    } else {
                        if (gameObject.transform.position.x <= NegativeBounds.x) {
                            SetPos(PositiveBounds.x, gameObject.transform.position.y);
                        }
                    }
                }
                
                if (YSpeed != 0) {
                    if (YSpeed > 0) {
                        if (gameObject.transform.position.y >= PositiveBounds.y) {
                            SetPos(gameObject.transform.position.x, NegativeBounds.y);
                        }
                    } else {
                        if (gameObject.transform.position.y >= NegativeBounds.y) {
                            SetPos(gameObject.transform.position.x, PositiveBounds.y);
                        }
                    }
                }    
            }
            Debug.Log(gameObject.transform.position);
        }

        public void SetPos(float x, float y)
        {
            gameObject.transform.position = new Vector3(x, y);
        }
    }
}