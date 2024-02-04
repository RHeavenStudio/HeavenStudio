using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJBigObject : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField] private float _rotationStart = -22f;
        [SerializeField] private float _rotationEnd = 22f;
        [SerializeField] private EasingFunction.Ease _ease = EasingFunction.Ease.EaseInOutQuad;

        [Header("Components")]
        [SerializeField] private SpriteRenderer _logSR;
        [SerializeField] private Sprite _logCutSprite;

        private LBJBear _bear;
        private LumBEARjack.BigType _type;
        private PlayerActionEvent _cutActionEvent;

        private double _rotationBeat;
        private double _rotationLength;

        public void Init(LBJBear bear, double beat, double length, LumBEARjack.BigType type, double startUpBeat = -1)
        {
            _bear = bear;
            _type = type;

            _rotationBeat = beat + (length / 4 * 2);
            _rotationLength = length / 4;
            if (startUpBeat <= beat + (length / 4 * 2)) LumBEARjack.instance.ScheduleInput(beat, length / 4 * 2, Minigame.InputAction_BasicPress, JustHit, Miss, Blank);
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
            float normalized = Conductor.instance.GetPositionFromBeat(_rotationBeat - _rotationLength, _rotationLength * 2);

            var func = EasingFunction.GetEasingFunction(_ease);

            float newRotation = func(_rotationStart, _rotationEnd, normalized);
            transform.localEulerAngles = new Vector3(0, 0, newRotation);
        }

        private void JustHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                _bear.SwingWhiff(false, true);
                Miss(caller);
                return;
            }

            SoundByte.PlayOneShotGame("lumbearjack/hitVoice" + (Random.Range(1, 3) == 1 ? "A" : "B"));
            SoundByte.PlayOneShotGame("lumbearjack/baseHit");

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
                _bear.SwingWhiff(false, true);
                Miss(caller);
                return;
            }

            SoundByte.PlayOneShotGame("lumbearjack/bigLogCutVoice");

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
            Destroy(gameObject);
        }

        private void Blank(PlayerActionEvent caller) { }
    }
}

