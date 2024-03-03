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
        [SerializeField] private GameObject _barrel;
        [SerializeField] private GameObject _book;

        private LBJBear _bear;
        private LBJObjectRotate _rotateObject;
        private LumBEARjack.SmallType _type;
        private LumBEARjack.HuhChoice _huh;
        private bool _right = true;
        private bool _bomb = true;

        private double _rotationBeat;
        private double _rotationLength;

        private PlayerActionEvent _inputEvent;

        private void Awake()
        {
            _log.SetActive(false);
            _can.SetActive(false);
            _bat.SetActive(false);
            _broom.SetActive(false);
            _barrel.SetActive(false);
            _book.SetActive(false);
            _rotateObject = GetComponent<LBJObjectRotate>();
        }

        public void Init(LBJBear bear, double beat, double length, LumBEARjack.SmallType type, LumBEARjack.HuhChoice huh, bool right, bool bomb, double startUpBeat = -1)
        {
            _bear = bear;
            _type = type;
            _huh = huh;
            _right = right;
            _bomb = bomb;

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
                case LumBEARjack.SmallType.barrel:
                    _barrel.SetActive(true);
                    break;
                case LumBEARjack.SmallType.book:
                    _book.SetActive(true);
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
                LumBEARjack.SmallType.barrel => "barrelCut",
                LumBEARjack.SmallType.book => "bookCut",
                _ => throw new System.NotImplementedException()
            };
            SoundByte.PlayOneShotGame("lumbearjack/" + cutSound);
            if (_type == LumBEARjack.SmallType.book) SoundByte.PlayOneShotGame("lumbearjack/bookBoom");

            LumBEARjack.instance.DoSmallObjectEffect(_type, _bomb, caller.startBeat + caller.timer);

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
                LumBEARjack.SmallType.barrel => _barrel.GetComponent<SpriteRenderer>(),
                LumBEARjack.SmallType.book => _book.GetComponent<SpriteRenderer>(),
                _ => throw new System.NotImplementedException(),
            };
            LumBEARjack.instance.ActivateMissEffect(sr.transform, sr);
            Destroy(gameObject);
        }

        private void Blank(PlayerActionEvent caller) { }
    }
}
