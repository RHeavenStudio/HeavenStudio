using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using HeavenStudio.Util;

namespace HeavenStudio
{
    public class StaticCamera : MonoBehaviour
    {
        [SerializeField] RectTransform canvas;
        [SerializeField] GameObject overlayView;
        public static StaticCamera instance { get; private set; }
        public new Camera camera;

        public enum ViewAxis
        {
            All,
            X,
            Y,
        }

        const float AspectRatioWidth = 1;
        const float AspectRatioHeight = 1;

        private List<DynamicBeatmap.DynamicEntity> panEvents = new List<DynamicBeatmap.DynamicEntity>();
        private List<DynamicBeatmap.DynamicEntity> scaleEvents = new List<DynamicBeatmap.DynamicEntity>();
        private List<DynamicBeatmap.DynamicEntity> rotationEvents = new List<DynamicBeatmap.DynamicEntity>();

        static Vector3 defaultPan = new Vector3(0, 0, 0);
        static Vector3 defaultScale = new Vector3(1, 1, 1);
        static float defaultRotation = 0;

        private static Vector3 pan;
        private static Vector3 scale;
        private static float rotation;

        private static Vector3 panLast;
        private static Vector3 scaleLast;
        private static float rotationLast;

        private void Awake()
        {
            instance = this;
            camera = this.GetComponent<Camera>();
        }

        // Start is called before the first frame update
        void Start()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;

            Reset();

            panLast = defaultPan;
            scaleLast = defaultScale;
            rotationLast = defaultRotation;
        }

        public void OnBeatChanged(float beat)
        { 
            Reset();

            panEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "pan view" });
            scaleEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "scale view" });
            rotationEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "rotate view" });

            panLast = defaultPan;
            scaleLast = defaultScale;
            rotationLast = defaultRotation;

            UpdatePan();
            UpdateRotation();
            UpdateScale();
        }

        // Update is called once per frame
        void Update()
        {
            UpdatePan();
            UpdateRotation();
            UpdateScale();

            canvas.localPosition = pan;
            canvas.eulerAngles = new Vector3(0, 0, rotation);
            canvas.localScale = scale;
        }

        private void UpdatePan()
        {
            foreach (var e in panEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    EasingFunction.Function func = EasingFunction.GetEasingFunction((EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            pan.x = func(panLast.x, e["valA"], Mathf.Min(prog, 1f));
                            break;
                        case (int) ViewAxis.Y:
                            pan.y = func(panLast.y, e["valB"], Mathf.Min(prog, 1f));
                            break;
                        default:
                            float dx = func(panLast.x, e["valA"], Mathf.Min(prog, 1f));
                            float dy = func(panLast.y, e["valB"], Mathf.Min(prog, 1f));
                            pan = new Vector3(dx, dy, 0);
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            panLast.x = e["valA"];
                            break;
                        case (int) ViewAxis.Y:
                            panLast.y = e["valB"];
                            break;
                        default:
                            panLast = new Vector3(e["valA"], e["valB"], 0);
                            break;
                    }
                }
            }
        }

        private void UpdateRotation()
        {
            foreach (var e in rotationEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    EasingFunction.Function func = EasingFunction.GetEasingFunction((EasingFunction.Ease) e["ease"]);
                    rotation = func(rotationLast, -e["valA"], Mathf.Min(prog, 1f));
                }
                if (prog > 1f)
                {
                    rotationLast = -e["valA"];
                }
            }
        }

        private void UpdateScale()
        {
            foreach (var e in scaleEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    EasingFunction.Function func = EasingFunction.GetEasingFunction((EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            scale.x = func(scaleLast.x, e["valA"], Mathf.Min(prog, 1f)) * AspectRatioWidth;
                            break;
                        case (int) ViewAxis.Y:
                            scale.y = func(scaleLast.y, e["valB"], Mathf.Min(prog, 1f)) * AspectRatioHeight;
                            break;
                        default:
                            float dx = func(scaleLast.x, e["valA"], Mathf.Min(prog, 1f)) * AspectRatioWidth;
                            float dy = func(scaleLast.y, e["valB"], Mathf.Min(prog, 1f)) * AspectRatioHeight;
                            scale = new Vector3(dx, dy, 1);
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            scaleLast.x = e["valA"] * AspectRatioWidth;
                            break;
                        case (int) ViewAxis.Y:
                            scaleLast.y = e["valB"] * AspectRatioHeight;
                            break;
                        default:
                            scaleLast = new Vector3(e["valA"] * AspectRatioWidth, e["valB"] * AspectRatioHeight, 1);
                            break;
                    }
                }
            }
        }

        public static void Reset()
        {
            pan = defaultPan;
            scale = defaultScale;
            rotation = defaultRotation;
        }

        public void ToggleOverlayView(bool toggle)
        {
            overlayView.SetActive(toggle);
        }
    }
}