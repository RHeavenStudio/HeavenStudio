using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJBear : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator _anim;
        [SerializeField] private Transform _cameraPoint;

        [Header("Properties")]
        [SerializeField] private double _zoomInLength = 0.25;
        [SerializeField] private float _zoomInPower = 4f;
        [SerializeField] private EasingFunction.Ease _ease = EasingFunction.Ease.EaseOutBounce;

        private bool _rested = false;
        private LumBEARjack.RestSoundChoice _restSound;

        private float _cameraPointZFrom;
        private EasingFunction.Function _cameraFunc;

        private void Awake()
        {
            _cameraPointZFrom = _cameraPoint.localPosition.z;
            _cameraFunc = EasingFunction.GetEasingFunction(_ease);
        }

        public void SwingWhiff(bool sound = true)
        {
            if (_rested) return;
            if (sound) SoundByte.PlayOneShotGame("lumbearjack/swing", -1, SoundByte.GetPitchFromCents(Random.Range(-200, 201), false));
            _anim.DoScaledAnimationAsync("BeastWhiff", 0.75f);
        }

        public void Cut(double beat, bool huh, bool huhL, bool zoomIn = false)
        {
            _anim.DoScaledAnimationAsync(huh ? "BeastHalfCut" : "BeastCut", 0.75f);
            if (zoomIn) ActivateZoomIn();
            if (!huh) return;
            BeatAction.New(this, new()
            {
                new(beat + 1, delegate
                {
                    _anim.DoScaledAnimationAsync(huhL ? "BeastHuhL" : "BeastHuhR", 0.5f);
                }),
                new(beat + 2, delegate
                {
                    _anim.DoScaledAnimationAsync("BeastReady", 0.75f);
                })
            });
        }

        public void CutMid(bool noImpact = false)
        {
            _anim.DoScaledAnimationAsync("BeastCutMid" + (noImpact ? "NoImpact" : ""), 0.75f);
        }

        public void Bop()
        {
            if (_anim.IsPlayingAnimationNames("BeastWhiff", "BeastRest") || _rested) return;
            _anim.DoScaledAnimationAsync("BeastBop", 0.75f);
        }

        public void Rest(bool instant, LumBEARjack.RestSoundChoice sound)
        {
            _anim.DoScaledAnimationAsync("BeastRest", 0.5f, instant ? 1 : 0);
            _rested = true;
            _restSound = sound;
        }

        public void RestSound()
        {
            switch (_restSound)
            {
                case LumBEARjack.RestSoundChoice.Random:
                    SoundByte.PlayOneShotGame("lumbearjack/sigh" + (Random.Range(1, 3) == 1 ? "A" : "B"));
                    break;
                case LumBEARjack.RestSoundChoice.restA:
                    SoundByte.PlayOneShotGame("lumbearjack/sighA");
                    break;
                case LumBEARjack.RestSoundChoice.restB:
                    SoundByte.PlayOneShotGame("lumbearjack/sighB");
                    break;
                default:
                    break;
            }
        }

        private Coroutine _currentZoomCo;

        private void ActivateZoomIn()
        {
            if (_currentZoomCo != null) StopCoroutine(_currentZoomCo);
            _currentZoomCo = StartCoroutine(ActivateZoomInCo());
        }

        private IEnumerator ActivateZoomInCo() 
        {
            double startBeat = Conductor.instance.songPositionInBeatsAsDouble;
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, _zoomInLength, false);

            while (normalizedBeat <= 1f)
            {
                normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, _zoomInLength, false);
                float newZ = _cameraFunc(_cameraPointZFrom + _zoomInPower, _cameraPointZFrom, Mathf.Clamp01(normalizedBeat));
                _cameraPoint.localPosition = new Vector3(_cameraPoint.localPosition.x, _cameraPoint.localPosition.y, newZ);
                yield return null;
            }
        }
    }

}
