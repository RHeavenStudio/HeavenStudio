using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Starpelly;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MrUpbeat
{
    public class UpbeatStep : PlayerActionObject
    {
        public float startBeat;
        private bool passedFirst = false;
        public float beatOffset = 0;

        private void Start()
        {
            PlayerActionInit(gameObject, startBeat);
        }

        public override void OnAce()
        {
            Hit(true, true);
        }

        private void Update()
        {
            if (Conductor.instance.GetPositionFromBeat(startBeat, 0.35f) >= 1 && !passedFirst)
            {
                if(MrUpbeat.instance.man.stepTimes % 2 != Math.Round(startBeat - beatOffset) % 2)
                    Hit(false);
                passedFirst = true;
            }
            if (Conductor.instance.GetPositionFromBeat(startBeat, 0.65f) >= 1)
                Hit(false);

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, 0.5f);
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

        public void Hit(bool hit, bool force = false)
        {
            if (force) MrUpbeat.instance.man.Step();
            else if (!hit) MrUpbeat.instance.man.Fall();

            CleanUp();
        }

        public void CleanUp()
        {
            Destroy(this.gameObject);
        }

    }
}