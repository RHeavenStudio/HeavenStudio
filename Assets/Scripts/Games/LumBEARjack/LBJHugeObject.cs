using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJHugeObject : MonoBehaviour
    {
        private LBJBear _bear;
        private LumBEARjack.HugeType _type;
        private PlayerActionEvent[] _soundsToDeleteIfMiss = new PlayerActionEvent[3];

        public void Init(LBJBear bear, double beat, double length, LumBEARjack.HugeType type, double startUpBeat = -1)
        {
            _bear = bear;
            _type = type;

            if (startUpBeat <= beat + (length / 6 * 2)) LumBEARjack.instance.ScheduleInput(beat, length / 6 * 2, Minigame.InputAction_BasicPress, JustHit1, Miss, Blank);
            if (startUpBeat <= beat + (length / 6 * 3)) _soundsToDeleteIfMiss[0] = LumBEARjack.instance.ScheduleInput(beat, length / 6 * 3, Minigame.InputAction_BasicPress, JustHit2, Miss, Blank);
            if (startUpBeat <= beat + (length / 6 * 4)) _soundsToDeleteIfMiss[1] = LumBEARjack.instance.ScheduleInput(beat, length / 6 * 4, Minigame.InputAction_BasicPress, JustHit3, Miss, Blank);
            _soundsToDeleteIfMiss[2] = LumBEARjack.instance.ScheduleInput(beat, length / 6 * 5, Minigame.InputAction_BasicPress, JustCut, Miss, Blank);
        }

        private void JustHit1(PlayerActionEvent caller, float state)
        {
            JustHit(caller, state, 1);
        }

        private void JustHit2(PlayerActionEvent caller, float state)
        {
            JustHit(caller, state, 2);
        }

        private void JustHit3(PlayerActionEvent caller, float state)
        {
            JustHit(caller, state, 3);
        }

        private void JustHit(PlayerActionEvent caller, float state, int step)
        {
            if (state >= 1f || state <= -1f)
            {
                // Barely
                return;
            }

            SoundByte.PlayOneShotGame("lumbearjack/" + (_type == LumBEARjack.HugeType.peach ? "peach" : "hit") + "Voice" + (Random.Range(1, 3) == 1 ? "A" : "B"));
            SoundByte.PlayOneShotGame("lumbearjack/baseHit");

            string hitSound = _type switch
            {
                LumBEARjack.HugeType.log => $"hugeLogHit{step}",
                LumBEARjack.HugeType.freezer => $"freezerHit{step}",
                LumBEARjack.HugeType.peach => $"peachHit{step}",
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

            SoundByte.PlayOneShotGame("lumbearjack/" + (_type == LumBEARjack.HugeType.peach ? "peachCutVoice" : "hugeLogCutVoice"));

            string cutSound = _type switch
            {
                LumBEARjack.HugeType.log => "hugeLogCut",
                LumBEARjack.HugeType.freezer => "freezerCut",
                LumBEARjack.HugeType.peach => "peachCut",
                _ => throw new System.NotImplementedException(),
            };
            SoundByte.PlayOneShotGame("lumbearjack/" + cutSound);
            Destroy(gameObject);
        }

        private void Miss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShot("miss");
            foreach (var s in _soundsToDeleteIfMiss)
            {
                if (s == null) continue;
                s.Disable();
                s.QueueDeletion();
            }
            Destroy(gameObject);
        }

        private void Blank(PlayerActionEvent caller) { }
    }
}

