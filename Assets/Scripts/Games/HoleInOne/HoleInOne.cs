using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlHoleInOneLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("holeInOne", "Hole in One", "6ab99e", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; HoleInOne.instance.ToggleBop(e.beat, e.length, e["bop"], e["autobop"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Toggle if the characters should bop for the duration of this event."),
                        new Param("autobop", false, "Bop (Auto)", "Toggle if the characters should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("mandrill", "Mandrill (no visuals)")
                {
                    function = delegate { HoleInOne.instance.DoMandrill(eventCaller.currentEntity.beat); },
                    defaultLength = 4.0f,
                },
                new GameAction("monkey", "Monkey")
                {
                    function = delegate { HoleInOne.instance.DoMonkey(eventCaller.currentEntity.beat); },
                    defaultLength = 3.0f,
                }
            }//,
            // new List<string>() { "rvl", "normal" },
            // "rvlgolf", "en",
            // new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class HoleInOne : Minigame
    {
        public Animator MonkeyAnim;
        public Animator MandrillAnim;
        public Animator GolferAnim;

        public static HoleInOne instance;

        void Awake()
        {
            HoleInOne.instance = this;
            SetupBopRegion("holeInOne", "bop", "toggle");
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat)) 
            {
                MonkeyAnim.DoScaledAnimationAsync("MonkeyBop", 0.4f);
                MandrillAnim.DoScaledAnimationAsync("MandrillBop", 0.4f);
                GolferAnim.DoScaledAnimationAsync("GolferBop", 0.4f);            
            }

        }

        public void ToggleBop(double beat, float length, bool shouldBop, bool autoBop)
        {
            if (shouldBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            MonkeyAnim.DoScaledAnimationAsync("MonkeyBop", 0.4f);
                            MandrillAnim.DoScaledAnimationAsync("MandrillBop", 0.4f);
                            GolferAnim.DoScaledAnimationAsync("GolferBop", 0.4f);                            
                            // TODO add bops for other characters
                        })
                    });
                }
            }
        }

        public void DoMandrill(double beat)
        {
            //Mandrill Multisound
            ScheduleInput(beat, 3f, InputAction_FlickPress, MandrillSuccess, MandrillMiss, Whiff);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("holeInOne/mandrill1", beat),
                new MultiSound.Sound("holeInOne/mandrill2", beat + 1f),
                new MultiSound.Sound("holeInOne/mandrill3", beat + 2f),
                new MultiSound.Sound("holeInOne/hole1", beat + 3f),
            });

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat,            delegate { MandrillAnim.Play("MandrillReady1");}),
                new BeatAction.Action(beat + 1.0f,     delegate { MandrillAnim.Play("MandrillReady2");}),
                new BeatAction.Action(beat + 2.0f,     delegate { MandrillAnim.Play("MandrillReady3");}),
                new BeatAction.Action(beat + 3.0f,     delegate { MandrillAnim.Play("MandrillPitch");}),   
            });
        }

        public void DoMonkey(double beat)
        {
            //Monkey Multisound
            ScheduleInput(beat, 2f, InputAction_FlickPress, MonkeySuccess, MonkeyMiss, Whiff);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("holeInOne/monkey1", beat),
                new MultiSound.Sound("holeInOne/monkey2", beat + 1f)
            });

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat,            delegate { MonkeyAnim.Play("MonkeyPrepare");}),
                new BeatAction.Action(beat + 1.0f,     delegate { MonkeyAnim.Play("MonkeyThrow");}),             
            });

        }

        public void MandrillSuccess(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("holeInOne/mandrill4");
            MonkeyAnim.Play("MonkeySpin");
            
        }

        public void MandrillMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("holeInOne/whale");
            MonkeyAnim.Play("MonkeySpin");            
        }

        public void MonkeySuccess(PlayerActionEvent caller, float state)
        {
            double beat = caller.timer;
            int randomSuccess = UnityEngine.Random.Range(1,5);

            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("holeInOne/monkey3", beat),
                new MultiSound.Sound("holeInOne/hole" + randomSuccess, beat + 2f)
            });
            MonkeyAnim.Play("MonkeySpin");
        }

        public void MonkeyMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("holeInOne/whale");
            MonkeyAnim.Play("MonkeySpin");            
        }

        public void Whiff(PlayerActionEvent caller)
        {
            MonkeyAnim.Play("MonkeyBop");
        }
    }
}