using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Common
{
    public class Billboard : MonoBehaviour
    {
        public float fixedSize = 0.03f;
        public bool constantScale = false;
        private Camera cam;

        private void Start()
        {
            cam = GameManager.instance.GameCamera;    
        }

        private void LateUpdate()
        {
            this.transform.rotation = cam.transform.rotation;

            if (constantScale)
            {
                var distance = (cam.transform.position - this.transform.position).magnitude;
                var size = distance * fixedSize * cam.fieldOfView;
                this.transform.localScale = Vector3.one * size;
                transform.forward = transform.position - cam.transform.position;
            }
        }
    }
}