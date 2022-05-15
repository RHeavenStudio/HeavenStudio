using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using System.Linq;

namespace HeavenStudio
{
    public class GameCamera : MonoBehaviour
    {
        public static GameCamera instance { get; private set; }
        public new Camera camera;

        // default cam position, for quick-resetting
        static Vector3 defaultPosition = new Vector3(0, 0, -10);
        static Vector3 defaultRotEluer = new Vector3(0, 0, 0);
        static Vector3 defaultScale = new Vector3(16, 9, 1);

        /**
            camera's current transformation
            TODO: stretching not working, will need help with this cause I don't understand Unity's camera
        **/
        private static Vector3 position;
        private static Vector3 rotEluer;
        private static Vector3 scale;

        /** 
            transformations to apply *after* the global transform, 
            to use in minigame scripts (Spaceball, Rhythm Rally, Built to Scale, etc.)
            and NOT in the editor
        **/
        public static Vector3 additionalPosition;
        public static Vector3 additionalRotEluer;
        public static Vector3 additionalScale;

        [Header("Components")]
        public Color baseColor;

        private void Awake()
        {
            instance = this;
            camera = this.GetComponent<Camera>();
        }

        private void Start()
        {
            camera.backgroundColor = baseColor;

            ResetTransforms();
            ResetAdditionalTransforms();
        }

        private void Update()
        {
            Camera cam = GetCamera();
            cam.transform.localPosition = position + additionalPosition;
            cam.transform.eulerAngles = rotEluer + additionalRotEluer;
            cam.transform.localScale = Vector3.Scale(scale, additionalScale);
        }

        public static void ResetTransforms()
        {
            position = defaultPosition;
            rotEluer = defaultRotEluer;
            scale = defaultScale;
        }

        public static void ResetAdditionalTransforms()
        {
            additionalPosition = new Vector3(0, 0, 0);
            additionalRotEluer = new Vector3(0, 0, 0);
            additionalScale = new Vector3(1, 1, 1);
        }

        public static Camera GetCamera()
        {
            return instance.camera;
        }
    }
}