using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_Kitties
{
    public class CtrTeppanPlayer : MonoBehaviour
    {
        [Header("Objects")]
        public GameObject Player;
        public Animator anim;
        private int spawnType;

        private bool hasClapped = false;
        public bool canClap = false;

        private bool hasSpun = false;
        private bool canSpin = true;

        private bool hasFish = false;
        private bool canFish = false;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;

            if (PlayerInput.Pressed() && canClap && !Kitties.instance.IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                Jukebox.PlayOneShot("miss");
                anim.Play("ClapFail", 0, 0);
            }
        }

        public void ScheduleClap(float beat)
        {
            Kitties.instance.ScheduleInput(beat, 2.5f, InputType.STANDARD_DOWN, ClapSuccessOne, ClapMissOne, ClapEmpty);
            Kitties.instance.ScheduleInput(beat, 3f, InputType.STANDARD_DOWN, ClapSuccessTwo, ClapMissTwo, ClapEmpty);
        }

        public void ClapSuccessOne(PlayerActionEvent Caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {  //todo: proper near miss feedback
                Jukebox.PlayOneShotGame("kitties/ClapMiss1");
                Jukebox.PlayOneShotGame("kitties/tink");
                anim.Play("Clap1", 0, 0);
            }
            else
            {
                Jukebox.PlayOneShotGame("kitties/clap1");
                anim.Play("Clap1", 0, 0);
            }
        }
        public void ClapMissOne(PlayerActionEvent Caller)
        {
            Jukebox.PlayOneShotGame("kitties/ClapMiss1");
            anim.Play("ClapMiss", 0, 0);
        }
        public void ClapEmpty(PlayerActionEvent Caller)
        {

        }

        public void ClapSuccessTwo(PlayerActionEvent Caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {  //todo: proper near miss feedback
                Jukebox.PlayOneShotGame("kitties/ClapMiss2");
                Jukebox.PlayOneShotGame("kitties/tink");
                anim.Play("Clap1", 0, 0);
            }
            else
            {
                Jukebox.PlayOneShotGame("kitties/clap2");
                anim.Play("Clap2", 0, 0);
            }
        }

        public void ClapMissTwo(PlayerActionEvent Caller)
        {
            Jukebox.PlayOneShotGame("kitties/ClapMiss2");
            anim.Play("ClapMiss", 0, 0);
        }
    }
}