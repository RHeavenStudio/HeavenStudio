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
using Kirurobo;

namespace HeavenStudio
{
    public class WindowController : MonoBehaviour
    {
        [SerializeField] UniWindowController UniController;
        public static WindowController instance { get; private set; }

        public enum ViewAxis
        {
            All,
            X,
            Y,
        }

        float AspectRatioWidth;
        float AspectRatioHeight;

        private List<RiqEntity> panEvents = new();
        private List<RiqEntity> scaleEvents = new();
        private List<RiqEntity> shakeEvents = new();

        public static Vector3 defaultPan;
        public static Vector3 defaultScale;
        public static Vector3 defaultShake = new Vector3(0, 0, 0);

        private static Vector3 pan;
        private static Vector3 scale;
        private static Vector3 shakeResult;

        private static Vector3 panLast;
        private static Vector3 scaleLast;

        public void SetPosition(short x, short y, short resX, short resY)
        {
            #if UNITY_EDITOR
            return;
            #else
            if(Application.isEditor || !Editor.Editor.instance.fullscreen || !Conductor.instance.isPlaying) return;
            if(PersistentDataManager.gameSettings.windowDanceEnable)
            {
                UniController.windowSize = (new Vector2(resX, resY));
                UniController.windowPosition = (new Vector2(x, y));
            }
            #endif
        }
        
        private void Awake()
        {
            AspectRatioWidth = Screen.mainWindowDisplayInfo.width;
            AspectRatioHeight = Screen.mainWindowDisplayInfo.height;
            defaultScale = new Vector3(Screen.width, Screen.height, 0);
            defaultPan = new Vector3(Screen.mainWindowPosition.x, Screen.mainWindowPosition.y, 0);
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;

            Reset();

            panLast = defaultPan;
            scaleLast = defaultScale;
        }

        public void OnBeatChanged(double beat)
        { 
            Reset();

            panEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "pan window" });
            scaleEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "scale window" });

            shakeEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "shake window" });

            panLast = defaultPan;
            scaleLast = defaultScale;

            UpdateScale();
            UpdatePan();
            //SetShakeIntensity();
        }

        // Update is called once per frame
        void Update()
        {
            //DO NOT PUT SCALE AFTER PAN
            //BAD IDEA
            //VERY BAD
            //idk if its a bad idea anymore
            //prob isnt
            UpdateScale();
            UpdatePan();
            //SetShakeIntensity();
            SetPosition(Convert.ToInt16(pan.x), Convert.ToInt16(pan.y), Convert.ToInt16(scale.x), Convert.ToInt16(scale.y));
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
                            pan.x = func(panLast.x, e["valA"] * .01f * Screen.mainWindowDisplayInfo.width, Mathf.Min(prog, 1f));
                            break;
                        case (int) ViewAxis.Y:
                            pan.y = func(panLast.y, e["valB"]* .01f * Screen.mainWindowDisplayInfo.height, Mathf.Min(prog, 1f));
                            break;
                        default:
                            float dx = func(panLast.x, e["valA"] * .01f * Screen.mainWindowDisplayInfo.width, Mathf.Min(prog, 1f)); 
                            float dy = func(panLast.y, e["valB"] * .01f * Screen.mainWindowDisplayInfo.height, Mathf.Min(prog, 1f));
                            pan = new Vector3(dx, dy, 0);
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            panLast.x = e["valA"] * .01f * Screen.mainWindowDisplayInfo.width;
                            break;
                        case (int) ViewAxis.Y:
                            panLast.y = e["valB"] * .01f * Screen.mainWindowDisplayInfo.height;
                            break;
                        default:
                            panLast = new Vector3(e["valA"] * .01f * Screen.mainWindowDisplayInfo.width, e["valB"] * .01f * Screen.mainWindowDisplayInfo.height, 0);
                            break;
                    }
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
                            scale.x = func(scaleLast.x, e["valA"] * .01f * Screen.mainWindowDisplayInfo.width, Mathf.Min(prog, 1f));
                            break;
                        case (int) ViewAxis.Y:
                            scale.y = func(scaleLast.y, e["valB"] * .01f * Screen.mainWindowDisplayInfo.height, Mathf.Min(prog, 1f));
                            break;
                        default:
                            float dx = func(scaleLast.x, e["valA"] * .01f * Screen.mainWindowDisplayInfo.width, Mathf.Min(prog, 1f));
                            float dy = func(scaleLast.y, e["valB"] * .01f * Screen.mainWindowDisplayInfo.height, Mathf.Min(prog, 1f));                            scale = new Vector3(dx, dy, 1);
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            scaleLast.x = e["valA"] * .01f * Screen.mainWindowDisplayInfo.width;
                            break;
                        case (int) ViewAxis.Y:
                            scaleLast.y = e["valB"] * .01f * Screen.mainWindowDisplayInfo.height;
                            break;
                        default:
                            scaleLast = new Vector3(e["valA"] * .01f * Screen.mainWindowDisplayInfo.width, e["valB"] * .01f * Screen.mainWindowDisplayInfo.height, 1);
                            break;
                    }
                }
            }
        }

        /*
        private void SetShakeIntensity()
        {
            foreach (var e in shakeEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    float fac = Mathf.Cos(Time.time * 80f) * 0.5f;
                    shakeResult = new Vector3(fac * e["valA"], fac * e["valB"]);
                    Debug.Log(shakeResult);
                }
                if (prog > 1f)
                {
                    shakeResult = new Vector3(0, 0, 0);
                    Debug.Log(shakeResult);
                }
            }
        }*/

        public void Reset()
        {
            pan = defaultPan;
            scale = defaultScale;
            shakeResult = defaultShake;
            SetPosition(Convert.ToInt16(pan.x), Convert.ToInt16(pan.y), Convert.ToInt16(scale.x), Convert.ToInt16(scale.y));
        }
    }
}