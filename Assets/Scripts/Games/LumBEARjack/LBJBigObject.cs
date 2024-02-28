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
        }

        public void Init(LBJBear bear, double beat, double length, LumBEARjack.BigType type, bool right, double startUpBeat = -1)
        {
            _bear = bear;
            _type = type;
            _right = right;

            _rotationBeat = beat + (length / 4 * 2);
            _rotationLength = length / 4;
            if (startUpBeat <= beat + (length / 4 * 2)) _hitActionEvent = LumBEARjack.instance.ScheduleInput(beat, length / 4 * 2, Minigame.InputAction_BasicPress, JustHit, Miss, Blank);
            else
            {
                _rotationBeat = beat + (length / 4 * 3);
                _logSR.sprite = _logCutSprite;
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
                _ => throw new System.NotImplementedException(),
            };
            SoundByte.PlayOneShotGame("lumbearjack/" + hitSound);
            _bear.CutMid();
            _rotationBeat = _cutActionEvent.startBeat + _cutActionEvent.timer;
            _logSR.sprite = _logCutSprite;
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
                _ => throw new System.NotImplementedException(),
            };
            SoundByte.PlayOneShotGame("lumbearjack/" + cutSound);
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
                _ => throw new System.NotImplementedException(),
            };
            LumBEARjack.instance.ActivateMissEffect(sr.transform, sr);
            Destroy(gameObject);
        }

        private void Blank(PlayerActionEvent caller) { }
    }
}

