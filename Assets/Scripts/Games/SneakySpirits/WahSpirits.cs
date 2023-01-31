using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NaughtyBezierCurves;

using HeavenStudio.Util;
using System;

namespace HeavenStudio.Games.Scripts_SneakySpirits
{
    public class WahSpirits : PlayerActionObject
    {
        public float startBeat;
        public int numBeats;

        // Start is called before the first frame update
        PlayerActionEvent OnHit;
        void Awake()
        {
            OnHit = SneakySpirits.instance.ScheduleInput(startBeat, numBeats, InputType.STANDARD_DOWN, SpiritHitOrNG, SpiritMiss, SpiritEmpty);
        }

        public void SpiritHitOrNG(PlayerActionEvent caller, float state)
        {
            if (GameManager.instance.currentGame != "sneakySpirits") return;
            //hitting a normal object with the alt input
            //WHEN SCORING THIS IS A MISS
            if (state <= -1f || state >= 1f)
            {
                startBeat = Conductor.instance.songPositionInBeats;
                Jukebox.PlayOneShot("miss");
                SpiritNG();
            }
            else
            {
                SpiritHit();
            }
            OnHit.CanHit(false);
        }

        public void SpiritMiss(PlayerActionEvent caller)
        {
            Animator mobjAnim = GetComponent<Animator>();
            mobjAnim.Play("Miss", -1, 0);
        }

        public void SpiritNG()
        {
            Animator mobjAnim = GetComponent<Animator>();
            //mobjAnim.Play("Hit", -1, 0);
        }

        public void SpiritHit()
        {
            Animator mobjAnim = GetComponent<Animator>();
            mobjAnim.Play("Hit", -1, 0);
        }

        public void SpiritEmpty(PlayerActionEvent caller) { }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
