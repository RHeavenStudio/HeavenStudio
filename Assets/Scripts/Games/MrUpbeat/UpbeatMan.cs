using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Starpelly;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MrUpbeat
{
    public class UpbeatMan : MonoBehaviour
    {
        [Header("References")]
        public Animator animator;
        public Animator blipAnimator;
        public GameObject[] shadows;

        public int stepTimes = 0;
        private bool onGround = false;

        public void Idle()
        {
            stepTimes = 0;
            transform.localScale = new Vector3(1, 1);
            animator.Play("Idle", 0, 0);
        }

        public void Step()
        {
            stepTimes++;

            animator.Play("Step", 0, 0);
            Jukebox.PlayOneShotGame("mrUpbeat/step");

            onGround = false;
            CheckShadows();
        }

        public void Fall()
        {
            animator.Play("Fall", 0, 0);
            Jukebox.PlayOneShot("miss");
            shadows[0].SetActive(false);
            shadows[1].SetActive(false);
            onGround = true;
        }

        private void CheckShadows()
        {
            if (onGround) return;

            if (stepTimes % 2 == 1)
            {
                shadows[0].SetActive(false);
                shadows[1].SetActive(true);
                transform.localScale = new Vector3(-1, 1);
            } else
            {
                shadows[0].SetActive(true);
                shadows[1].SetActive(false);
                transform.localScale = new Vector3(1, 1);
            }
        }
    }
}