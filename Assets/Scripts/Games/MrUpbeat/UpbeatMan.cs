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

        public void Step()
        {
            stepTimes++;

            animator.Play("Step", 0, 0);
            Jukebox.PlayOneShotGame("mrUpbeat/step");
            
            bool x = (stepTimes % 2 == 1);
            shadows[0].SetActive(!x);
            shadows[1].SetActive(x);
            transform.localScale = new Vector3(x ? -1 : 1, 1);
        }

        public void Fall()
        {
            animator.Play("Fall", 0, 0);
            Jukebox.PlayOneShot("miss");
            shadows[0].SetActive(false);
            shadows[1].SetActive(false);
        }
    }
}