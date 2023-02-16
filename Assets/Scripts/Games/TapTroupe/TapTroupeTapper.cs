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
                anim.DoScaledAnimationAsync("HitStepFeet", 0.25f);
            }
            else
            {
                anim.DoScaledAnimationAsync("StepFeet", 0.25f);
            }
        }

        public void Bop()
        {
            anim.DoScaledAnimationAsync("BopFeet", 0.3f);
        }
    }
}


