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
        public float targetX;
        public float metresPerSecond;
        public sweetsType sweetType;
        public Animator sweetAnim;
        // public SpriteRenderer sweetSprite;

        public enum sweetsType
        {
            None = -1,
            Pudding = 0,
            CherryPudding = 1,
            ShortCake = 2,
            Cherry = 3,
            LayerCake = 4,
        };

        private NailCarpenter game;

        public void Init()
        {
            game = NailCarpenter.instance;

            AwakeAnim();
            game.ScheduleUserInput(targetBeat, 0, NailCarpenter.InputAction_SweetsHit, HammmerJust, null, null);
            Update();
        }

        private void AwakeAnim()
        {
            switch (sweetType)
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
            switch (sweetType)
            {
                case sweetsType.Pudding:
                    sweetAnim.DoScaledAnimationAsync("puddingBreak", 0.25f);
                    break;
                case sweetsType.CherryPudding:
                    sweetAnim.DoScaledAnimationAsync("cherryPuddingBreak", 0.25f);
                    break;
                case sweetsType.ShortCake:
                    sweetAnim.DoScaledAnimationAsync("shortCakeBreak", 0.25f);
                    break;
                case sweetsType.Cherry:
                    sweetAnim.DoScaledAnimationAsync("cherryBreak", 0.25f);
                    break;
                case sweetsType.LayerCake:
                    sweetAnim.DoScaledAnimationAsync("layerCakeBreak", 0.25f);
                    break;
            }
        }

        private void HammmerJust(PlayerActionEvent caller, float state)
        {
            game.ScoreMiss();
            BreakAnim();
            game.Carpenter.DoScaledAnimationAsync("eyeBlink", 0.25f, animLayer: 1);
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                double beat = cond.songPositionInBeats;
                Vector3 pos = transform.position;
                pos.x = targetX + (float)((beat - targetBeat) * metresPerSecond);
                transform.position = pos;
                if (targetBeat != double.MinValue)
                {
                    if (beat >= targetBeat + 9) Destroy(gameObject);
                }
            }
        }
    }
}