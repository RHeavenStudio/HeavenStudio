using HeavenStudio.Util;
using JetBrains.Annotations;
using Starpelly.Transformer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using static HeavenStudio.EntityTypes;

namespace HeavenStudio.Games
{
        public class RocketScript : PlayerActionObject
    {
        public string awakeAnim;
        public float startBeat;
        public RocketType type;
        public enum RocketType
        {
            Family,
            PartyCracker,
            Bell,
            Bowling
        }
        
        // Start is called before the first frame update
        void Awake()
        {
            PlayerActionEvent onHit;
            switch(type)
            {
                case RocketType.Family:
                    onHit = LaunchParty.instance.ScheduleInput(startBeat, 3f, InputType.STANDARD_DOWN, RocketFourSuccess, RocketFourMiss, RocketFourBlank);
                    break;
                case RocketType.PartyCracker:
                    onHit = LaunchParty.instance.ScheduleInput(startBeat, 2f, InputType.STANDARD_DOWN, RocketFiveSuccess, RocketFiveMiss, RocketFiveBlank);
                    break;
                case RocketType.Bell:
                    onHit = LaunchParty.instance.ScheduleInput(startBeat, 2f, InputType.STANDARD_DOWN, RocketSevenSuccess, RocketSevenMiss, RocketSevenBlank);
                    break;
                case RocketType.Bowling:
                    onHit = LaunchParty.instance.ScheduleInput(startBeat, 2f, InputType.STANDARD_DOWN, RocketOneSuccess, RocketOneMiss, RocketOneBlank);
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void RocketFourSuccess(PlayerActionEvent caller, float state)
        {   
            
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
                float randomNumber = UnityEngine.Random.Range(0, 10);
                if (randomNumber >= 5)
                    LaunchParty.instance.Rockets.Play("RocketBlank");
                else
                    LaunchParty.instance.Rockets.Play("RocketBlankRight");
            }
            else
            {
                Jukebox.PlayOneShotGame("launchParty/rocket_note", 0f, 1f);
                Jukebox.PlayOneShotGame("launchParty/rocket_family");
                LaunchParty.instance.Rockets.Play("RocketLaunch");
            }
                
            
                
            
        }

        public void RocketFourMiss(PlayerActionEvent caller)
        {
            float randomNumber = UnityEngine.Random.Range(0, 10);
            if (randomNumber >= 5)
                LaunchParty.instance.Rockets.Play("RocketMiss");
            
            else
                LaunchParty.instance.Rockets.Play("RocketMissRight");
            Jukebox.PlayOneShotGame("launchParty/miss");
            
        }

        public void RocketFourBlank(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            float randomNumber = UnityEngine.Random.Range(0, 10);
            if (randomNumber >= 5)
                LaunchParty.instance.Rockets.Play("RocketBlank");
            
            else
                LaunchParty.instance.Rockets.Play("RocketBlankRight");
        }

        public void RocketFiveSuccess(PlayerActionEvent caller, float state)
        {
            float finalpitch = 1f;
            Jukebox.PlayOneShotGame("launchParty/popper_note", 0f, finalpitch);
            Jukebox.PlayOneShotGame("launchParty/rocket_crackerblast");
            LaunchParty.instance.Crackers.Play("PopperLaunch");
        }

        public void RocketFiveMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            LaunchParty.instance.Crackers.Play("PopperMiss");
        }

        public void RocketFiveBlank(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            LaunchParty.instance.Crackers.Play("PopperMiss");
        }

        public void RocketSevenSuccess(PlayerActionEvent caller, float state)
        {
            float finalpitch = 1f;
            Jukebox.PlayOneShotGame("launchParty/bell_note", 0f, finalpitch);
            Jukebox.PlayOneShotGame("launchParty/bell_blast");
            LaunchParty.instance.Bells.Play("BellLaunch");
        }

        public void RocketSevenMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            LaunchParty.instance.Bells.Play("BellMiss");
        }

        public void RocketSevenBlank(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            LaunchParty.instance.Bells.Play("BellMiss");
        }

        public void RocketOneSuccess(PlayerActionEvent caller, float state)
        {
            float finalpitch = 1f;
            float finalpitch2 = 1f;
            Jukebox.PlayOneShotGame("launchParty/pin", 0f, finalpitch);
            Jukebox.PlayOneShotGame("launchParty/flute", 0f, finalpitch2);
            Jukebox.PlayOneShotGame("launchParty/rocket_bowling");
            LaunchParty.instance.Pins.Play("PinLaunch");
        }

        public void RocketOneMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
            LaunchParty.instance.Pins.Play("PinMiss");
        }

        public void RocketOneBlank(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/miss");
            LaunchParty.instance.Pins.Play("PinMiss");
        }

    }
}  
