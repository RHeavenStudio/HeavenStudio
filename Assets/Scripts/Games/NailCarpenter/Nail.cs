using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_NailCarpenter
{
    public class Nail : MonoBehaviour
    {
        public double targetBeat;
        public Animator nailAnim;

        private NailCarpenter game;

        public void Init()
        {
            game = NailCarpenter.instance;
            
            game.ScheduleInput(targetBeat, 0, NailCarpenter.InputAction_BasicPress, HammmerJust, HammmerMiss, Empty);
            game.ScheduleUserInput(targetBeat, 0, NailCarpenter.InputAction_AltFinish, strongHammmerJust, Empty, Empty);
        }

        private void HammmerJust(PlayerActionEvent caller, float state)
        {
            game.Carpenter.DoScaledAnimationAsync("carpenterHit", 0.5f);
            if (state >= 1f || state <= -1f)
            {
                nailAnim.DoScaledAnimationAsync(
                    (state >= 1f ? "nailBendRight" : "nailBendLeft"), 0.5f);
                SoundByte.PlayOneShot("miss");
                return;
            }
            SoundByte.PlayOneShotGame("nailCarpenter/HammerWeak");
            nailAnim.DoScaledAnimationAsync("nailHammered", 0.5f);
        }
        private void strongHammmerJust(PlayerActionEvent caller, float state)
        {
            game.ScoreMiss();
            game.Carpenter.DoScaledAnimationAsync("carpenterHit", 0.5f);
            if (state >= 1f || state <= -1f)
            {
                nailAnim.DoScaledAnimationAsync(
                    (state >= 1f ? "nailBendRight" : "nailBendLeft"), 0.5f);
                SoundByte.PlayOneShot("miss");
                return;
            }
            SoundByte.PlayOneShotGame("nailCarpenter/HammerStrong");
            nailAnim.DoScaledAnimationAsync("nailStrongHammered", 0.5f);
        }

        private void HammmerMiss(PlayerActionEvent caller)
        {
            game.EyeAnim.DoScaledAnimationAsync("eyeBlink", 0.5f);
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