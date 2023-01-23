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
                if (spawnType != 3)
                    anim.Play("ClapFail", 0, 0);
                else
                    anim.Play("FaceClapFail", 0, 0);
            }
        }

        public void ScheduleClap(float beat, int type)
        {
            spawnType = type;
            Kitties.instance.ScheduleInput(beat, 2.5f, InputType.STANDARD_DOWN, ClapSuccessOne, ClapMissOne, ClapEmpty);
            Kitties.instance.ScheduleInput(beat, 3f, InputType.STANDARD_DOWN, ClapSuccessTwo, ClapMissTwo, ClapEmpty);
        }

        public void ScheduleRoll(float beat)
        {
            Kitties.instance.ScheduleInput(beat, 2f, InputType.STANDARD_ALT_DOWN, SpinSuccessOne, SpinMiss, SpinEmpty);
            Kitties.instance.ScheduleInput(beat, 2.75f, InputType.STANDARD_ALT_UP, SpinSuccessTwo, SpinMiss, SpinEmpty);
        }

        public void ClapSuccessOne(PlayerActionEvent Caller, float state)
        {
            if (spawnType != 3)
            {
                if (state >= 1f || state <= -1f)
                {  //todo: proper near miss feedback
                    Jukebox.PlayOneShotGame("kitties/ClapMiss1");
                    Jukebox.PlayOneShotGame("kitties/tink");
                    anim.Play("ClapMiss", 0, 0);
                }
                else
                {
                    Jukebox.PlayOneShotGame("kitties/clap1");
                    anim.Play("Clap1", 0, 0);
                }
            }
            else
            {
                if (state >= 1f || state <= -1f)
                {  //todo: proper near miss feedback
                    Jukebox.PlayOneShotGame("kitties/ClapMiss1");
                    Jukebox.PlayOneShotGame("kitties/tink");
                    anim.Play("FaceClapFail", 0, 0);
                }

                Jukebox.PlayOneShotGame("kitties/clap1");
                anim.Play("FaceClap", 0, 0);
            }
        }
        public void ClapMissOne(PlayerActionEvent Caller)
        {
            Jukebox.PlayOneShotGame("kitties/ClapMiss1");
        }
        public void ClapEmpty(PlayerActionEvent Caller)
        {

        }

        public void ClapSuccessTwo(PlayerActionEvent Caller, float state)
        {
            if (spawnType != 3)
            {
                if (state >= 1f || state <= -1f)
                {  //todo: proper near miss feedback
                    Jukebox.PlayOneShotGame("kitties/ClapMiss2");
                    Jukebox.PlayOneShotGame("kitties/tink");
                    anim.Play("ClapMiss", 0, 0);
                }
                else
                {
                    Jukebox.PlayOneShotGame("kitties/clap2");
                    anim.Play("Clap2", 0, 0);
                }
            }
            else
            {
                if (state >= 1f || state <= -1f)
                {  //todo: proper near miss feedback
                    Jukebox.PlayOneShotGame("kitties/ClapMiss2");
                    Jukebox.PlayOneShotGame("kitties/tink");
                    anim.Play("FaceClapFail", 0, 0);
                }

                Jukebox.PlayOneShotGame("kitties/clap2");
                anim.Play("FaceClap", 0, 0);
            }
        }

        public void ClapMissTwo(PlayerActionEvent Caller)
        {
            Jukebox.PlayOneShotGame("kitties/ClapMiss2");
        }

        public void SpinSuccessOne(PlayerActionEvent caller, float beat)
        {
            Jukebox.PlayOneShotGame("kitties/roll5");
            anim.Play("Rolling", 0, 0);
        }

        public void SpinSuccessTwo(PlayerActionEvent caller, float beat)
        {
            Jukebox.PlayOneShotGame("kitties/roll6");
        }

        public void SpinMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("miss");
        }

        public void SpinEmpty(PlayerActionEvent caller)
        {

        }
    }
}