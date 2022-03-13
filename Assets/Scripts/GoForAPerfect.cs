using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio
{
    public class GoForAPerfect : MonoBehaviour
    {
        public static GoForAPerfect instance { get; set; }

        private Animator pAnim;

        private bool active = false;

        public bool perfect;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            pAnim = transform.GetChild(0).GetChild(0).GetComponent<Animator>();
            perfect = true;
        }

        public void Hit()
        {
            if (!active) return;
            pAnim.Play("PerfectIcon_Hit", 0, 0);
        }

        public void Miss()
        {
            if (!active) return;
            perfect = false;

            GameProfiler.instance.perfect = false;

            transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            this.GetComponent<Animator>().Play("GoForAPerfect_Miss");
            Jukebox.PlayOneShot("perfectMiss");
        }

        public void Enable()
        {
            SetActive();
            transform.GetChild(0).gameObject.SetActive(true);
        }

        public void Disable()
        {
            SetInactive();
            transform.GetChild(0).gameObject.SetActive(false);
        }

        public void SetActive()
        {
            active = true;
        }
        public void SetInactive()
        {
            active = false;
        }
    }

}