using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using System.Linq;
using Jukebox;
using Jukebox.Legacy;

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

        private List<RiqEntity> positionEvents = new();
        private List<RiqEntity> rotationEvents = new();
        private List<RiqEntity> shakeEvents = new();
        private List<RiqEntity> colorEvents = new();

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
        private static Vector3 _position;
        private static Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                instance.ApplyCameraValues();
            }
        }
        private static Vector3 _rotEuler;
        private static Vector3 RotEuler
        {
            get => _rotEuler;
            set
            {
                _rotEuler = value;
                instance.ApplyCameraValues();
            }
        }
        private static Vector3 _shakeResult;
        private static Vector3 ShakeResult
        {
            get => _shakeResult;
            set
            {
                _shakeResult = value;
                instance.ApplyCameraValues();
            }
        }

        /**
            camera's last transformation
        **/
        private static Vector3 positionLast;
        private static Vector3 rotEulerLast;
        private static Vector3 shakeLast;

        /** 
            transformations to apply *after* the global transform, 
            to use in minigame scripts (Spaceball, Rhythm Rally, Built to Scale, etc.)
            and NOT in the editor
        **/
        private static Vector3 _additionalPosition;
        public static Vector3 AdditionalPosition
        {
            get => _additionalPosition;
            set
            {
                _additionalPosition = value;
                instance.ApplyCameraValues();
            }
        }
        private static Vector3 _additionalRotEuler;
        public static Vector3 AdditionalRotEuler
        {
            get => _additionalRotEuler;
            set
            {
                _additionalRotEuler = value;
                instance.ApplyCameraValues();
            }
        }
        private static Vector3 _additionalScale;
        public static Vector3 AdditionalScale
        {
            get => _additionalScale;
            set
            {
                _additionalScale = value;
                instance.ApplyCameraValues();
            }
        }
        private static float _additionalFoV;
        public static float AdditionalFoV
        {
            get => _additionalFoV;
            set
            {
                _additionalFoV = value;
                instance.ApplyCameraValues();
            }
        }

        [Header("Components")]
        public Color baseColor;

        public static Color currentColor;

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
            currentColor = baseColor;
            
            positionLast = defaultPosition;
            rotEulerLast = defaultRotEluer;
        }

        public void OnBeatChanged(double beat)
        {
            ResetTransforms();
            ResetAdditionalTransforms();
            currentColor = baseColor;

            positionLast = defaultPosition;
            rotEulerLast = defaultRotEluer;

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

            //color/colour time baybee

            colorEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "camera background color" });

            UpdateCameraTranslate();
            UpdateCameraRotate();
            UpdateShakeIntensity();
            UpdateCameraColor();
        }

        private void Update()
        {
            UpdateCameraTranslate();
            UpdateCameraRotate();
            UpdateShakeIntensity();
            UpdateCameraColor();
        }

        private void LateUpdate()
        {
            Camera cam = GetCamera();
            cam.backgroundColor = currentColor;
            if (!StaticCamera.instance.usingMinigameAmbientColor) StaticCamera.instance.SetAmbientGlowColour(currentColor, false, false);
        }

        private void ApplyCameraValues()
        {
            Camera cam = GetCamera();
            // rotate position by additional rotation
            Vector3 userPos = Quaternion.Euler(_additionalRotEuler) * _position;
            cam.transform.localPosition = userPos + _additionalPosition + _shakeResult;
            cam.transform.eulerAngles = _rotEuler + _additionalRotEuler;
            cam.fieldOfView = _additionalFoV;
            //Debug.Log("Camera Pos: " + _additionalPosition);
        }

        private void UpdateCameraColor()
        {
            foreach (var e in colorEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);
                    float newColorR = func(e["color"].r, e["color2"].r, prog);
                    float newColorG = func(e["color"].g, e["color2"].g, prog);
                    float newColorB = func(e["color"].b, e["color2"].b, prog);

                    currentColor = new Color(newColorR, newColorG, newColorB);
                }
                if (prog > 1f)
                {
                    currentColor = e["color2"];
                }
            }
        }

        private void UpdateCameraTranslate()
        {
            foreach (var e in positionEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) CameraAxis.X:
                            Position = new Vector3(func(positionLast.x, e["valA"], Mathf.Min(prog, 1f)), Position.y, Position.z);
                            break;
                        case (int) CameraAxis.Y:
                            Position = new Vector3(Position.x, func(positionLast.y, e["valB"], Mathf.Min(prog, 1f)), Position.z);
                            break;
                        case (int) CameraAxis.Z:
                            Position = new Vector3(Position.x, Position.y, func(positionLast.z, -e["valC"], Mathf.Min(prog, 1f)));
                            break;
                        default:
                            float dx = func(positionLast.x, e["valA"], Mathf.Min(prog, 1f));
                            float dy = func(positionLast.y, e["valB"], Mathf.Min(prog, 1f));
                            float dz = func(positionLast.z, -e["valC"], Mathf.Min(prog, 1f));
                            Position = new Vector3(dx, dy, dz);
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
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) CameraAxis.X:
                            RotEuler = new Vector3(func(rotEulerLast.x, e["valA"], Mathf.Min(prog, 1f)), RotEuler.y, RotEuler.z);
                            break;
                        case (int) CameraAxis.Y:
                            RotEuler = new Vector3(RotEuler.x, func(rotEulerLast.y, e["valB"], Mathf.Min(prog, 1f)), RotEuler.z);
                            break;
                        case (int) CameraAxis.Z:
                            RotEuler = new Vector3(RotEuler.x, RotEuler.y, func(rotEulerLast.z, -e["valC"], Mathf.Min(prog, 1f)));
                            break;
                        default:
                            float dx = func(rotEulerLast.x, e["valA"], Mathf.Min(prog, 1f));
                            float dy = func(rotEulerLast.y, e["valB"], Mathf.Min(prog, 1f));
                            float dz = func(rotEulerLast.z, -e["valC"], Mathf.Min(prog, 1f));
                            RotEuler = new Vector3(dx, dy, dz);    //I'm stupid and forgot to negate the rotation gfd 😢
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) CameraAxis.X:
                            rotEulerLast.x = e["valA"];
                            break;
                        case (int) CameraAxis.Y:
                            rotEulerLast.y = e["valB"];
                            break;
                        case (int) CameraAxis.Z:
                            rotEulerLast.z = -e["valC"];
                            break;
                        default:
                            rotEulerLast = new Vector3(e["valA"], e["valB"], -e["valC"]);
                            break;
                    }
                    rotEulerLast = new Vector3(e["valA"], e["valB"], -e["valC"]);
                }
            }
        }

        private void UpdateShakeIntensity()
        {
            foreach (var e in shakeEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                float fac = Mathf.Cos(Time.time * 80f) * 0.5f;
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
                    ShakeResult = new Vector3(fac * func(e["easedA"], e["valA"], Mathf.Min(prog, 1f)), fac * func(e["easedB"], e["valB"], Mathf.Min(prog, 1f)));
                }
                if (prog > 1f)
                {
                    ShakeResult = new Vector3(fac * e["valA"], fac * e["valB"]);
                }
            }
        }

        public static void ResetTransforms()
        {
            Position = defaultPosition;
            RotEuler = defaultRotEluer;
            ShakeResult = defaultShake;
        }

        public static void ResetAdditionalTransforms()
        {
            AdditionalPosition = new Vector3(0, 0, 0);
            AdditionalRotEuler = new Vector3(0, 0, 0);
            AdditionalFoV = defaultFoV;
        }

        public static Camera GetCamera()
        {
            return instance.camera;
        }
    }
}