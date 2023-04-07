using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Common
{
    public class CircularMotion : MonoBehaviour
    {
        public float timeOffset = 0;
        public float timeCounter = 0;
        [SerializeField] Transform rootPos;
        public float speed;
        public float width;
        public float height;

        private void Start()
        {
            timeCounter = 0;
        }

        private void Update()
        {
            timeCounter += Time.deltaTime * speed;
            float x = Mathf.Cos(timeCounter + timeOffset) * width + rootPos.position.x;
            float y = Mathf.Sin(timeCounter + timeOffset) * height + rootPos.position.y;
            float z = transform.position.z;

            transform.position = new Vector3(x, y, z);
        }
    }

}
