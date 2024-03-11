using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Common
{
    public class StickyCanvas : MonoBehaviour
    {
        /// <summary>
        /// Attach to a GameObject to make the object follow the camera while also moving with the viewport.
        /// Can be enabled or disabled.
        /// May malfunction when rescaled.
        /// </summary>
        private Vector3 _OriginalPosition;
        private Quaternion _OriginalRotation;

        [SerializeField] public bool Sticky = true;
        [SerializeField] float CameraOffset = 10;

        // Start is called before the first frame update
        void Start()
        {
            _OriginalPosition = transform.position;
            _OriginalRotation = transform.rotation;
        }

        void Update()
        {
            if (!Sticky)
            {
                transform.position = _OriginalPosition;
                transform.rotation = _OriginalRotation;
                return;
            }

            if (Conductor.instance == null) return;
            Transform target = GameCamera.instance.transform;

            Vector3 displacement = target.forward * CameraOffset;
            transform.position = target.position + displacement;
            transform.rotation = target.rotation;
        }
    }
}