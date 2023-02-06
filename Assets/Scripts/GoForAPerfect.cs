using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio
{
    public class GoForAPerfect : MonoBehaviour
    {
        public static GoForAPerfect instance { get; set; }

        [SerializeField] Animator texAnim;
        [SerializeField] Animator pAnim;

        private bool active = false;

        public bool perfect;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            perfect = true;
        }

        public void Hit()
        {
            if (!active) return;
            pAnim.Play("PerfectIcon_Hit", 0, 0);
        }

        public void Miss()
        {
            perfect = false;
            if (!active) return;
            SetInactive();

            GameProfiler.instance.perfect = false;

            texAnim.Play("GoForAPerfect_Miss");
            Jukebox.PlayOneShot("perfectMiss");
        }

        public void Enable()
        {
            SetActive();
            gameObject.SetActive(true);
            pAnim.gameObject.SetActive(true);
            texAnim.gameObject.SetActive(true);
            texAnim.Play("GoForAPerfect_Idle");
        }

        public void Disable()
        {
            SetInactive();
            gameObject.SetActive(false);
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