using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_Spaceball
{
    public class Alien : MonoBehaviour
    {
        private Animator anim;

        private float showBeat = 0;
        private bool isShowing = false;

        private void Start()
        {
            anim = GetComponent<Animator>();
            anim.Play("AlienIdle", 0, 0);
        }

        private void Update()
        {
            if (Conductor.instance.isPlaying && !isShowing)
            {
                anim.Play("AlienSwing", 0, Conductor.instance.GetLoopPositionFromBeat(0, 1f));
                anim.speed = 0;
            }
            else if (!Conductor.instance.isPlaying)
            {
                anim.Play("AlienIdle", 0, 0);
            }

            if (isShowing)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(showBeat, 1f);
                anim.Play("AlienShow", 0, normalizedBeat);
                anim.speed = 0;

                if (normalizedBeat >= 2)
                {
                    isShowing = false;
                }
            }
        }

        public void Show(float showBeat)
        {
            isShowing = true;
            this.showBeat = showBeat;
        }
    }
}