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
        public bool dontSwitchNextStep = false;
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

        public void FadeOut(float pos)
        {
            anim.DoNormalizedAnimation("FeetFadeOut", pos * 8);
        }

        public void Step(bool hit = true, bool switchFeet = true)
        {
            if (switchFeet && !dontSwitchNextStep) transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
            if (dontSwitchNextStep) dontSwitchNextStep = false;
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
            string animName = "";
            if (hit) animName = "Hit";
            switch (animType)
            {
                case TapAnim.Bam:
                    animName += "BamFeet";
                    if (switchFeet && !dontSwitchNextStep) transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
                    break;
                case TapAnim.Tap:
                    animName += "TapFeet";
                    if (switchFeet) transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
                    break;
                case TapAnim.BamReady:
                    animName += "BamReadyFeet";
                    if (switchFeet) transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
                    break;
                case TapAnim.BamTapReady:
                    animName += "BamReadyTap";
                    if (switchFeet) transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
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
                    if (switchFeet) transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
                    break;
            }
            anim.DoScaledAnimationAsync(animName, 0.5f);
        }

        public void Bop()
        {
            anim.DoScaledAnimationAsync("BopFeet", 0.5f);
        }
    }
}


