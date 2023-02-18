using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TapTroupe
{
    public class TapTroupeTapper : MonoBehaviour
    {
        public Animator anim;
        [SerializeField] GameObject impactStep;
        private TapTroupe game;
        [SerializeField] bool player;
        [SerializeField] int soundIndex;
        public enum TapAnim
        {
            Bam,
            Tap,
            BamReady,
            BamTapReady,
            LastTap
        }

        void Awake()
        {
            game = TapTroupe.instance;
            anim = GetComponent<Animator>();
        }

        public void Step(bool hit = true, bool switchFeet = true)
        {
            if (switchFeet) transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
            if (hit)
            {
                if (TapTroupe.prepareTap)
                {
                    anim.DoScaledAnimationAsync("HitStepReadyTap", 0.5f);
                }
                else
                {
                    anim.DoScaledAnimationAsync("HitStepFeet", 0.5f);
                }
            }
            else
            {
                if (TapTroupe.prepareTap)
                {
                    anim.DoScaledAnimationAsync("StepReadyTap", 0.5f);
                }
                else
                {
                    anim.DoScaledAnimationAsync("StepFeet", 0.5f);
                }
            }
        }

        public void Tap(TapAnim animType, bool hit = true, bool switchFeet = true)
        {
            if (switchFeet) transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
            string animName = "";
            if (hit) animName = "Hit";
            switch (animType)
            {
                case TapAnim.Bam:
                    animName += "BamFeet";
                    break;
                case TapAnim.Tap:
                    animName += "TapFeet";
                    break;
                case TapAnim.BamReady:
                    animName += "BamReadyFeet";
                    break;
                case TapAnim.BamTapReady:
                    animName += "BamReadyTap";
                    break;
                case TapAnim.LastTap:
                    if (hit)
                    {
                        animName = "LastTapFeet";
                    }
                    else
                    {
                        animName = "StepFeet";
                    }
                    break;
            }
            anim.DoScaledAnimationAsync(animName, 0.5f);
            if (!player) Jukebox.PlayOneShotGame($"tapTroupe/other{soundIndex}");
        }

        public void Bop()
        {
            anim.DoScaledAnimationAsync("BopFeet", 0.5f);
        }
    }
}


