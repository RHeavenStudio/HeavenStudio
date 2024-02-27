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

        private bool hasSpun = false;
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

            if (PlayerInput.GetIsAction(Kitties.InputAction_BasicPress) && canClap && !Kitties.instance.IsExpectingInputNow(Kitties.InputAction_BasicPress))
            {
                if (PlayerInput.CurrentControlStyle == InputSystem.InputController.ControlStyles.Touch && !Kitties.instance.IsExpectingInputNow(Kitties.InputAction_AltStart))
                {
                    SoundByte.PlayOneShot("miss");
                    if (spawnType != 3)
                        anim.Play("ClapFail", 0, 0);
                    else
                        anim.Play("FaceClapFail", 0, 0);
                }
            }

            if ((PlayerInput.GetIsAction(Kitties.InputAction_AltStart) && canClap && !(Kitties.instance.IsExpectingInputNow(Kitties.InputAction_AltStart) || Kitties.instance.IsExpectingInputNow(Kitties.InputAction_BasicPress)))
                || (PlayerInput.GetIsAction(Kitties.InputAction_AltFinish) && canClap && !Kitties.instance.IsExpectingInputNow(Kitties.InputAction_AltFinish) && hasSpun)
                || ((PlayerInput.GetIsAction(Kitties.InputAction_TouchRelease) && !PlayerInput.GetIsAction(Kitties.InputAction_AltFinish)) && canClap && hasSpun))
            {
                RollFail();
            }
        }

        public void ScheduleClap(double beat, int type)
        {
            spawnType = type;
            Kitties.instance.ScheduleInput(beat, 2.5f, Kitties.InputAction_BasicPress, ClapSuccessOne, ClapMissOne, ClapEmpty);
            Kitties.instance.ScheduleInput(beat, 3f, Kitties.InputAction_BasicPress, ClapSuccessTwo, ClapMissTwo, ClapEmpty);
        }

        public void ScheduleRoll(double beat)
        {
                Kitties.instance.ScheduleInput(beat, 2f, Kitties.InputAction_AltStart, SpinSuccessOne, SpinMissOne, SpinEmpty);
        }

        public void ScheduleRollFinish(double beat)
        {
            if (hasSpun)
                Kitties.instance.ScheduleInput(beat, 2.75f, Kitties.InputAction_AltFinish, SpinSuccessTwo, SpinMissTwo, SpinEmpty);
        }

        public void ScheduleFish(double beat)
        {
            Kitties.instance.ScheduleInput(beat, 2.75f, Kitties.InputAction_BasicPress, FishSuccess, FishMiss, FishEmpty);
        }

        public void ClapSuccessOne(PlayerActionEvent Caller, float state)
        {
            if (spawnType != 3)
            {
                if (state >= 1f || state <= -1f)
                {  //todo: proper near miss feedback
                    SoundByte.PlayOneShotGame("kitties/ClapMiss1");
                    SoundByte.PlayOneShotGame("kitties/tink");
                    anim.Play("ClapMiss", 0, 0);
                }
                else
                {
                    SoundByte.PlayOneShotGame("kitties/clap1");
                    anim.Play("Clap1", 0, 0);
                }
            }
            else
            {
                if (state >= 1f || state <= -1f)
                {  //todo: proper near miss feedback
                    SoundByte.PlayOneShotGame("kitties/ClapMiss1");
                    SoundByte.PlayOneShotGame("kitties/tink");
                    anim.Play("FaceClapFail", 0, 0);
                }

                SoundByte.PlayOneShotGame("kitties/clap1");
                anim.Play("FaceClap", 0, 0);
            }
        }
        public void ClapMissOne(PlayerActionEvent Caller)
        {
            SoundByte.PlayOneShotGame("kitties/ClapMiss1");
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
                    SoundByte.PlayOneShotGame("kitties/ClapMiss2");
                    SoundByte.PlayOneShotGame("kitties/tink");
                    anim.Play("ClapMiss", 0, 0);
                }
                else
                {
                    SoundByte.PlayOneShotGame("kitties/clap2");
                    anim.Play("Clap2", 0, 0);
                }
            }
            else
            {
                if (state >= 1f || state <= -1f)
                {  //todo: proper near miss feedback
                    SoundByte.PlayOneShotGame("kitties/ClapMiss2");
                    SoundByte.PlayOneShotGame("kitties/tink");
                    anim.Play("FaceClapFail", 0, 0);
                }

                SoundByte.PlayOneShotGame("kitties/clap2");
                anim.Play("FaceClap", 0, 0);
            }
        }

        public void ClapMissTwo(PlayerActionEvent Caller)
        {
            SoundByte.PlayOneShotGame("kitties/ClapMiss2");
        }

        public void SpinSuccessOne(PlayerActionEvent caller, float beat)
        {
            hasSpun = true;
            SoundByte.PlayOneShotGame("kitties/roll5");
            anim.Play("Rolling", 0, 0);
            ScheduleRollFinish(caller.startBeat);
        }

        public void SpinSuccessTwo(PlayerActionEvent caller, float beat)
        {
            SoundByte.PlayOneShotGame("kitties/roll6");
            anim.Play("RollEnd", 0, 0);
            hasSpun = false;
        }

        public void SpinMissOne(PlayerActionEvent caller)
        {
            hasSpun = false;
            var cond = Conductor.instance;
            SoundByte.PlayOneShotGame("kitties/roll5", -1f, 1, .1f);
            SoundByte.PlayOneShotGame("kitties/roll6", cond.songPositionInBeatsAsDouble + .75f, 1, .1f);
        }

        public void SpinMissTwo(PlayerActionEvent caller)
        {
            if (hasSpun)
            {
                RollFail();
            }
            SoundByte.PlayOneShotGame("kitties/roll6", -1f, 1, .3f);
        }

        public void SpinEmpty(PlayerActionEvent caller)
        {

        }

        public void RollFail()
        {
            SoundByte.PlayOneShot("miss");
            anim.Play("RollFail", 0, 0);
            hasSpun = false;
        }

        public void FishSuccess(PlayerActionEvent caller, float beat)
        {
            Kitties.instance.RemoveCats(false);
            SoundByte.PlayOneShotGame("kitties/fish4");
            fish.Play("CaughtSuccess", 0, 0);
        }

        public void FishMiss(PlayerActionEvent caller)
        {
            Kitties.instance.RemoveCats(false);
            SoundByte.PlayOneShot("miss");
            fish.Play("CaughtFail", 0, 0);
        }

        public void FishEmpty(PlayerActionEvent caller)
        {

        }
    }
}