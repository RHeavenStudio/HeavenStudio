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
    /// Minigame loaders handle the setup of your minigame.
    /// Here, you designate the game prefab, define entities, and mark what AssetBundle to load

    /// Names of minigame loaders follow a specific naming convention of `PlatformcodeNameLoader`, where:
    /// `Platformcode` is a three-leter platform code with the minigame's origin
    /// `Name` is a short internal name
    /// `Loader` is the string "Loader"

    /// Platform codes are as follows:
    /// Agb: Gameboy Advance    ("Advance Gameboy")
    /// Ntr: Nintendo DS        ("Nitro")
    /// Rvl: Nintendo Wii       ("Revolution")
    /// Ctr: Nintendo 3DS       ("Centrair")
    /// Mob: Mobile
    /// Pco: PC / Other

    /// Fill in the loader class label, "*prefab name*", and "*Display Name*" with the relevant information
    /// For help, feel free to reach out to us on our discord, in the #development channel.
    public static class AgbRhythmTestGBALoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("rhythmTestGBA", "Rhythm Test (GBA) \n<color=#adadad>(Rhythm-kan Check)</color>", "ffffff", false, false, new List<GameAction>()
            {

                new GameAction("countin", "Start Beeping")
                {
                    function = delegate { RhythmTestGBA.instance.KeepTheBeep(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["toggle"], eventCaller.currentEntity["auto"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Toggle", "Toggle the automatic beeping on or off."),
                        new Param("auto", false, "Auto", "Toggle if the machine should beep automatically."),
                    },
                },

                new GameAction("button", "Start Keep-the-Beat")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmTestGBA.StartKeepbeat(e.beat); },
                    defaultLength = 1f,
                    resizable = false,

                },

                new GameAction("stopktb", "Stop Keep-the-Beat")
                {
                    preFunction = delegate { RhythmTestGBA.instance.PreStopKeepbeat(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["mutecue"]); },
                    defaultLength = 4f,
                    resizable = false,
                    parameters = new List<Param>()
                    {
                        new Param("mutecue", false, "Mute Cue", "Mute the sound cue signifying the end of the keep-the-beat section."),
                    }
                },

                new GameAction("countdown", "Countdown")
                {
                    preFunction = delegate {RhythmTestGBA.instance.PreCountDown(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["val1"]);},
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("val1", new EntityTypes.Integer(1, 9, 3), "Beats", "Set how many beats there will be before the player has to input.")                        
                    }
                },

                new GameAction("hidecount", "Toggle Countdown")
                {
                    function = delegate {RhythmTestGBA.instance.HideCountdown(eventCaller.currentEntity["togglecount"]);},
                    defaultLength = 0.5f,
                    resizable = false,
                    parameters = new List<Param>()
                    {
                        new Param("togglecount", true, "Toggle Countdown?", "Toggles whether the countdown appears or not."),
                    }

                },

            },
            new List<string>() {"abg", "aim"},
            "agbRhythmTestGBA", "en",
            new List<string>() { "en" }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class RhythmTestGBA : Minigame
    {
        public static RhythmTestGBA instance;
        static List<double> queuedButton = new();
        
        bool goBeep;
        bool stopBeep;
        bool keepPressing;
        bool shouldmute;
        bool disableCount;

        private double numberSelect;
        private float countLength;

        [Header("Animators")]
        [SerializeField] Animator buttonAnimator;
        [SerializeField] Animator flashAnimator;
        [SerializeField] Animator numberBGAnimator;
        [SerializeField] Animator numberAnimator;

        [Header("Properties")]
        private static double startBlippingBeat = double.MaxValue;

        [Header("Variables")]
        
        int pressPlayerCount;
        public static double wantButton = double.MinValue;

        GameEvent button = new GameEvent();

        double lastButton = double.MaxValue;
    
 //       public struct QueuedButton
 //       {
//            public double beat;
//            public float length;
 //       }
//        static List<QueuedButton> queuedButton = new List<QueuedButton>();

        private void Awake()
        {
            instance = this;
            
        }

        void OnDestroy()
        {
            if (queuedButton.Count > 0) queuedButton.Clear();
            foreach (var ktb in scheduledInputs)
            {
                ktb.Disable();
            }
        }
    

        private void Update()
        {
            var cond = Conductor.instance;

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                PressButton();
                //print("unexpected input");
            }

            if (wantButton != double.MinValue)
            {
                queuedButton.Add(wantButton);
                keepPressing = true;
                pressPlayerCount = 0;
                wantButton = double.MinValue;
            }

            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {            

                if (queuedButton.Count > 0)
                {
                     
                    foreach (var ktb in queuedButton)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>() {
                            new BeatAction.Action(ktb, delegate {
                                ScheduleInput(ktb, 1f, InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);

                            }),
                            new BeatAction.Action(ktb + 1, delegate {
                               if (keepPressing) queuedButton.Add(ktb + 1);
                            }),
                        });
                    }
                        queuedButton.Clear();
                }  
            }      

            if (lastButton + 1 <= cond.songPositionInBeatsAsDouble)
            {
            lastButton++;
                ScheduleInput(lastButton, 1, InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);

            }
        }

        public void KeepTheBeep(double beat, float length, bool shouldBeep, bool autoBeep)
        {
            stopBeep = false;
            if (!shouldBeep) { goBeep = false; return;}
            goBeep = autoBeep;
            if (shouldBeep)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + i, delegate
                        {
                            flashAnimator.Play("KTBPulse", 0, 0);
                            SoundByte.PlayOneShotGame("rhythmTestGBA/blip");
                
                        })
                    });
                
                }
            }
        }





        public void PressButton()
        {
            SoundByte.PlayOneShotGame("rhythmTestGBA/press");

            buttonAnimator.Play("Press", 0, 0);

        }

        public void PreStopKeepbeat(double beat, float length, bool muted)
        {
            shouldmute = muted;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat-1, delegate {killBeeps(beat);}),
                new BeatAction.Action(beat, delegate {StopKeepbeat(beat, shouldmute);})
            });
            

        }        


        public void StopKeepbeat(double beat,  bool shouldmute)
        {

            
            keepPressing = false;
            ScheduleInput(beat, 1f, InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);
            ScheduleInput(beat, 2f, InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);
            ScheduleInput(beat, 3f, InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);


          
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                
                new BeatAction.Action(beat, delegate {flashAnimator.Play("KTBPulse");}),  
                
                new BeatAction.Action(beat+1, delegate {flashAnimator.Play("KTBPulse");}),                      
  
                new BeatAction.Action(beat+2, delegate {flashAnimator.Play("KTBPulse");}),  

                new BeatAction.Action(beat+3, delegate { SoundByte.PlayOneShotGame("rhythmTestGBA/end_ding", beat: beat, forcePlay: true);})

            });
            if (!shouldmute)
            {
                SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat+1, delegate { SoundByte.PlayOneShotGame("rhythmTestGBA/blip2", beat: beat);}),
                    new BeatAction.Action(beat+2, delegate { SoundByte.PlayOneShotGame("rhythmTestGBA/blip2", beat: beat);})
                });  

            }
        }


        public void StopKeepbeatInput(double beat)
        {
            ScheduleInput(beat, 0f, InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);
            flashAnimator.Play("KTBPulse");
        }

        public override void OnBeatPulse(double beat)
        {
            if (goBeep)
            {
                flashAnimator.Play("KTBPulse");
                SoundByte.PlayOneShotGame("rhythmTestGBA/blip");
            }
            
        }

        public void killBeeps(double beat)
        {
            goBeep = false;
        }
        


        public static void StartKeepbeat(double beat)
        {
            RhythmTestGBA.wantButton = beat-1;

            
        }


        public void PreCountDown(double startBeat, float length, int countdownNumber)
        {
            if (keepPressing) return;
            ScheduleInput(startBeat, length * countdownNumber, InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);
            countLength = length;
            switch (countdownNumber)
            {
                case 1:
                    CountOne(startBeat, length);
                    break;
                case 2:
                    CountTwo(startBeat, length);
                    break;
                case 3:
                    CountThree(startBeat, length);
                    break;
                case 4:
                    CountFour(startBeat, length);
                    break;
                case 5:
                    CountFive(startBeat, length);
                    break;
                case 6:
                    CountSix(startBeat, length);
                    break;
                case 7:
                    CountSeven(startBeat, length);
                    break;
                case 8:
                    CountEight(startBeat, length);
                    break;
                case 9:
                    CountNine(startBeat, length);
                    break;
            }
        }

// Countdown playing functions

        public void CountNine(double startBeat, float length)
        {
            
            BeatAction.New(instance, new List<BeatAction.Action>()
            {

                new BeatAction.Action(startBeat, delegate {FlashNine(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length, delegate {FlashEight(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*2, delegate {FlashSeven(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*3, delegate {FlashSix(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*4, delegate {FlashFive(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*5, delegate {FlashFour(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*6, delegate {FlashThree(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*7, delegate {FlashTwo(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*8, delegate {FlashOne(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*9, delegate {FlashZero(startBeat);})
            });
        }


        public void CountEight(double startBeat, float length)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat, delegate {FlashEight(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length, delegate {FlashSeven(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*2, delegate {FlashSix(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*3, delegate {FlashFive(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*4, delegate {FlashFour(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*5, delegate {FlashThree(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*6, delegate {FlashTwo(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*7, delegate {FlashOne(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*8, delegate {FlashZero(startBeat);})
            });

        }        
        public void CountSeven(double startBeat, float length)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat, delegate {FlashSeven(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length, delegate {FlashSix(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*2, delegate {FlashFive(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*3, delegate {FlashFour(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*4, delegate {FlashThree(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*5, delegate {FlashTwo(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*6, delegate {FlashOne(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*7, delegate {FlashZero(startBeat);})
            });
        }
        public void CountSix(double startBeat, float length)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat, delegate {FlashSix(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length, delegate {FlashFive(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*2, delegate {FlashFour(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*3, delegate {FlashThree(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*4, delegate {FlashTwo(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*5, delegate {FlashOne(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*6, delegate {FlashZero(startBeat);})
            });
        }
        public void CountFive(double startBeat, float length)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat, delegate {FlashFive(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length, delegate {FlashFour(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*2, delegate {FlashThree(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*3, delegate {FlashTwo(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*4, delegate {FlashOne(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*5, delegate {FlashZero(startBeat);})
            });
        }
        public void CountFour(double startBeat, float length)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat, delegate {FlashFour(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length, delegate {FlashThree(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*2, delegate {FlashTwo(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*3, delegate {FlashOne(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*4, delegate {FlashZero(startBeat);})
            });
        }
        public void CountThree(double startBeat, float length)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat, delegate {FlashThree(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length, delegate {FlashTwo(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*2, delegate {FlashOne(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*3, delegate {FlashZero(startBeat);})
            });
        }
        public void CountTwo(double startBeat, float length)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat, delegate {FlashTwo(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length, delegate {FlashOne(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length*2, delegate {FlashZero(startBeat);})
            });
        }
        public void CountOne(double startBeat, float length)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat, delegate {FlashOne(startBeat, disableCount);}),
                new BeatAction.Action(startBeat + length, delegate {FlashZero(startBeat);})
            });
        }
// Number Call Functions

        public void HideCountdown(bool toggleCount)
        {
            if (toggleCount)
            {
                disableCount = false;
            }
            else
            {
                disableCount = true;
            }
        }

        public void FlashNine(double beat, bool disableCount)
        {
            if (disableCount != true)
            {
                numberBGAnimator.Play("FlashBG", -1, 0);
                numberAnimator.Play("Nine");
                SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");
            }
            else
            {
                numberBGAnimator.Play("Idle");
                numberAnimator.Play("Idle");                
            }
        }

        public void FlashEight(double beat, bool disableCount)
        {
            if (!disableCount)
            {
                numberBGAnimator.Play("FlashBG", -1, 0);
                numberAnimator.Play("Eight");
                SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");
            }
            else
            {
                numberBGAnimator.Play("Idle");
                numberAnimator.Play("Idle");                
            }
        }

        public void FlashSeven(double beat, bool disableCount)
        {
            if (!disableCount)
            {
                numberBGAnimator.Play("FlashBG", -1, 0);
                numberAnimator.Play("Seven");
                SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");
            }
            else
            {
                numberBGAnimator.Play("Idle");
                numberAnimator.Play("Idle");                
            }
        }

        public void FlashSix(double beat, bool disableCount)
        {
            if (!disableCount)
            {
                numberBGAnimator.Play("FlashBG", -1, 0);
                numberAnimator.Play("Six");
                SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");
            }
            else
            {
                numberBGAnimator.Play("Idle");
                numberAnimator.Play("Idle");                
            }
        }

        public void FlashFive(double beat, bool disableCount)
        {
            if (!disableCount)
            {
                numberBGAnimator.Play("FlashBG", -1, 0);
                numberAnimator.Play("Five");
                SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");
            }
            else
            {
                numberBGAnimator.Play("Idle");
                numberAnimator.Play("Idle");                
            }
        }

        public void FlashFour(double beat, bool disableCount)
        {
            if (!disableCount)
            {
                numberBGAnimator.Play("FlashBG", -1, 0);
                numberAnimator.Play("Four");
                SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");
            }
            else
            {
                numberBGAnimator.Play("Idle");
                numberAnimator.Play("Idle");                
            }
        }        

        public void FlashThree(double beat, bool disableCount)
        {
            if (!disableCount)
            {
                numberBGAnimator.Play("FlashBG", -1, 0);
                numberAnimator.Play("Three");
                SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");
            }
            else
            {
                numberBGAnimator.Play("Idle");
                numberAnimator.Play("Idle");                
            }
        }

        public void FlashTwo(double beat, bool disableCount)
        {
            if (!disableCount)
            {
                numberBGAnimator.Play("FlashBG", -1, 0);
                numberAnimator.Play("Two");
                SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");
            }
            else
            {
                numberBGAnimator.Play("Idle");
                numberAnimator.Play("Idle");                
            }
        }

        public void FlashOne(double beat, bool disableCount)
        {
            if (!disableCount)
            {
                numberBGAnimator.Play("FlashBG", -1, 0);
                numberAnimator.Play("One");
                SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");
            }
            else
            {
                numberBGAnimator.Play("Idle");
                numberAnimator.Play("Idle");                
            }
        }

        public void FlashZero(double beat)
        {

            numberBGAnimator.Play("FlashHit");
            numberAnimator.Play("Zero");
            SoundByte.PlayOneShotGame("rhythmTestGBA/blip3");

        }




        public void HideCountdown(double beat, bool showcount)
        {

        }


        public void ButtonSuccess(PlayerActionEvent caller, float state)
        {
            PressButton();
       

        }

        public void ButtonFailure(PlayerActionEvent caller)
        {

        }
        public void ButtonEmpty(PlayerActionEvent caller) {



         }


    }
}
