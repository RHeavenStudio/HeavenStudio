using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using Starpelly;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJSmallObject : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField] private float _rotationStart = -22f;
        [SerializeField] private float _rotationEnd = 22f;
        [SerializeField] private EasingFunction.Ease _ease = EasingFunction.Ease.EaseInOutQuad;

        [Header("Components")]
        [SerializeField] private GameObject _log;
        [SerializeField] private GameObject _can;
        [SerializeField] private GameObject _bat;
        [SerializeField] private GameObject _broom;

        private LBJBear _bear;
        private LumBEARjack.SmallType _type;
        private LumBEARjack.HuhChoice _huh;
        private bool _right = true;

        private double _rotationBeat;
        private double _rotationLength;

        private void Awake()
        {
            _log.SetActive(false);
            _can.SetActive(false);
            _bat.SetActive(false);
            _broom.SetActive(false);
        }

        public void Init(LBJBear bear, double beat, double length, LumBEARjack.SmallType type, LumBEARjack.HuhChoice huh, bool right, double startUpBeat = -1)
        {
            _bear = bear;
            _type = type;
            _huh = huh;
            _right = right;

            switch (type)
            {
                case LumBEARjack.SmallType.log:
                    _log.SetActive(true);
                    break;
                case LumBEARjack.SmallType.can:
                    _can.SetActive(true);
                    break;
                case LumBEARjack.SmallType.bat:
                    _bat.SetActive(true);
                    break;
                case LumBEARjack.SmallType.broom:
                    _broom.SetActive(true);
                    break;
                default:
                    break;
            }

            _rotationBeat = beat + (length / 3 * 2);
            _rotationLength = length / 3;
            LumBEARjack.instance.ScheduleInput(beat, length / 3 * 2, Minigame.InputAction_BasicPress, Just, Miss, Blank);
            Update();
        }

        private void Update()
        {
            float normalized = Conductor.instance.GetPositionFromBeat(_rotationBeat - _rotationLength, _rotationLength * 2);

            var func = EasingFunction.GetEasingFunction(_ease);

            float newRotation = func(_rotationStart, _rotationEnd, normalized);
            transform.localEulerAngles = new Vector3(0, 0, newRotation * (_right ? 1 : -1));
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                _bear.SwingWhiff(false);
                Miss(caller);
                return;
            }

            SoundByte.PlayOneShotGame("lumbearjack/cutVoice" + (Random.Range(1, 3) == 1 ? "A" : "B"));

            string cutSound = _type switch
            {
                LumBEARjack.SmallType.log => "smallLogCut",
                LumBEARjack.SmallType.can => "canCut",
                LumBEARjack.SmallType.bat => "batCut",
                LumBEARjack.SmallType.broom => "broomCut",
                _ => throw new System.NotImplementedException()
            };
            SoundByte.PlayOneShotGame("lumbearjack/" + cutSound);

            switch (_huh)
            {
                case LumBEARjack.HuhChoice.ObjectSpecific:
                    if (_type != LumBEARjack.SmallType.log) SoundByte.PlayOneShotGame("lumbearjack/huh", caller.startBeat + caller.timer + 1);
                    _bear.Cut(caller.startBeat + caller.timer, _type != LumBEARjack.SmallType.log, !_right);
                    break;
                case LumBEARjack.HuhChoice.On:
                    SoundByte.PlayOneShotGame("lumbearjack/huh", caller.startBeat + caller.timer + 1);
                    _bear.Cut(caller.startBeat + caller.timer, true, false);
                    break;
                default:
                    _bear.Cut(caller.startBeat + caller.timer, false, false);
                    break;
            }

            Destroy(gameObject);
        }

        private void Miss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShot("miss");
            Destroy(gameObject);
        }

        private void Blank(PlayerActionEvent caller) { }
    }
}
