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
                    function = delegate { var e = eventCaller.currentEntity; HoleInOne.instance.ToggleBop(e.beat, e.length, e["bop"], e["autoBop"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Toggle if the characters should bop for the duration of this event."),
                        new Param("autoBop", false, "Bop (Auto)", "Toggle if the characters should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("mandrill", "Mandrill")
                {
                    function = delegate { HoleInOne.instance.DoMandrill(eventCaller.currentEntity.beat); },
                    defaultLength = 4.0f,
                },
                new GameAction("monkey", "Monkey")
                {
                    function = delegate { HoleInOne.instance.DoMonkey(eventCaller.currentEntity.beat); },
                    defaultLength = 3.0f,
                },
                // new GameAction("whale", "Whale")
                // {
                //     function = delegate { var e = eventCaller.currentEntity;  HoleInOne.instance.Whale(e.beat, e.length, e["ease"], e["appear"]); },
                //     parameters = new List<Param>()
                //     {
                //         new Param("appear", true, "Enter", "Toggle if the whale should enter or exit the scene."),
                //         new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                //     },
                //     resizable = true
                // },
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
        public Animator MonkeyHeadAnim;
        public Animator MandrillAnim;
        public Animator GolferAnim;
        public Animator HoleAnim;
        public Animator GrassEffectAnim;
        public Animator BallEffectAnim;

        double whaleStartBeat;
        float whaleLength;
        Util.EasingFunction.Ease lastEase;
        bool isWhale;

        public static HoleInOne instance;

        void Awake()
        {
            HoleInOne.instance = this;
            SetupBopRegion("holeInOne", "bop", "autoBop");
            isWhale = false;
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat)) 
            {
                MonkeyAnim.DoScaledAnimationAsync("MonkeyBop", 0.3f);
                MandrillAnim.DoScaledAnimationAsync("MandrillBop", 0.3f);
                GolferAnim.DoScaledAnimationAsync("GolferBop", 0.3f);            
            }

        }

        void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                SoundByte.PlayOneShotGame("holeInOne/hole1"); // temp miss sound. Should be: whiff sound
                GolferAnim.Play("GolferWhiff");
                ScoreMiss();
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
                            MonkeyAnim.DoScaledAnimationAsync("MonkeyBop", 0.3f);
                            MandrillAnim.DoScaledAnimationAsync("MandrillBop", 0.3f);
                            GolferAnim.DoScaledAnimationAsync("GolferBop", 0.3f);                            
                            // TODO add bops for other characters
                        })
                    });
                }
            }
        }
    
        public void Whale(double beat, float length, int ease, bool appear)
        {
            SoundByte.PlayOneShotGame("clappyTrio/sign");
            whaleStartBeat = beat;
            whaleLength = length;
            lastEase = (Util.EasingFunction.Ease)ease;
            isWhale = appear;
        }

        public void DoMandrill(double beat)
        {
            //Mandrill Multisound
            ScheduleInput(beat, 3f, InputAction_FlickPress, MandrillSuccess, MandrillMiss, Nothing);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("holeInOne/mandrill1", beat),
                new MultiSound.Sound("holeInOne/mandrill2", beat + 1f),
                new MultiSound.Sound("holeInOne/mandrill3", beat + 2f),
                new MultiSound.Sound("holeInOne/hole1", beat + 3f),
            });

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat,      delegate { MandrillAnim.DoScaledAnimationAsync("MandrillReady1", 0.2f);}),
                new BeatAction.Action(beat + 1f, delegate { MandrillAnim.DoScaledAnimationAsync("MandrillReady2", 0.3f);}),
                new BeatAction.Action(beat + 2f, delegate { MandrillAnim.DoScaledAnimationAsync("MandrillReady3", 0.3f);}),
                new BeatAction.Action(beat + 2f, delegate { GolferAnim.DoScaledAnimationAsync("GolferPrepare", 1.0f);}),
                new BeatAction.Action(beat + 3f, delegate { GolferAnim.DoScaledAnimationAsync("GolferThrough", 1.0f);}),
                new BeatAction.Action(beat + 3f, delegate { MandrillAnim.DoScaledAnimationAsync("MandrillPitch", 0.4f);}),
                new BeatAction.Action(beat + 3f, delegate { MonkeyAnim.DoScaledAnimationAsync("MonkeySpin", 2.0f);}),

            });
        }

        public void DoMonkey(double beat)
        {
            //Monkey Multisound
            ScheduleInput(beat, 2f, InputAction_FlickPress, MonkeySuccess, MonkeyMiss, Nothing);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("holeInOne/monkey1", beat),
                new MultiSound.Sound("holeInOne/monkey2", beat + 1f)
            });

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat,      delegate { MonkeyAnim.Play("MonkeyPrepare");}),
                new BeatAction.Action(beat + 1f, delegate { MonkeyAnim.Play("MonkeyThrow");}),
                new BeatAction.Action(beat + 1f, delegate { GolferAnim.DoScaledAnimationAsync("GolferPrepare", 1.0f);}),
                new BeatAction.Action(beat + 2f, delegate { GolferAnim.DoScaledAnimationAsync("GolferThrough", 1.0f);}),                
            });

        }

        public void MandrillSuccess(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                double beat = caller.startBeat + caller.timer;

                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("holeInOne/mandrill1", beat),// temp should be miss
                    new MultiSound.Sound("holeInOne/hole2", beat + 1f)// temp should be splash
                });
                GolferAnim.DoScaledAnimationAsync("GolferMiss", 1.0f);
                BallEffectAnim.Play("BallEffectJust");
            }
            else
            {
                double beat = caller.startBeat + caller.timer;

                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("holeInOne/mandrill4", beat),
                    new MultiSound.Sound((isWhale) ? "holeInOne/whale" : "holeInOne/hole4", beat + 2f)
                });

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat,      delegate { GolferAnim.DoScaledAnimationAsync("GolferJust", 1.0f);}),
                    new BeatAction.Action(beat,      delegate { BallEffectAnim.Play("BallEffectJust");}),
                    new BeatAction.Action(beat + 2f, delegate { HoleAnim.DoScaledAnimationAsync("ZoomBig", 2.5f);}),
                });
            }
            
        }

        public void MandrillMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("holeInOne/whale");
            MonkeyAnim.Play("MonkeySpin");
            BallEffectAnim.Play("BallEffectThrough");
            GolferAnim.Play("GolferThroughMandrill");       
        }

        public void MonkeySuccess(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                double beat = caller.startBeat + caller.timer;

                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("holeInOne/mandrill1", beat),// temp should be miss
                    new MultiSound.Sound("holeInOne/hole2", beat + 1f)// temp should be splash
                });
                MonkeyHeadAnim.DoScaledAnimationAsync("MonkeyMissHead", 2.0f);
                GolferAnim.DoScaledAnimationAsync("GolferMiss", 1.0f);
            }
            else
            {
                double beat = caller.startBeat + caller.timer;
                int randomSuccess = UnityEngine.Random.Range(1,5);

                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("holeInOne/monkey3", beat),
                    new MultiSound.Sound((isWhale) ? "holeInOne/whale" : ("holeInOne/hole" + randomSuccess), beat + 2f)
                });

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat,      delegate { MonkeyHeadAnim.DoScaledAnimationAsync("MonkeyJustHead", 3.5f);}),
                    new BeatAction.Action(beat,      delegate { GolferAnim.DoScaledAnimationAsync("GolferJust", 1.0f);}),
                    new BeatAction.Action(beat + 2f, delegate { HoleAnim.DoScaledAnimationAsync("ZoomSmall" + randomSuccess, 2.5f);}),
                });
            }
        }

        public void MonkeyMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("holeInOne/whale");
            MonkeyHeadAnim.DoScaledAnimationAsync("MonkeySadHead", 0.2f);
        }

        public void Nothing(PlayerActionEvent caller)
        {
        }
    }
}