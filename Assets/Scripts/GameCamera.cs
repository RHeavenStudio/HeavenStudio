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

        public enum CameraAxis
        {
            All,
            X,
            Y,
            Z
        }

        private List<DynamicBeatmap.DynamicEntity> positionEvents = new List<DynamicBeatmap.DynamicEntity>();
        private List<DynamicBeatmap.DynamicEntity> rotationEvents = new List<DynamicBeatmap.DynamicEntity>();
        private List<DynamicBeatmap.DynamicEntity> shakeEvents = new List<DynamicBeatmap.DynamicEntity>();

        /**
            default cam position, for quick-resetting
        **/
        public static Vector3 defaultPosition = new Vector3(0, 0, -10);
        public static Vector3 defaultRotEluer = new Vector3(0, 0, 0);
        public static Vector3 defaultShake = new Vector3(0, 0, 0);
        public static float defaultFoV = 53.15f;

        /**
            camera's current transformation
        **/
        private static Vector3 position;
        private static Vector3 rotEluer;
        private static Vector3 shakeResult;

        /**
            camera's last transformation
        **/
        private static Vector3 positionLast;
        private static Vector3 rotEluerLast;
        private static Vector3 shakeLast;

        /** 
            transformations to apply *after* the global transform, 
            to use in minigame scripts (Spaceball, Rhythm Rally, Built to Scale, etc.)
            and NOT in the editor
        **/
        public static Vector3 additionalPosition;
        public static Vector3 additionalRotEluer;
        public static Vector3 additionalScale;
        public static float additionalFoV;

        [Header("Components")]
        public Color baseColor;

        private void Awake()
        {
            instance = this;
            camera = this.GetComponent<Camera>();
        }

        private void Start()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;
            camera.backgroundColor = baseColor;

            ResetTransforms();
            ResetAdditionalTransforms();
            
            positionLast = defaultPosition;
            rotEluerLast = defaultRotEluer;
        }

        public void OnBeatChanged(float beat)
        {
            ResetTransforms();
            ResetAdditionalTransforms();

            positionLast = defaultPosition;
            rotEluerLast = defaultRotEluer;

            // this entire thing is a mess redo it later
            //pos
            positionEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "move camera" });
            // legacy event
            positionEvents.AddRange(EventCaller.GetAllInGameManagerList("gameManager", new string[] { "move camera" }));

            //rot
            rotationEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "rotate camera" });
            positionEvents.AddRange(EventCaller.GetAllInGameManagerList("gameManager", new string[] { "rotate camera" }));

            //screen shake time baybee

            shakeEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "screen shake" });

            UpdateCameraTranslate();
            UpdateCameraRotate();
            SetShakeIntensity();
        }

        private void Update()
        {
            UpdateCameraTranslate();
            UpdateCameraRotate();
            SetShakeIntensity();
        }

        private void LateUpdate()
        {
            Camera cam = GetCamera();
            // rotate position by additional rotation
            Vector3 userPos = Quaternion.Euler(additionalRotEluer) * position;
            cam.transform.localPosition = userPos + additionalPosition + shakeResult;
            cam.transform.eulerAngles = rotEluer + additionalRotEluer;
            cam.fieldOfView = additionalFoV;
        }

        private void UpdateCameraTranslate()
        {
            foreach (var e in positionEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    EasingFunction.Function func = EasingFunction.GetEasingFunction((EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) CameraAxis.X:
                            position.x = func(positionLast.x, e["valA"], Mathf.Min(prog, 1f));
                            break;
                        case (int) CameraAxis.Y:
                            position.y = func(positionLast.y, e["valB"], Mathf.Min(prog, 1f));
                            break;
                        case (int) CameraAxis.Z:
                            position.z = func(positionLast.z, -e["valC"], Mathf.Min(prog, 1f));
                            break;
                        default:
                            float dx = func(positionLast.x, e["valA"], Mathf.Min(prog, 1f));
                            float dy = func(positionLast.y, e["valB"], Mathf.Min(prog, 1f));
                            float dz = func(positionLast.z, -e["valC"], Mathf.Min(prog, 1f));
                            position = new Vector3(dx, dy, dz);
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) CameraAxis.X:
                            positionLast.x = e["valA"];
                            break;
                        case (int) CameraAxis.Y:
                            positionLast.y = e["valB"];
                            break;
                        case (int) CameraAxis.Z:
                            positionLast.z = -e["valC"];
                            break;
                        default:
                            positionLast = new Vector3(e["valA"], e["valB"], -e["valC"]);
                            break;
                    }
                }
            }
        }

        private void UpdateCameraRotate()
        {
            foreach (var e in rotationEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    EasingFunction.Function func = EasingFunction.GetEasingFunction((EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) CameraAxis.X:
                            rotEluer.x = func(rotEluerLast.x, e["valA"], Mathf.Min(prog, 1f));
                            break;
                        case (int) CameraAxis.Y:
                            rotEluer.y = func(rotEluerLast.y, e["valB"], Mathf.Min(prog, 1f));
                            break;
                        case (int) CameraAxis.Z:
                            rotEluer.z = func(rotEluerLast.z, -e["valC"], Mathf.Min(prog, 1f));
                            break;
                        default:
                            float dx = func(rotEluerLast.x, e["valA"], Mathf.Min(prog, 1f));
                            float dy = func(rotEluerLast.y, e["valB"], Mathf.Min(prog, 1f));
                            float dz = func(rotEluerLast.z, -e["valC"], Mathf.Min(prog, 1f));
                            rotEluer = new Vector3(dx, dy, dz);    //I'm stupid and forgot to negate the rotation gfd 😢
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) CameraAxis.X:
                            rotEluerLast.x = e["valA"];
                            break;
                        case (int) CameraAxis.Y:
                            rotEluerLast.y = e["valB"];
                            break;
                        case (int) CameraAxis.Z:
                            rotEluerLast.z = -e["valC"];
                            break;
                        default:
                            rotEluerLast = new Vector3(e["valA"], e["valB"], -e["valC"]);
                            break;
                    }
                    rotEluerLast = new Vector3(e["valA"], e["valB"], -e["valC"]);
                }
            }
        }

        private void SetShakeIntensity()
        {
            foreach (var e in shakeEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    float fac = Mathf.Cos(Time.time * 80f) * 0.5f;
                    shakeResult = new Vector3(fac * e["valA"], fac * e["valB"]);
                }
                if (prog > 1f)
                {
                    shakeResult = new Vector3(0, 0);
                }
            }
        }

        public static void ResetTransforms()
        {
            position = defaultPosition;
            rotEluer = defaultRotEluer;
            shakeResult = defaultShake;
        }

        public static void ResetAdditionalTransforms()
        {
            additionalPosition = new Vector3(0, 0, 0);
            additionalRotEluer = new Vector3(0, 0, 0);
            additionalFoV = defaultFoV;
        }

        public static Camera GetCamera()
        {
            return instance.camera;
        }
    }
}