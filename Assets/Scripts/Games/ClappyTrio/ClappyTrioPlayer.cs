using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_ClappyTrio
{
    public class ClappyTrioPlayer : PlayerActionObject
    {
        private float lastClapBeat;
        private float lastClapLength;
        [SerializeField] private bool clapVacant;

        public bool clapStarted = false;
        public bool canHit;

        private GameObject clapEffect;
        new int aceTimes = 0;

        private void Start()
        {
            clapEffect = transform.GetChild(4).GetChild(3).gameObject;
        }

        public override void OnAce()
        {
            if (aceTimes == 0)
            {
                Clap(true);
                aceTimes++;
            }
        }

        private void Update()
        {
            if (clapVacant == true)
            {
                float normalizedBeat = (Conductor.instance.GetPositionFromBeat(lastClapBeat, lastClapLength));

                StateCheck(normalizedBeat);

                if (normalizedBeat > Minigame.EndTime())
                {
                    clapVacant = false;
                    lastClapLength = 0;
                    lastClapBeat = 0;
                }
            }

            if (PlayerInput.Pressed())
            {
                Clap(false);
            }

        }

        public void SetClapAvailability(float startBeat, float length)
        {
            aceTimes = 0;
            lastClapBeat = startBeat;
            clapVacant = true;
            lastClapLength = length;

            ResetState();
        }

        private void Clap(bool overrideCanHit)
        {
            if (state.early || state.perfect || overrideCanHit)
            {
                clapEffect.SetActive(true);
                Jukebox.PlayOneShotGame("clappyTrio/rightClap");

                if (this.canHit)
                    ClappyTrio.instance.playerHitLast = true;
            }
            else
            {
                clapEffect.SetActive(false);
                Jukebox.PlayOneShot("miss");
                ClappyTrio.instance.playerHitLast = false;

                if (clapStarted)
                    this.canHit = false;
            }

            ClappyTrio.instance.SetFace(ClappyTrio.instance.Lion.Count - 1, 4);
            this.GetComponent<Animator>().Play("Clap", 0, 0);
        }
    }
}