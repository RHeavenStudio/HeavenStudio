using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Common
{
    public class FollowMouse : MonoBehaviour
    {
        public Vector2 offset;
        public Camera cam;

        private void Awake()
        {
            if (cam == null)
                cam = Camera.main;
        }

        private void Update()
        {
            var pos = cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(pos.x - offset.x, pos.y - offset.y, 0);
        }
    }
}