using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using UnityEngine.UIElements;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJBigObject : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer _logSR;
        [SerializeField] private Sprite _logCutSprite;
        [SerializeField] private SpriteRenderer _ballSR1;
        [SerializeField] private Sprite _ballCutSprite;
        [SerializeField] private SpriteRenderer _ballSR2;

        private LBJBear _bear;
        private LBJObjectRotate _rotateObject;
        private LumBEARjack.BigType _type;
        private bool _right = true;

        private PlayerActionEvent _hitActionEvent;
        private PlayerActionEvent _cutActionEvent;

        private double _rotationBeat;
        private double _rotationLength;

        private void Awake()
        {
            _rotateObject = GetComponent<LBJObjectRotate>();
            _logSR.gameObject.SetActive(false);
            _ballSR2.gameObject.SetActive(false);
        }

        public void Init(LBJBear bear, double beat, double length, LumBEARjack.BigType type, bool right, double startUpBeat = -1)
        {
            _bear = bear;
            _type = type;
            _right = right;

            _rotationBeat = beat + (length / 4 * 2);
            _rotationLength = length / 4;

            switch (_type)
            {
                case LumBEARjack.BigType.log:
                    _logSR.gameObject.SetActive(true);
                    break;
                case LumBEARjack.BigType.bigBall:
                    _ballSR1.gameObject.SetActive(true);
                    _ballSR2.gameObject.SetActive(true);
                    break;
            }

            if (startUpBeat <= beat + (length / 4 * 2)) _hitActionEvent = LumBEARjack.instance.ScheduleInput(beat, length / 4 * 2, Minigame.InputAction_BasicPress, JustHit, Miss, Blank);
            else
            {
                _rotationBeat = beat + (length / 4 * 3);
                _logSR.sprite = _logCutSprite;
                _ballSR1.sprite = _ballCutSprite;
            }
            _cutActionEvent = LumBEARjack.instance.ScheduleInput(beat, length / 4 * 3, Minigame.InputAction_BasicPress, JustCut, Miss, Blank);
            Update();
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(Minigame.InputAction_BasicPress) && !LumBEARjack.instance.IsExpectingInputNow(Minigame.InputAction_BasicPress))
            {
                LumBEARjack.instance.ScoreMiss();
                Miss(null);
                if (_hitActionEvent != null)
                {
                    _hitActionEvent.Disable();
                    _hitActionEvent.QueueDeletion();
                }
                return;
            }
            if (_type == LumBEARjack.BigType.bigBall)
            {
                _rotateObject.SingleMove(_rotationBeat, _rotationLength, _right);
                return;
            }
            _rotateObject.Move(_rotationBeat, _rotationLength, _right);
        }

        private void JustHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                _bear.SwingWhiff(false);
                Miss(caller);
                return;
            }

            SoundByte.PlayOneShotGame("lumbearjack/hitVoice" + (Random.Range(1, 3) == 1 ? "A" : "B"));
            SoundByte.PlayOneShotGame("lumbearjack/baseHit");

            LumBEARjack.instance.DoBigObjectEffect(_type, true);

            string hitSound = _type switch
            {
                LumBEARjack.BigType.log => "bigLogHit",
                LumBEARjack.BigType.bigBall => "bigBallHit",
                _ => throw new System.NotImplementedException(),
            };
            SoundByte.PlayOneShotGame("lumbearjack/" + hitSound);
            _bear.CutMid();
            _rotationBeat = _cutActionEvent.startBeat + _cutActionEvent.timer;
            _logSR.sprite = _logCutSprite;
            _ballSR1.sprite = _ballCutSprite;
        }

        private void JustCut(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                _bear.SwingWhiff(false);
                Miss(caller);
                return;
            }

            SoundByte.PlayOneShotGame("lumbearjack/bigLogCutVoice");

            LumBEARjack.instance.DoBigObjectEffect(_type, false);

            string cutSound = _type switch
            {
                LumBEARjack.BigType.log => "bigLogCut",
                LumBEARjack.BigType.bigBall => "bigBallCut",
                _ => throw new System.NotImplementedException(),
            };
            SoundByte.PlayOneShotGame("lumbearjack/" + cutSound, -1, 1, (_type == LumBEARjack.BigType.bigBall) ? 2 : 1);
            if (_type == LumBEARjack.BigType.bigBall) SoundByte.PlayOneShotGame("lumbearjack/bigBallHit", -1, 1.5f);
            _bear.Cut(caller.startBeat + caller.timer, false, false);
            Destroy(gameObject);
        }

        private void Miss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShot("miss");
            if (_cutActionEvent != null)
            {
                _cutActionEvent.Disable();
                _cutActionEvent.QueueDeletion();
            }
            SpriteRenderer sr = _type switch
            {
                LumBEARjack.BigType.log => _logSR,
                LumBEARjack.BigType.bigBall => _ballSR2,
                _ => throw new System.NotImplementedException(),
            };
            LumBEARjack.instance.ActivateMissEffect(sr.transform, sr);
            Destroy(gameObject);
        }

        private void Blank(PlayerActionEvent caller) { }
    }
}

