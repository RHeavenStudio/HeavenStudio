using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DrummingPractice
{
    public class DrummerHit : PlayerActionObject
    {
        public float startBeat;
        public bool applause = true;
        private bool hit = false;
        private bool hasHit = false;

        // Start is called before the first frame update
        void Start()
        {
            PlayerActionInit(gameObject, startBeat);
        }

        public override void OnAce()
        {
            Hit(true);
        }

        // Update is called once per frame
        void Update()
        {
            if (Conductor.instance.GetPositionFromBeat(startBeat, 2) >= 1)
            {
                DrummingPractice.instance.SetFaces(0);
                CleanUp();
            }

            if (!hit && Conductor.instance.GetPositionFromBeat(startBeat, 1) >= 1)
            {
                Jukebox.PlayOneShotGame("drummingPractice/drum");
                DrummingPractice.instance.leftDrummer.Hit(true, false);
                DrummingPractice.instance.rightDrummer.Hit(true, false);
                hit = true;
                if (hasHit) CleanUp();
            }

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, 1f);
            StateCheck(normalizedBeat);

            if (PlayerInput.Pressed())
            {
                if (state.perfect)
                {
                    Hit(true);
                } else if (state.notPerfect())
                {
                    Hit(false);
                }
            }
        }

        public void Hit(bool _hit)
        {
            if (!hasHit)
            {
                DrummingPractice.instance.player.Hit(_hit, applause, true);
                DrummingPractice.instance.SetFaces(_hit ? 1 : 2);

                hasHit = true;

                if (hit) CleanUp();
            }
        }

        public void CleanUp()
        {
            Destroy(gameObject);
        }
    }
}