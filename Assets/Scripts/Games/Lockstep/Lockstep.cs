/* I do not know crap about Unity or C#
Almost none of this code is mine, but it's all fair game when the game you're stealing from
borrowed from other games */

using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrBackbeatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("lockstep", "Lockstep \n<color=#eb5454>[WIP]</color>", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                    {
                        function = delegate { var e = eventCaller.currentEntity; Lockstep.instance.Bop(e.beat, e["toggle"]); },
                        parameters = new List<Param>()
                        {
                        new Param("toggle", false, "Reset Pose", "Resets to idle pose.")
                        },
                        defaultLength = 1f,
                    },

                

                new GameAction("hai", "Hai!")
                    {
                        function = delegate { var e = eventCaller.currentEntity; Lockstep.instance.Hai(e.beat); },
                        defaultLength = 1f,
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; Lockstep.instance.Hai(e.beat);}


                    },

                new GameAction("offbeatSwitch", "Switch to Offbeat")
                    {
                        function = delegate { var e = eventCaller.currentEntity; Lockstep.instance.OnbeatSwitch(e.beat); },
                        defaultLength = 8f


                    },

                new GameAction("onbeatSwitch", "Switch to Onbeat")
                    {
                        function = delegate { var e = eventCaller.currentEntity; Lockstep.instance.OffbeatSwitch(e.beat); },
                        defaultLength = 2f


                    },

                new GameAction("marching", "Onbeat Stepping")
                    {
                        function = delegate { var e = eventCaller.currentEntity; Lockstep.instance.OnbeatStep(e.beat, e.length); },
                        defaultLength = 4f,
                        resizable = true,
                        hidden = true
                    },

                new GameAction("startStepping", "Start Stepping")
                    {
                        function = delegate { var e = eventCaller.currentEntity; Lockstep.instance.BeginStepping(e.beat); },
                        defaultLength = 1f,
                        hidden = true

                    },

                new GameAction("test1", "onbeat march test")
                    {
                        function = delegate { var e = eventCaller.currentEntity; Lockstep.instance.OnbeatMarch(e.beat); },
                        defaultLength = 1f,
                        hidden = true
                    }

            });

        }
    }
}

namespace HeavenStudio.Games
{
   // using Scripts_Lockstep;
    public class Lockstep : Minigame
    {


        //   private Animator stepswitcher;
        

        public Animator stepswitcherP;
        public Animator stepswitcher0;
        public Animator stepswitcher1;

        public GameObject Player;


        [Header("Properties")]
        public GameEvent bop = new GameEvent();
        public bool goStep;

        public float steppingLength;
        public float steppingStartBeat;
        private float lastReportedBeat = 0f;


        public static Lockstep instance { get; set; }


        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            goStep = false;
        }

        // Update is called once per frame
        public void Update()
        {

            var cond = Conductor.instance;

            if (goStep)
            {
                print("stepping is on");
                if (Conductor.instance.ReportBeat(ref lastReportedBeat))
                {
                    print("one small step for switch");
                    Jukebox.PlayOneShotGame("Lockstep/marchOnBeat1");
                    stepswitcherP.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
                    stepswitcher0.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
                    stepswitcher1.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
                }

            }


            if (PlayerInput.Pressed() && !IsExpectingInputNow())
            {
                //Jukebox.PlayOneShot("miss");

                
                var beatAnimCheck = Math.Round(Conductor.instance.songPositionInBeats * 2);
                print("check: " + beatAnimCheck);
                var stepPlayerAnim = (beatAnimCheck % 2 != 0 ? "OffbeatMarch" : "OnbeatMarch");

                Jukebox.PlayOneShotGame("lockstep/miss");
                stepswitcherP.DoScaledAnimationAsync(stepPlayerAnim, 0.5f);
            }


        }

        public void Bop(float beat, bool reset)
        {
            
            if(reset)
            {
                stepswitcher0.DoScaledAnimationAsync("BopReset", 0.5f);
                stepswitcher1.DoScaledAnimationAsync("BopReset", 0.5f);
                stepswitcherP.DoScaledAnimationAsync("BopReset", 0.5f);

            }
            else
            {
                stepswitcher0.DoScaledAnimationAsync("Bop", 0.5f);
                stepswitcher1.DoScaledAnimationAsync("Bop", 0.5f);
                stepswitcherP.DoScaledAnimationAsync("Bop", 0.5f);

            }
            



        }

        public void Hai(float beat)
        {
            Jukebox.PlayOneShotGame("lockstep/switch1");
        }

        public void BeginStepping(float beat)
        {

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { goStep = true; }),
                });

            print("Start Stepping");
            print(goStep);
        }


        public void OnbeatSwitch(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
                {
                new MultiSound.Sound("lockstep/switch1", beat),
                new MultiSound.Sound("lockstep/switch1", beat + 1f),
                new MultiSound.Sound("lockstep/switch1", beat + 2f),
                new MultiSound.Sound("lockstep/switch2", beat + 3f),
                new MultiSound.Sound("lockstep/switch3", beat + 3.5f),

                new MultiSound.Sound("lockstep/switch4", beat + 4.5f),
                new MultiSound.Sound("lockstep/switch4", beat + 5.5f),
                new MultiSound.Sound("lockstep/switch4", beat + 6.5f),
                new MultiSound.Sound("lockstep/switch4", beat + 7.5f),
                }, forcePlay: false);
        }

        public void OffbeatSwitch(float beat)
        {
            var sound = new MultiSound.Sound[]
                {
                    new MultiSound.Sound("lockstep/switch5", beat),
                    new MultiSound.Sound("lockstep/switch6", beat + 0.5f),
                    new MultiSound.Sound("lockstep/switch5", beat + 1f),
                    new MultiSound.Sound("lockstep/switch6", beat + 1.5f)
                };


            MultiSound.Play(sound);


        }

        public void OnbeatStep(float beat, float length)
        {
            /*marching.length = length;
            marching.startBeat = beat;
            print("onbeatstep len: " + marching.length);
            print("onbeatstep start: " + marching.startBeat);*/
        }

        public void OnbeatMarch(float beat)
        {

            stepswitcher0.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
            stepswitcher1.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
            stepswitcherP.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
            Jukebox.PlayOneShotGame("lockstep/marchOnbeat1");



        }

    }
}
