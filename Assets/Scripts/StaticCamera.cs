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
        [SerializeField] RectTransform overlayCanvas;
        [SerializeField] RectTransform parentView;

        [SerializeField] Image ambientBg;
        [SerializeField] GameObject ambientBgGO;
        [SerializeField] GameObject letterboxBgGO;
        [SerializeField] RectTransform letterboxMask;
        [SerializeField] RawImage viewportTexture;

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
        private List<RiqEntity> panTileEvents = new();
        private List<RiqEntity> tileScreenEvents = new();
        private List<RiqEntity> fitToScreenEvents = new();

        static Vector3 defaultPan = new Vector3(0, 0, 0);
        static Vector3 defaultScale = new Vector3(1, 1, 1);
        static float defaultRotation = 0;
        static Vector3 defaultLetterbox = new Vector3(1, 1, 1);
        static Vector3 defaultTilePan = new Vector3(0, 0, 0);
        static Vector3 defaultScreenTile = new Vector3(1, 1, 1);
        static bool defaultFitToScreen = false;

        private static Vector3 pan;
        private static Vector3 scale;
        private static float rotation;
        private static Vector3 letterbox;
        private static Vector3 panInv;
        private static Vector3 scaleInv;
        private static Vector3 tilePan;
        private static Vector3 screenTile;
        private static bool fitToScreen;

        private static Vector3 panLast;
        private static Vector3 scaleLast;
        private static float rotationLast;
        private static Vector3 letterboxLast;
        private static Vector3 tilePanLast;
        private static Vector3 screenTileLast;

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
            tilePanLast = defaultTilePan;
            screenTileLast = defaultScreenTile;
            fitToScreen = defaultFitToScreen;

            ToggleLetterboxBg(PersistentDataManager.gameSettings.letterboxBgEnable);
            ToggleLetterboxGlow(PersistentDataManager.gameSettings.letterboxFxEnable);
        }

        public void OnBeatChanged(double beat)
        { 
            Reset();

            panEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "pan view" });
            scaleEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "scale view" });
            rotationEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "rotate view" });
            letterboxEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "scale letterbox" });
            fitToScreenEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "fit to screen" });
            panTileEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "pan tiles"});
            tileScreenEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "tile screen" });

            panLast = defaultPan;
            letterboxLast = defaultLetterbox;
            scaleLast = defaultScale;
            rotationLast = defaultRotation;
            tilePanLast = defaultTilePan;
            screenTileLast = defaultScreenTile;
            fitToScreen = defaultFitToScreen;
            
            UpdatePan();
            UpdateRotation();
            UpdateScale();
            UpdateLetterbox();
            UpdateTilePan();
            UpdateScreenTile();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateLetterbox();
            UpdatePan();
            UpdateRotation();
            UpdateScale();
            UpdateTilePan();
            UpdateScreenTile();
            
            canvas.localPosition = pan;
            canvas.eulerAngles = new Vector3(0, 0, rotation);
            //letterboxMask.localScale = letterbox;
            if(fitToScreen)
            {
                letterboxMask.localScale = new Vector3(parentView.sizeDelta.x/16, parentView.sizeDelta.y/9, 1);
                overlayCanvas.localScale = new Vector3(parentView.sizeDelta.x/16, parentView.sizeDelta.y/9, 1);
            }
            else 
            {
                letterboxMask.localScale = new Vector3(1, 1, 1);
                overlayCanvas.localScale = new Vector3(1, 1, 1);
            }
            canvas.localScale = scale;
            viewportTexture.uvRect = new Rect(tilePan.x, tilePan.y, screenTile.x, screenTile.y);
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
                            letterbox.x = func(letterboxLast.x, e["valA"] * .01f * parentView.localScale.x, Mathf.Min(prog, 1f)) * AspectRatioWidth;
                            break;
                        case (int) ViewAxis.Y:
                            letterbox.y = func(letterboxLast.y, e["valB"] * .01f * parentView.localScale.y, Mathf.Min(prog, 1f)) * AspectRatioHeight;
                            break;
                        default:
                            float dx = func(letterboxLast.x, e["valA"] * .01f * parentView.localScale.x, Mathf.Min(prog, 1f)) * AspectRatioWidth;
                            float dy = func(letterboxLast.y, e["valB"] * .01f * parentView.localScale.y, Mathf.Min(prog, 1f)) * AspectRatioHeight;
                            letterbox = new Vector3(dx, dy, 1);
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            letterboxLast.x = e["valA"] * .01f * parentView.localScale.x * AspectRatioWidth;
                            break;
                        case (int) ViewAxis.Y:
                            letterboxLast.y = e["valB"] * .01f * parentView.localScale.y * AspectRatioHeight;
                            break;
                        default:
                            letterboxLast = new Vector3(e["valA"] * .01f * parentView.localScale.x * AspectRatioWidth, e["valB"] * .01f * canvas.localScale.y * AspectRatioHeight, 1);
                            break;
                    }
                }
            }
        }

        private void UpdateTilePan()
        {
            foreach (var e in panTileEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            tilePan.x = func(tilePanLast.x, e["valA"], Mathf.Min(prog, 1f));
                            break;
                        case (int) ViewAxis.Y:
                            tilePan.y = func(tilePan.y, e["valB"], Mathf.Min(prog, 1f));
                            break;
                        default:
                            float dx = func(tilePanLast.x, e["valA"], Mathf.Min(prog, 1f));
                            float dy = func(tilePanLast.y, e["valB"], Mathf.Min(prog, 1f));
                            tilePan = new Vector3(dx, dy, 0);
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            tilePanLast.x = e["valA"];
                            break;
                        case (int) ViewAxis.Y:
                            tilePanLast.y = e["valB"];
                            break;
                        default:
                            tilePanLast = new Vector3(e["valA"], e["valB"], 0);
                            break;
                    }
                }
            }
        }

        private void UpdateScreenTile()
        {
            foreach (var e in tileScreenEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            screenTile.x = func(screenTileLast.x, e["valA"] * canvas.localScale.x, Mathf.Min(prog, 1f)) * AspectRatioWidth;
                            break;
                        case (int) ViewAxis.Y:
                            screenTile.y = func(screenTileLast.y, e["valB"] * canvas.localScale.y, Mathf.Min(prog, 1f)) * AspectRatioHeight;
                            break;
                        default:
                            float dx = func(screenTileLast.x, e["valA"] * canvas.localScale.x, Mathf.Min(prog, 1f)) * AspectRatioWidth;
                            float dy = func(screenTileLast.y, e["valB"] * canvas.localScale.y, Mathf.Min(prog, 1f)) * AspectRatioHeight;
                            screenTile = new Vector3(dx, dy, 1);
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            screenTileLast.x = e["valA"] * canvas.localScale.x * AspectRatioWidth;
                            break;
                        case (int) ViewAxis.Y:
                            screenTileLast.y = e["valB"] * canvas.localScale.y * AspectRatioHeight;
                            break;
                        default:
                            screenTileLast = new Vector3(e["valA"] * canvas.localScale.x * AspectRatioWidth, e["valB"] * canvas.localScale.y * AspectRatioHeight, 1);
                            break;
                    }
                }
            }
        }

        public void UpdateFitToScreen(bool toggle)
        {
            fitToScreen = toggle;
        }

        public static void Reset()
        {
            pan = defaultPan;
            scale = defaultScale;
            rotation = defaultRotation;
            letterbox = defaultLetterbox;
            tilePan = defaultTilePan;
            screenTile = defaultScreenTile;
            fitToScreen = defaultFitToScreen;
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