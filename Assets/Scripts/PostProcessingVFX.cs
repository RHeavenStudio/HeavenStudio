using Jukebox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace HeavenStudio
{
    public class PostProcessingVFX : MonoBehaviour
    {
        private PostProcessVolume _volume;

        // events
        private List<RiqEntity> _vignettes = new();

        private void Awake()
        {
            _volume = GetComponent<PostProcessVolume>();
        }

        private void Start()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;
        }

        public void OnBeatChanged(double beat)
        {
            _vignettes = EventCaller.GetAllInGameManagerList("vfx", new string[] { "vignette" });
            UpdateVignette();
        }

        private void Update()
        {
            UpdateVignette();
        }

        private void UpdateVignette()
        {
            if (!_volume.profile.TryGetSettings<Vignette>(out var v)) return;

            v.enabled.Override(false);
            foreach (var e in _vignettes)
            {
                float normalized = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (normalized < 0) break;

                float clampNormal = Mathf.Clamp01(normalized);
                var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

                v.enabled.Override(e["enable"]);
                if (!v.enabled) continue;
                v.rounded.Override(e["rounded"]);

                Color newColor = ColorEase(e["colorStart"], e["colorEnd"], clampNormal, func);

                v.color.Override(newColor);

                float newIntensity = func(e["intenStart"], e["intenEnd"], clampNormal);
                v.intensity.Override(newIntensity);

                float newSmoothness = func(e["smoothStart"], e["smoothEnd"], clampNormal);
                v.smoothness.Override(newSmoothness);

                float newRoundness = func(e["roundStart"], e["roundEnd"], clampNormal);
                v.roundness.Override(newRoundness);
            }
        }

        private Color ColorEase(Color start, Color end, float time, Util.EasingFunction.Function func)
        {
            float newR = func(start.r, end.r, time);
            float newG = func(start.g, end.g, time);
            float newB = func(start.b, end.b, time);

            return new Color(newR, newG, newB, 1);
        }
    }
}

