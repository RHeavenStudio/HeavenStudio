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
        public Animator fish;
        private int spawnType;

        private bool hasClapped = false;
        public bool canClap = false;

        private bool hasSpun = true;
        private bool checkSpin = false;

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

            if (PlayerInput.AltPressed() && canClap && !Kitties.instance.IsExpectingInputNow(InputType.STANDARD_ALT_DOWN)
                || PlayerInput.AltPressedUp() && canClap && !Kitties.instance.IsExpectingInputNow(InputType.STANDARD_ALT_UP) && hasSpun)
            {
                RollFail();
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
                Kitties.instance.ScheduleInput(beat, 2f, InputType.STANDARD_ALT_DOWN, SpinSuccessOne, SpinMissOne, SpinEmpty);
        }

        public void ScheduleRollFinish(float beat)
        {
            Debug.Log(hasSpun);
            if (hasSpun)
                Kitties.instance.ScheduleInput(beat, 2.75f, InputType.STANDARD_ALT_UP, SpinSuccessTwo, SpinMissTwo, SpinEmpty);
        }

        public void ScheduleFish(float beat)
        {
            Kitties.instance.ScheduleInput(beat, 2.75f, InputType.STANDARD_DOWN, FishSuccess, FishMiss, FishEmpty);
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
            hasSpun = true;
            Jukebox.PlayOneShotGame("kitties/roll5");
            anim.Play("Rolling", 0, 0);
        }

        public void SpinSuccessTwo(PlayerActionEvent caller, float beat)
        {
            Jukebox.PlayOneShotGame("kitties/roll6");
            anim.Play("RollEnd", 0, 0);
        }

        public void SpinMissOne(PlayerActionEvent caller)
        {
            hasSpun = false;
            var cond = Conductor.instance;
            Jukebox.PlayOneShotGame("kitties/roll5", -1f, 1, .1f);
            Jukebox.PlayOneShotGame("kitties/roll6", cond.songPositionInBeats + .75f, 1, .1f);
        }

        public void SpinMissTwo(PlayerActionEvent caller)
        {
            if (hasSpun)
            {
                RollFail();
            }
            Jukebox.PlayOneShotGame("kitties/roll6", -1f, 1, .3f);
        }

        public void SpinEmpty(PlayerActionEvent caller)
        {

        }

        public void RollFail()
        {
            Jukebox.PlayOneShot("miss");
            anim.Play("RollFail", 0, 0);
            hasSpun = false;
        }

        public void FishSuccess(PlayerActionEvent caller, float beat)
        {
            Kitties.instance.RemoveCats(false);
            Jukebox.PlayOneShotGame("kitties/fish4");
            fish.Play("CaughtSuccess", 0, 0);
        }

        public void FishMiss(PlayerActionEvent caller)
        {
            Kitties.instance.RemoveCats(false);
            Jukebox.PlayOneShot("miss");
            fish.Play("CaughtFail", 0, 0);
        }

        public void FishEmpty(PlayerActionEvent caller)
        {

        }
    }
}