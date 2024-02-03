using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJSmallObject : MonoBehaviour
    {
        private LBJBear _bear;
        private LumBEARjack.SmallType _type;

        public void Init(LBJBear bear, double beat, double length, LumBEARjack.SmallType type, double startUpBeat = -1)
        {
            _bear = bear;
            _type = type;

            LumBEARjack.instance.ScheduleInput(beat, length / 3 * 2, Minigame.InputAction_BasicPress, Just, Miss, Blank);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                // Barely
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

            if (_type != LumBEARjack.SmallType.log) SoundByte.PlayOneShotGame("lumbearjack/huh", caller.startBeat + caller.timer + 1);

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
