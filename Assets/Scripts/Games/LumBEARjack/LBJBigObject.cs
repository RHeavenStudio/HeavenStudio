using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJBigObject : MonoBehaviour
    {
        private LBJBear _bear;
        private LumBEARjack.BigType _type;
        private PlayerActionEvent _cutActionEvent;

        public void Init(LBJBear bear, double beat, double length, LumBEARjack.BigType type, double startUpBeat = -1)
        {
            _bear = bear;
            _type = type;

            if (startUpBeat <= beat + (length / 4 * 2)) LumBEARjack.instance.ScheduleInput(beat, length / 4 * 2, Minigame.InputAction_BasicPress, JustHit, Miss, Blank);
            _cutActionEvent = LumBEARjack.instance.ScheduleInput(beat, length / 4 * 3, Minigame.InputAction_BasicPress, JustCut, Miss, Blank);
        }

        private void JustHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                // Barely
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
        }

        private void JustCut(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                // Barely
                return;
            }

            SoundByte.PlayOneShotGame("lumbearjack/bigLogCutVoice");

            string cutSound = _type switch
            {
                LumBEARjack.BigType.log => "bigLogCut",
                _ => throw new System.NotImplementedException(),
            };
            SoundByte.PlayOneShotGame("lumbearjack/" + cutSound);
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

