using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_NailCarpenter
{
    public class Pudding : MonoBehaviour
    {
        public double targetBeat;
        public Sprite[] puddingSprites;
        public int puddingType;
        public SpriteRenderer puddingSprite;
        public Animator pudAnim;

        private NailCarpenter game;

        public void Init()
        {
            game = NailCarpenter.instance;

            puddingSprite.sprite = puddingSprites[puddingType];
            game.ScheduleUserInput(targetBeat, 0, NailCarpenter.InputAction_BasicPress, HammmerJust, Empty, Empty);
            game.ScheduleUserInput(targetBeat, 0, NailCarpenter.InputAction_AltFinish, HammmerJust, Empty, Empty);
        }

        private void HammmerJust(PlayerActionEvent caller, float state)
        {
            game.ScoreMiss();
            game.HammerArm.DoScaledAnimationAsync("hammerHit", 0.5f);
            SoundByte.PlayOneShot("miss");
            game.EffectShock.DoScaledAnimationAsync("ShockAppear", 0.5f);
            puddingSprite.sprite = puddingSprites[puddingType+1];
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