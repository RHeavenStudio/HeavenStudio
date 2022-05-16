using System.Collections;
using System.Collections.Generic;
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

        // Update is called once per frame
        void Update()
        {
            
        }

        public void Bop()
        {
            if (!stepping && !(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Slash"))
                animator.Play("Beat", -1, 0);
        }

        public void Step(bool off)
        {
            stepping = !off;
            if (off)
            {
                animator.Play("Beat", -1, 0);
            }
            else
            {
                if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Slash")
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

        public bool isStepping()
        {
            return stepping;
        }
    }
}