using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_NailCarpenter
{
    public class Sweet : MonoBehaviour
    {
        public double targetBeat;
        public sweetsType sweetType;
        public Animator sweetAnim;
        // public SpriteRenderer sweetSprite;
        
        public enum sweetsType
        {
            Pudding=0,
            CherryPudding=1,
            ShortCake=2,
            Cherry=3,
            LayerCake=4,
        };

        private NailCarpenter game;

        public void Init()
        {
            game = NailCarpenter.instance;

            AwakeAnim();
            game.ScheduleUserInput(targetBeat, 0, NailCarpenter.InputAction_BasicPress, HammmerJust, Empty, Empty);
            game.ScheduleUserInput(targetBeat, 0, NailCarpenter.InputAction_AltFinish, HammmerJust, Empty, Empty);
        }
        private void AwakeAnim()
        {
            switch(sweetType)
            {
                case sweetsType.Pudding:
                    sweetAnim.Play("puddingIdle", -1, 0);
                    break;
                case sweetsType.CherryPudding:
                    sweetAnim.Play("cherryPuddingIdle", -1, 0);
                    break;
                case sweetsType.ShortCake:
                    sweetAnim.Play("shortCakeIdle", -1, 0);
                    break;
                case sweetsType.Cherry:
                    sweetAnim.Play("cherryIdle", -1, 0);
                    break;
                case sweetsType.LayerCake:
                    sweetAnim.Play("layerCakeIdle", -1, 0);
                    break;
            }
        }
        private void BreakAnim()
        {
            switch(sweetType)
            {
                case sweetsType.Pudding:
                    sweetAnim.DoScaledAnimationAsync("puddingBreak", 0.5f);
                    break;
                case sweetsType.CherryPudding:
                    sweetAnim.DoScaledAnimationAsync("cherryPuddingBreak", 0.5f);
                    break;
                case sweetsType.ShortCake:
                    sweetAnim.DoScaledAnimationAsync("shortCakeBreak", 0.5f);
                    break;
                case sweetsType.Cherry:
                    sweetAnim.DoScaledAnimationAsync("cherryBreak", 0.5f);
                    break;
                case sweetsType.LayerCake:
                    sweetAnim.DoScaledAnimationAsync("layerCakeBreak", 0.5f);
                    break;
            }
        }

        private void HammmerJust(PlayerActionEvent caller, float state)
        {
            game.ScoreMiss();
            BreakAnim();
            game.Carpenter.DoScaledAnimationAsync("carpenterHit", 0.5f);
            SoundByte.PlayOneShot("miss");
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