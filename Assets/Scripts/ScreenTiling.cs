using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;
using UnityEngine.UI;

namespace HeavenStudio
{
    public class ScreenTiling : MonoBehaviour
    {
        private RawImage _image;

        private List<RiqEntity> _tileEvents = new();
        private List<RiqEntity> _scrollEvents = new();

        private void Awake()
        {
            _image = GetComponent<RawImage>();
        }

        private void Start()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;
        }

        public void OnBeatChanged(double beat)
        {
            _tileEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "screenTiling" });
            _scrollEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "scrollTiles" });
            ResetUVRect();
            Update();
        }

        private void Update()
        {
            float newXTiles = 1;
            float newYTiles = 1;
            float newXScroll = 0;
            float newYScroll = 0;
            foreach (var e in _tileEvents)
            {
                float normalized = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (normalized < 0) break;

                float clampNormal = Mathf.Clamp01(normalized);

                var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

                switch ((StaticCamera.ViewAxis)e["axis"])
                {
                    case StaticCamera.ViewAxis.All:
                        newXTiles = func(e["xStart"], e["xEnd"], clampNormal);
                        newYTiles = func(e["yStart"], e["yEnd"], clampNormal);
                        break;
                    case StaticCamera.ViewAxis.X:
                        newXTiles = func(e["xStart"], e["xEnd"], clampNormal);
                        break;
                    case StaticCamera.ViewAxis.Y:
                        newYTiles = func(e["yStart"], e["yEnd"], clampNormal);
                        break;
                }
            }
            
            foreach (var e in _scrollEvents)
            {
                float normalized = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (normalized < 0) break;

                float clampNormal = Mathf.Clamp01(normalized);

                var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

                switch ((StaticCamera.ViewAxis)e["axis"])
                {
                    case StaticCamera.ViewAxis.All:
                        newXScroll = func(e["xScrollStart"], e["xScrollEnd"], clampNormal);
                        newYScroll = func(e["yScrollStart"], e["yScrollEnd"], clampNormal);
                        break;
                    case StaticCamera.ViewAxis.X:
                        newXScroll = func(e["xScrollStart"], e["xScrollEnd"], clampNormal);
                        break;
                    case StaticCamera.ViewAxis.Y:
                        newYScroll = func(e["yScrollStart"], e["yScrollEnd"], clampNormal);
                        break;
                }
            }
            _image.uvRect = new Rect(newXScroll, newYScroll, newXTiles, newYTiles);
        }

        public void ResetUVRect()
        {
            _image.uvRect = new Rect(0, 0, 1, 1);
        }
    }
}

