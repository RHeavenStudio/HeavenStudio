using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Common
{
    public class Scroll : MonoBehaviour
    {
        public float scrollSpeedX;
        public float scrollSpeedY;
        Vector3 startPos;

        public float lengthX;
        public float lengthY = 43.20976f;

        private void Start()
        {
            startPos = transform.localPosition;
            UpdatePos();
        }

        private void Update()
        {
            UpdatePos();
        }

        private void UpdatePos()
        {
            float newPosX = Mathf.Repeat(Time.time * scrollSpeedX, lengthX);
            float newPosY = Mathf.Repeat(Time.time * scrollSpeedY, lengthY);
            transform.localPosition = startPos + new Vector3(1 * newPosX, 1 * newPosY);
        }
    }
}