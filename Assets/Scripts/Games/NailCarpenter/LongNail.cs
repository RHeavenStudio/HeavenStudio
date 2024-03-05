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
            
            game.ScheduleInput(targetBeat, 1f, NailCarpenter.InputAction_AltPress, HammmerJust, HammmerMiss, Empty);
            // wrongInput
            game.ScheduleUserInput(targetBeat, 1f, NailCarpenter.InputAction_BasicPress, weakHammmerJust, Empty, Empty);
        }

        private void HammmerJust(PlayerActionEvent caller, float state)
        {
            game.Carpenter.DoScaledAnimationAsync("carpenterHit", 1f);
            if (state >= 1f || state <= -1f)
            {
                nailAnim.DoScaledAnimationAsync(
                    (state >= 1f ? "longNailBendRight" : "longNailBendLeft"), 0.25f);
                SoundByte.PlayOneShot("miss");
                return;
            }
            SoundByte.PlayOneShotGame("nailCarpenter/HammerStrong");
            nailAnim.DoScaledAnimationAsync("longNailHammered", 0.25f);
            game.EyeAnim.DoScaledAnimationAsync("eyeSmile", 0.25f);
        }

        private void weakHammmerJust(PlayerActionEvent caller, float state)
        {
            game.ScoreMiss();
            game.Carpenter.DoScaledAnimationAsync("carpenterHit", 0.25f);
            if (state >= 1f || state <= -1f)
            {
                nailAnim.DoScaledAnimationAsync(
                    (state >= 1f ? "longNailBendRight" : "longNailBendLeft"), 0.25f);
                SoundByte.PlayOneShot("miss");
                return;
            }
            SoundByte.PlayOneShotGame("nailCarpenter/HammerWeak");
            nailAnim.DoScaledAnimationAsync("longNailWeakHammered", 0.25f);
        }

        private void HammmerMiss(PlayerActionEvent caller)
        {
            game.EyeAnim.DoScaledAnimationAsync("eyeBlink", 0.25f);
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