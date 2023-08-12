using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using HeavenStudio.Util;
using HeavenStudio.Common;
using HeavenStudio.Editor;
using Jukebox;
using Jukebox.Legacy;
using System;

namespace HeavenStudio
{
    public class StaticCamera : MonoBehaviour
    {
        [SerializeField] RectTransform canvas;
        [SerializeField] GameObject overlayView;

        [SerializeField] Image ambientBg;
        [SerializeField] GameObject ambientBgGO;
        [SerializeField] GameObject letterboxBgGO;
        [SerializeField] RectTransform letterboxMask;
        [SerializeField] GameObject maskObj;

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

        private List<RiqEntity> panEvents = new();
        private List<RiqEntity> scaleEvents = new();
        private List<RiqEntity> rotationEvents = new();
        private List<RiqEntity> letterboxEvents = new();

        static Vector3 defaultPan = new Vector3(0, 0, 0);
        static Vector3 defaultScale = new Vector3(1, 1, 1);
        static float defaultRotation = 0;
        static Vector3 defaultLetterbox = new Vector3(1, 1, 1);

        private static Vector3 pan;
        private static Vector3 scale;
        private static float rotation;
        private static Vector3 letterbox;
        private static Vector3 panInv;
        private static Vector3 scaleInv;

        private static bool toggle;
        private Transform[] children = null;

        private static Vector3 panLast;
        private static Vector3 scaleLast;
        private static float rotationLast;
        private static Vector3 letterboxLast;

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
            letterboxLast = defaultLetterbox;
            scaleLast = defaultScale;
            rotationLast = defaultRotation;

            ToggleLetterboxBg(PersistentDataManager.gameSettings.letterboxBgEnable);
            ToggleLetterboxGlow(PersistentDataManager.gameSettings.letterboxFxEnable);
            /*
            children = new Transform[ maskObj.transform.childCount ];
            foreach(Transform T in transform)
                children.add(T);*/
        }

        public void OnBeatChanged(double beat)
        { 
            Reset();

            panEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "pan view" });
            scaleEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "scale view" });
            rotationEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "rotate view" });
            letterboxEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "scale letterbox" });

            panLast = defaultPan;
            letterboxLast = defaultLetterbox;
            scaleLast = defaultScale;
            rotationLast = defaultRotation;
            
            UpdatePan();
            UpdateRotation();
            UpdateScale();
            UpdateLetterbox();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateLetterbox();
            UpdatePan();
            UpdateRotation();
            UpdateScale();
            
            canvas.localPosition = pan;
            canvas.localScale = scale;
            canvas.eulerAngles = new Vector3(0, 0, rotation);
            //if(!toggle)canvas.localPosition = panInv;
            //if(!toggle)canvas.localScale = scaleInv;
            if(!toggle)
            {
                //canvas.transform.SetParent(null); dont do that
                letterboxMask.localScale = letterbox;
                canvas.transform.localScale = scale;
                //letterboxMask.localScale = letterbox;
                
            } //heyo future me FIX THIS MESS PLEASEAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
            
            //canvas.localScale = canvas.InverseTransformDirection(scaleInv);
            /*if(!toggle)
            {
                canvas.localPosition = canvas.InverseTransformVector(panInv);
                
                canvas.eulerAngles = canvas.TransformVector(new Vector3(0, 0, rotation));
                toggle = true;
            }else{
                canvas.localPosition = pan;
                canvas.localScale = scale;
                canvas.eulerAngles = new Vector3(0, 0, rotation);
            }
            */
        }

        private void UpdatePan()
        {
            foreach (var e in panEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
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
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
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
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
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

        private void UpdateLetterbox()
        {
            foreach (var e in letterboxEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            letterbox.x = func(letterboxLast.x, e["valA"], Mathf.Min(prog, 1f)) * AspectRatioWidth;
                            panInv = pan;
                            scaleInv = scale;
                            toggle = e["toggle"];
                            break;
                        case (int) ViewAxis.Y:
                            letterbox.y = func(letterboxLast.y, e["valB"], Mathf.Min(prog, 1f)) * AspectRatioHeight;
                            panInv = pan;
                            scaleInv = scale;
                            toggle = e["toggle"];
                            break;
                        default:
                            float dx = func(letterboxLast.x, e["valA"], Mathf.Min(prog, 1f)) * AspectRatioWidth;
                            float dy = func(letterboxLast.y, e["valB"], Mathf.Min(prog, 1f)) * AspectRatioHeight;
                            letterbox = new Vector3(dx, dy, 1);
                            panInv = pan;
                            scaleInv = scale;
                            toggle = e["toggle"];
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            letterboxLast.x = e["valA"] * AspectRatioWidth;
                            break;
                        case (int) ViewAxis.Y:
                            letterboxLast.y = e["valB"] * AspectRatioHeight;
                            break;
                        default:
                            letterboxLast = new Vector3(e["valA"] * AspectRatioWidth, e["valB"] * AspectRatioHeight, 1);
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
            letterbox = defaultLetterbox;
            toggle = true;
        }

        public void ToggleOverlayView(bool toggle)
        {
            overlayView.SetActive(toggle);
        }

        [NonSerialized] public bool usingMinigameAmbientColor;

        public void SetAmbientGlowColour(Color colour, bool minigameColor, bool overrideMinigameColor = true)
        {
            if (overrideMinigameColor) usingMinigameAmbientColor = minigameColor;
            ambientBg.color = colour;
            GameSettings.UpdatePreviewAmbient(colour);
        }

        public void ToggleLetterboxBg(bool toggle)
        {
            letterboxBgGO.SetActive(toggle);
        }

        public void ToggleLetterboxGlow(bool toggle)
        {
            ambientBgGO.SetActive(toggle);
        }

        public void ToggleCanvasVisibility(bool toggle)
        {
            canvas.gameObject.SetActive(toggle);
        }
    }
}