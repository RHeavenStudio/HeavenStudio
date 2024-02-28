using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using Starpelly;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJSmallObject : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject _log;
        [SerializeField] private GameObject _can;
        [SerializeField] private GameObject _bat;
        [SerializeField] private GameObject _broom;

        private LBJBear _bear;
        private LBJObjectRotate _rotateObject;
        private LumBEARjack.SmallType _type;
        private LumBEARjack.HuhChoice _huh;
        private bool _right = true;

        private double _rotationBeat;
        private double _rotationLength;

        private PlayerActionEvent _inputEvent;

        private void Awake()
        {
            _log.SetActive(false);
            _can.SetActive(false);
            _bat.SetActive(false);
            _broom.SetActive(false);
            _rotateObject = GetComponent<LBJObjectRotate>();
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
            _inputEvent = LumBEARjack.instance.ScheduleInput(beat, length / 3 * 2, Minigame.InputAction_BasicPress, Just, Miss, Blank);
            Update();
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(Minigame.InputAction_BasicPress) && !LumBEARjack.instance.IsExpectingInputNow(Minigame.InputAction_BasicPress))
            {
                LumBEARjack.instance.ScoreMiss();
                Miss(_inputEvent);
                _inputEvent.Disable();
                _inputEvent.QueueDeletion();
                return;
            }
            if (_type == LumBEARjack.SmallType.bat)
            {
                _rotateObject.SingleMove(_rotationBeat, _rotationLength, _right);
                return;
            }
            _rotateObject.Move(_rotationBeat, _rotationLength, _right);
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

            LumBEARjack.instance.DoSmallObjectEffect(_type);

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
            SpriteRenderer sr = _type switch
            {
                LumBEARjack.SmallType.log => _log.GetComponent<SpriteRenderer>(),
                LumBEARjack.SmallType.can => _can.GetComponent<SpriteRenderer>(),
                LumBEARjack.SmallType.bat => _bat.GetComponent<SpriteRenderer>(),
                LumBEARjack.SmallType.broom => _broom.GetComponent<SpriteRenderer>(),
                _ => throw new System.NotImplementedException(),
            };
            LumBEARjack.instance.ActivateMissEffect(sr.transform, sr);
            Destroy(gameObject);
        }

        private void Blank(PlayerActionEvent caller) { }
    }
}
