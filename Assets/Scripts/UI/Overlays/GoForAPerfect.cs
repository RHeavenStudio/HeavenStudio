using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Common
{
    public class GoForAPerfect : MonoBehaviour
    {
        public static GoForAPerfect instance { get; set; }

        [SerializeField] Animator texAnim;
        [SerializeField] Animator pAnim;

        private bool active = false;
        private bool hiddenActive = false;

        public bool perfect;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            perfect = true;
        }

        private void Update() {
            gameObject.SetActive(hiddenActive);
        }

        public void Hit()
        {
            if (!active) return;
            if (!OverlaysManager.OverlaysEnabled) return;
            pAnim.Play("PerfectIcon_Hit", 0, 0);
        }

        public void Miss()
        {
            perfect = false;
            if (!active) return;
            SetInactive();
            if (!OverlaysManager.OverlaysEnabled)
            {
                hiddenActive = false;
                return;
            }

            GameProfiler.instance.perfect = false;

            texAnim.Play("GoForAPerfect_Miss");
            pAnim.Play("PerfectIcon_Miss", -1, 0);
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
            hiddenActive = true;
            active = true;
        }
        public void SetInactive()
        {
            active = false;
        }
    }

}