using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlRocketLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            var e = eventCaller.currentEntity;
            return new Minigame("launchParty", "Launch Party \n<color=#eb5454>[WIP]</color>", "000000", false, false, new List<GameAction>()
            {
                new GameAction("rocket",                 delegate { LaunchParty.instance.RocketFour(e.beat, e.type); }, 4f, false, parameters: new List<Param>()
                {

                }),

                new GameAction("party popper",           delegate { LaunchParty.instance.RocketFive(e.beat, e.type); }, 3f, false, parameters: new List<Param>()
                {

                }),

                new GameAction("bell",                   delegate { LaunchParty.instance.RocketSeven(e.beat, e.type); }, 3f, false, parameters: new List<Param>()
                {

                }),

                new GameAction("bowling pin",           delegate { LaunchParty.instance.RocketOne(e.beat, e.type); }, 3f, false, parameters: new List<Param>()
                {

                }),

            });

        }
    }
}

namespace HeavenStudio.Games
{
    // using Scripts_LaunchParty;
    public class LaunchParty : Minigame
    {
        public static LaunchParty instance;

        [Header("Objects")]
        public Animator Rockets;
        public GameObject launchParty;
        public PlayerActionEvent padLaunch;

        [Header("Operator")]
        public bool launchingThisFrame;





        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            padLaunch = null;
            Rockets = launchParty.GetComponent<Animator>();
            launchingThisFrame = false;
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void RocketFour(float beat, int type)
        {
            if (padLaunch != null) return;

            var sound = new MultiSound.Sound[]
            {
                    new MultiSound.Sound("launchParty/VT_CL",   beat),
                    new MultiSound.Sound("launchParty/rocket_prepare",   beat),
                    new MultiSound.Sound("launchParty/VT_CL",   beat + 1f),
                    new MultiSound.Sound("launchParty/VT_CL",   beat + 2f),
            };


            BeatAction.New(launchParty, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { Rockets.Play("Rocket3", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { Rockets.Play("Rocket2", 0, 0);}),
                new BeatAction.Action(beat + 2f, delegate { Rockets.Play("Rocket1", 0, 0);})
            }
            );
            var SoundSource = MultiSound.Play(sound);
            padLaunch = ScheduleInput(beat, 3f, InputType.STANDARD_DOWN, RocketFourSuccess, RocketFourMiss, RocketFourMiss);
        }
       
        public void RocketFive(float beat, int type)
        {
            if (padLaunch != null) return;

            var sound = new MultiSound.Sound[]
            {
                    new MultiSound.Sound("launchParty/VT_CL",   beat),
                    new MultiSound.Sound("launchParty/rocket_prepare", beat),
                    new MultiSound.Sound("launchParty/VT_CL",   beat + 2/3f),
                    new MultiSound.Sound("launchParty/VT_CL",   beat + 1f),
                    new MultiSound.Sound("launchParty/VT_CL",   beat + 4/3f),
                    new MultiSound.Sound("launchParty/VT_CL",   beat + 5/3f),
            };
            var SoundSource = MultiSound.Play(sound);

            BeatAction.New(launchParty, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { Rockets.Play("Popper5", 0, 0); }),
                new BeatAction.Action(beat + 2/3f, delegate { Rockets.Play("Popper4", 0, 0);}),
                new BeatAction.Action(beat + 1f, delegate { Rockets.Play("Popper3", 0, 0);}),
                new BeatAction.Action(beat + 4/3f, delegate { Rockets.Play("Popper2", 0, 0);}),
                new BeatAction.Action(beat + 5/3f, delegate { Rockets.Play("Popper1", 0, 0);}),

            });
            
            padLaunch = ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, RocketFiveSuccess, RocketFiveMiss, RocketFiveMiss);
            
        }
        public void RocketSeven(float beat, int type)
        {
            if (padLaunch != null) return;
            var sound = new MultiSound.Sound[]
            {
                    new MultiSound.Sound("launchParty/VT_CL",   beat),
                    new MultiSound.Sound("launchParty/rocket_prepare", beat),
                    new MultiSound.Sound("launchParty/VT_CL",   beat + 1f),
                    new MultiSound.Sound("launchParty/VT_CL",   beat + 7/6f),
                    new MultiSound.Sound("launchParty/VT_CL",   beat + 8/6f),
                    new MultiSound.Sound("launchParty/VT_CL",   beat + 9/6f),
                    new MultiSound.Sound("launchParty/VT_CL",   beat + 10/6f),
                    new MultiSound.Sound("launchParty/VT_CL",   beat + 11/6f),
            };
            var SoundSource = MultiSound.Play(sound);
            BeatAction.New(launchParty, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { Rockets.Play("Bell7", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { Rockets.Play("Bell6", 0, 0);}),
                new BeatAction.Action(beat + 7/6f, delegate { Rockets.Play("Bell5", 0, 0);}),
                new BeatAction.Action(beat + 8/6f, delegate { Rockets.Play("Bell4", 0, 0);}),
                new BeatAction.Action(beat + 9/6f, delegate { Rockets.Play("Bell3", 0, 0);}),
                new BeatAction.Action(beat + 10/6f, delegate { Rockets.Play("Bell2", 0, 0);}),
                new BeatAction.Action(beat + 11/6f, delegate { Rockets.Play("Bell1", 0, 0);}),

            });
            padLaunch = ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, RocketSevenSuccess, RocketSevenMiss, RocketSevenMiss);

        }

        public void RocketOne(float beat, int type)
        {
            if (padLaunch != null) return;
            var sound = new MultiSound.Sound[]
            {
                new MultiSound.Sound("launchParty/VT_CL",   beat),
                new MultiSound.Sound("launchParty/rocket_prepare", beat),
                // new MultiSound.Sound("launchParty/flute",   beat),
                // new MultiSound.Sound("launchParty/flute",   beat + 1/6),
                // new MultiSound.Sound("launchParty/flute",   beat + 2/6f),
                // new MultiSound.Sound("launchParty/flute",   beat + 3/6f),
                // new MultiSound.Sound("launchParty/flute",   beat + 4/6f),
                // new MultiSound.Sound("launchParty/flute",   beat + 5/6f),
                // new MultiSound.Sound("launchParty/flute",   beat + 1f),
                // new MultiSound.Sound("launchParty/flute",   beat + 7/6f),
                // new MultiSound.Sound("launchParty/flute",   beat + 8/6f),
                // new MultiSound.Sound("launchParty/flute",   beat + 9/6f),
                // new MultiSound.Sound("launchParty/flute",   beat + 10/6f),
                // new MultiSound.Sound("launchParty/flute",   beat + 11/6f),

            };
            var SoundSource = MultiSound.Play(sound);
            BeatAction.New(launchParty, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { Rockets.Play("Pin1", 0, 0); }),
            });

            padLaunch = ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, RocketOneSuccess, RocketOneMiss, RocketOneMiss);
        }
        public void RocketFourSuccess(PlayerActionEvent caller, float state)
        {

            Jukebox.PlayOneShotGame("launchParty/VT_CL");
            Jukebox.PlayOneShotGame("launchParty/rocket_family");
            Rockets.Play("RocketLaunch");
            launchingThisFrame = true;


        }

        public void RocketFourMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
            Rockets.Play("RocketMiss");
            launchingThisFrame = true;
        }

        public void RocketFiveSuccess(PlayerActionEvent caller, float state)
            {

                Jukebox.PlayOneShotGame("launchParty/VT_CL");
                Jukebox.PlayOneShotGame("launchParty/rocket_crackerblast");
                Rockets.Play("PopperLaunch");
                launchingThisFrame = true;

        }

        public void RocketFiveMiss(PlayerActionEvent caller)
            {
                Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
                Rockets.Play("PopperMiss");
                launchingThisFrame = true;
        }
        public void RocketSevenSuccess(PlayerActionEvent caller, float state)
        {
            Jukebox.PlayOneShotGame("launchParty/VT_CL");
            Jukebox.PlayOneShotGame("launchParty/bell_blast");
            Rockets.Play("BellLaunch");
            launchingThisFrame = true;
        }

        public void RocketSevenMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
            Rockets.Play("BellMiss");
            launchingThisFrame = true;
        }

        public void RocketOneSuccess(PlayerActionEvent caller, float state)
        {
            Jukebox.PlayOneShotGame("launchParty/VT_CL");
            // Jukebox.PlayOneShotGame("launchParty/flute");
            Jukebox.PlayOneShotGame("launchParty/rocket_bowling");
            Rockets.Play("PinLaunch");
            launchingThisFrame = true;
        }
        public void RocketOneMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("launchParty/rocket_endBad");
            Rockets.Play("PinMiss");
            launchingThisFrame = true;
        }

    }

    }


