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
        private List<RiqEntity> _cabbs = new();
        private List<RiqEntity> _blooms = new();
        private List<RiqEntity> _lensDs = new();
        private List<RiqEntity> _grains = new();
        private List<RiqEntity> _colorGradings = new();

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
            _cabbs = EventCaller.GetAllInGameManagerList("vfx", new string[] { "cabb" });
            _blooms = EventCaller.GetAllInGameManagerList("vfx", new string[] { "bloom" });
            _lensDs = EventCaller.GetAllInGameManagerList("vfx", new string[] { "lensD" });
            _grains = EventCaller.GetAllInGameManagerList("vfx", new string[] { "grain" });
            _colorGradings = EventCaller.GetAllInGameManagerList("vfx", new string[] { "colorGrading" });

            UpdateVignette();
            UpdateChromaticAbberations();
            UpdateBlooms();
            UpdateLensDistortions();
            UpdateGrain();
            UpdateColorGrading();
        }

        private void Update()
        {
            UpdateVignette();
            UpdateChromaticAbberations();
            UpdateBlooms();
            UpdateLensDistortions();
            UpdateGrain();
            UpdateColorGrading();
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

                float newIntensity = func(e["intenStart"], e["intenEnd"], clampNormal);

                v.enabled.Override(newIntensity != 0);
                if (!v.enabled) continue;
                v.rounded.Override(e["rounded"]);

                Color newColor = ColorEase(e["colorStart"], e["colorEnd"], clampNormal, func);

                v.color.Override(newColor);

                v.intensity.Override(newIntensity);

                float newSmoothness = func(e["smoothStart"], e["smoothEnd"], clampNormal);
                v.smoothness.Override(newSmoothness);

                float newRoundness = func(e["roundStart"], e["roundEnd"], clampNormal);
                v.roundness.Override(newRoundness);
            }
        }

        private void UpdateChromaticAbberations()
        {
            if (!_volume.profile.TryGetSettings<ChromaticAberration>(out var c)) return;
            c.enabled.Override(false);
            foreach (var e in _cabbs)
            {
                float normalized = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (normalized < 0) break;

                float clampNormal = Mathf.Clamp01(normalized);
                var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

                float newIntensity = func(e["intenStart"], e["intenEnd"], clampNormal);
                c.enabled.Override(newIntensity != 0);
                if (!c.enabled) continue;
                c.intensity.Override(newIntensity);
            }
        }

        private void UpdateBlooms()
        {
            if (!_volume.profile.TryGetSettings<Bloom>(out var b)) return;
            b.enabled.Override(false);
            foreach (var e in _blooms)
            {
                float normalized = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (normalized < 0) break;

                float clampNormal = Mathf.Clamp01(normalized);
                var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

                float newIntensity = func(e["intenStart"], e["intenEnd"], clampNormal);
                b.enabled.Override(newIntensity != 0);
                if (!b.enabled) continue;
                b.intensity.Override(newIntensity);

                Color newColor = ColorEase(e["colorStart"], e["colorEnd"], clampNormal, func);

                b.color.Override(newColor);

                float newThreshold = func(e["thresholdStart"], e["thresholdEnd"], clampNormal);
                b.threshold.Override(newThreshold);

                float newSoftKnee = func(e["softKneeStart"], e["softKneeEnd"], clampNormal);
                b.softKnee.Override(newSoftKnee);

                float newAna = func(e["anaStart"], e["anaEnd"], clampNormal);
                b.anamorphicRatio.Override(newAna);
            }
        }

        private void UpdateLensDistortions()
        {
            if (!_volume.profile.TryGetSettings<LensDistortion>(out var l)) return;
            l.enabled.Override(false);
            foreach (var e in _lensDs)
            {
                float normalized = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (normalized < 0) break;

                float clampNormal = Mathf.Clamp01(normalized);
                var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

                float newIntensity = func(e["intenStart"], e["intenEnd"], clampNormal);
                l.enabled.Override(newIntensity != 0);
                if (!l.enabled) continue;
                l.intensity.Override(newIntensity);

                float newX = func(e["xStart"], e["xEnd"], clampNormal);
                l.intensityX.Override(newX);

                float newY = func(e["yStart"], e["yEnd"], clampNormal);
                l.intensityY.Override(newY);
            }
        }

        private void UpdateGrain()
        {
            if (!_volume.profile.TryGetSettings<Grain>(out var g)) return;
            g.enabled.Override(false);
            foreach (var e in _grains)
            {
                float normalized = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (normalized < 0) break;

                float clampNormal = Mathf.Clamp01(normalized);
                var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

                float newIntensity = func(e["intenStart"], e["intenEnd"], clampNormal);
                g.enabled.Override(newIntensity != 0);
                if (!g.enabled) continue;
                g.intensity.Override(newIntensity);
                g.colored.Override(e["colored"]);

                float newSize = func(e["sizeStart"], e["sizeEnd"], clampNormal);
                g.size.Override(newSize);
            }
        }

        private void UpdateColorGrading()
        {
            if (!_volume.profile.TryGetSettings<ColorGrading>(out var c)) return;
            c.enabled.Override(false);
            foreach (var e in _colorGradings)
            {
                c.enabled.Override(true);
                float normalized = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (normalized < 0) break;

                float clampNormal = Mathf.Clamp01(normalized);
                var func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)e["ease"]);

                float newTemp = func(e["tempStart"], e["tempEnd"], clampNormal);
                c.temperature.Override(newTemp);

                float newTint = func(e["tintStart"], e["tintEnd"], clampNormal);
                c.tint.Override(newTint);

                Color newColor = ColorEase(e["colorStart"], e["colorEnd"], clampNormal, func);
                c.colorFilter.Override(newColor);

                float newHue = func(e["hueShiftStart"], e["hueShiftEnd"], clampNormal);
                c.hueShift.Override(newHue);

                float newSat = func(e["satStart"], e["satEnd"], clampNormal);
                c.saturation.Override(newSat);

                float newBright = func(e["brightStart"], e["brightEnd"], clampNormal);
                c.brightness.Override(newBright);

                float newCon = func(e["conStart"], e["conEnd"], clampNormal);
                c.contrast.Override(newCon);
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

