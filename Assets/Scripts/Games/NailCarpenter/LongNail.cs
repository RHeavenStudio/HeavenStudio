using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_NailCarpenter
{
    public class LongNail : MonoBehaviour
    {
        public double targetBeat;
        public Animator nailAnim;

        private NailCarpenter game;

        public void Init()
        {
            game = NailCarpenter.instance;
            
            game.ScheduleInput(targetBeat, 0.5f, NailCarpenter.InputAction_AltFinish, HammmerJust, HammmerMiss, Empty);
            // wrongInput
            game.ScheduleUserInput(targetBeat, 0.5f, NailCarpenter.InputAction_BasicPress, weakHammmerJust, Empty, Empty);
        }

        private void HammmerJust(PlayerActionEvent caller, float state)
        {
            game.HammerArm.DoScaledAnimationAsync("hammerHit", 0.5f);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                return;
            }
            SoundByte.PlayOneShotGame("nailCarpenter/somen_catch");
            nailAnim.DoScaledAnimationAsync("longNailHammered", 0.5f);
        }

        private void weakHammmerJust(PlayerActionEvent caller, float state)
        {
            game.ScoreMiss();
            game.HammerArm.DoScaledAnimationAsync("hammerHit", 0.5f);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                return;
            }
            nailAnim.DoScaledAnimationAsync("longNailWeakHammered", 0.5f);
            SoundByte.PlayOneShotGame("nailCarpenter/somen_catch");
        }

        private void HammmerMiss(PlayerActionEvent caller)
        {
            game.EffectShock.DoScaledAnimationAsync("ShockAppear", 0.5f);
        }

        private void Empty(PlayerActionEvent caller) { }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                double beat = cond.songPositionInBeats;
                if (targetBeat !=  double.MinValue)
                {
                    if (beat >= targetBeat + 9) Destroy(gameObject);
                }
            }
        }
    }
}