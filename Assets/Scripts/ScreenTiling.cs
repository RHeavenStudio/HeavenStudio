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

        private List<RiqEntity> _events = new();

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
            _events = EventCaller.GetAllInGameManagerList("vfx", new string[] { "screenTiling" });
            ResetUVRect();
            Update();
        }

        private void Update()
        {
            foreach (var e in _events)
            {
                float normalized = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (normalized < 0) break;

                float clampNormal = Mathf.Clamp01(normalized);

                var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

                float newXTiles = func(e["xStart"], e["xEnd"], clampNormal);
                float newYTiles = func(e["yStart"], e["yEnd"], clampNormal);
                float newXScroll = func(e["xScrollStart"], e["xScrollEnd"], clampNormal);
                float newYScroll = func(e["yScrollStart"], e["yScrollEnd"], clampNormal);

                _image.uvRect = new Rect(newXScroll, newYScroll, newXTiles, newYTiles);
            }
        }

        public void ResetUVRect()
        {
            _image.uvRect = new Rect(0, 0, 1, 1);
        }
    }
}

