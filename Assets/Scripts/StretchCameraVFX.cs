using Jukebox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio
{
    public class StretchCameraVFX : MonoBehaviour
    {
        private List<RiqEntity> _events = new();

        private void Start()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;
        }

        public void OnBeatChanged(double beat)
        {
            _events = EventCaller.GetAllInGameManagerList("vfx", new string[] { "stretch camera" });
            Update();
        }

        private void Update()
        {
            float newX = 1f;
            float newY = 1f;
            foreach (var e in _events)
            {
                float normalized = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (normalized < 0f) break;
                float clampNormal = Mathf.Clamp01(normalized);
                var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

                switch ((StaticCamera.ViewAxis)e["axis"])
                {
                    case StaticCamera.ViewAxis.All:
                        newX = func(e["x1"], e["x2"], clampNormal);
                        newY = func(e["y1"], e["y2"], clampNormal);
                        break;
                    case StaticCamera.ViewAxis.X:
                        newX = func(e["x1"], e["x2"], clampNormal);
                        break;
                    case StaticCamera.ViewAxis.Y:
                        newY = func(e["y1"], e["y2"], clampNormal);
                        break;
                }
            }
            EventCaller.instance.GamesHolder.transform.localScale = new Vector3(newX, newY, 1f);
        }
    }
}

