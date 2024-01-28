using System.Collections;
using System.Collections.Generic;
using HeavenStudio.Util;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_NtrSamurai
{
    public class NtrSamurai : MonoBehaviour
    {
        [Header("References")]
        public Animator animator;

        [Header("Properties")]
        public bool stepping;

        public void Init()
        {
            stepping = false;
        }

        public void Bop()
        {
            if (!stepping && !animator.IsPlayingAnimationNames("Beat", "Unstep", "Slash"))
                animator.Play("Beat", -1, 0);
        }

        public void Step(bool off)
        {
            stepping = !off;
            if (off)
            {
                animator.Play("Unstep", -1, 0);
            }
            else
            {
                if (animator.IsPlayingAnimationNames("Slash"))
                    animator.Play("StepSeathe", -1, 0);
                else
                    animator.Play("Step", -1, 0);
            }
        }

        public void Slash()
        {
            stepping = false;
            animator.Play("Slash", -1, 0);
        }

        public bool IsStepping()
        {
            return stepping;
        }
    }
}