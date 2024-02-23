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
                    function = delegate { RhythmTestGBA.instance.KeepTheBeep(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); },
                    defaultLength = 1f,
                    resizable = true,
                },

                new GameAction("button", "Start Keep-the-Beat")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmTestGBA.StartKeepbeat(e.beat); },
                    defaultLength = 1f,
                    resizable = false,

                },

                new GameAction("stopktb", "Stop Keep-the-Beat")
                {
                    function = delegate { RhythmTestGBA.instance.StopKeepbeat(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); },
                    defaultLength = 4f,
                    resizable = false,
                },

                new GameAction("countthree", "Count Down From Three")
                {
                    function = delegate {RhythmTestGBA.instance.CountDownThree(
eventCaller.currentEntity.beat, eventCaller.currentEntity["mute3"], eventCaller.currentEntity["mute2"], eventCaller.currentEntity["mute1"]);},
                    defaultLength = 4f,
                    resizable = false,
                    parameters = new List<Param>()
                    {
                        new Param("mute3", false, "3 Muted", "Hides and mutes the 3 in the countdown."),
                        new Param("mute2", false, "2 Muted", "Hides and mutes the 2 in the countdown."),
                        new Param("mute1", false, "1 Muted", "Hides and mutes the 1 in the countdown."),
                    }
                },

                new GameAction("countdown", "Countdown")
                {
                    function = delegate {RhythmTestGBA.instance.CountDown(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["countdownNumber"]);},
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("countdownNumber", new EntityTypes.Integer(1, 9, 3), "Beats", "Set how many beats there will be before the player has to input.")                        
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

        [Header("Animators")]
        [SerializeField] Animator buttonAnimator;
        [SerializeField] Animator flashAnimator;
        [SerializeField] Animator numberbgAnimator;
        [SerializeField] Animator numberAnimator;

        [Header("Properties")]
        private static double startBlippingBeat = double.MaxValue;

        [Header("Variables")]
        bool keepPressing;
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
                                flashAnimator.Play("KTBPulse", 0, 0);
                                SoundByte.PlayOneShotGame("rhythmTestGBA/blip");

                            }),
                            new BeatAction.Action(ktb + 1, delegate {
                               flashAnimator.Play("KTBPulse", 0, 0);
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

        public void KeepTheBeep(double beat, float length)
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



        public void PressButton()
        {
            SoundByte.PlayOneShotGame("rhythmTestGBA/press");

            buttonAnimator.Play("Press", 0, 0);

        }

                


        public void StopKeepbeat(double beat, float length)
        {

            keepPressing = false;
            SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");            
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat+1, delegate { StopKeepbeatInput(beat);}),
                new BeatAction.Action(beat+1, delegate { SoundByte.PlayOneShotGame("rhythmTestGBA/blip2", beat: beat);}),
                new BeatAction.Action(beat+2, delegate { StopKeepbeatInput(beat);}),
                new BeatAction.Action(beat+2, delegate { SoundByte.PlayOneShotGame("rhythmTestGBA/blip2", beat: beat);}),
                new BeatAction.Action(beat+3, delegate { StopKeepbeatInput(beat);}),
                new BeatAction.Action(beat+3, delegate { SoundByte.PlayOneShotGame("rhythmTestGBA/end_ding", beat: beat, forcePlay: true);})

            });
        }


        public void StopKeepbeatInput(double beat)
        {
            ScheduleInput(beat, 0f, InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);
            flashAnimator.Play("KTBPulse");
        }
        


        public static void StartKeepbeat(double beat)
        {
            RhythmTestGBA.wantButton = beat-1;

            
        }

        public void CountDownThree(double beat, bool mute3, bool mute2, bool mute1)
        {
            ScheduleInput(beat, 3f, InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);
            if (mute3)
            {
                FlashNone(beat);
            }
            else
            {
                FlashThree(beat);
            }
            if (mute2)
            {
                BeatAction.New(instance, new List<BeatAction.Action>(){
                    new BeatAction.Action(beat+1, delegate {FlashNone(beat);})});
            }
            else
            {
                BeatAction.New(instance, new List<BeatAction.Action>(){
                    new BeatAction.Action(beat+1, delegate {FlashTwo(beat);})});
            }
            if (mute1)
            {
                BeatAction.New(instance, new List<BeatAction.Action>(){
                    new BeatAction.Action(beat+2, delegate {FlashNone(beat);})});
            }
            else
            {
                BeatAction.New(instance, new List<BeatAction.Action>(){
                    new BeatAction.Action(beat+2, delegate {FlashOne(beat);})});
            }
                BeatAction.New(instance, new List<BeatAction.Action>(){
                    new BeatAction.Action(beat+3, delegate {FlashZero(beat);})});

        }

        public void CountDown(double beat, float length, int countdownNumber)
        {



            ScheduleInput(beat, length * (countdownNumber), InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);


        }

// Number Call Functions
        public void FlashSeven(double beat)
        {

            numberbgAnimator.Play("FlashBG");
            numberAnimator.Play("Seven");
            SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");

        }
        public void FlashSix(double beat)
        {

            numberbgAnimator.Play("FlashBG");
            numberAnimator.Play("Six");
            SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");

        }
        public void FlashFive(double beat)
        {

            numberbgAnimator.Play("FlashBG");
            numberAnimator.Play("Five");
            SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");

        }
        public void FlashFour(double beat)
        {

            numberbgAnimator.Play("FlashBG");
            numberAnimator.Play("Four");
            SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");

        }
        public void FlashThree(double beat)
        {

            numberbgAnimator.Play("FlashBG");
            numberAnimator.Play("Three");
            SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");

        }
        public void FlashTwo(double beat)
        {

            numberbgAnimator.Play("FlashBG");
            numberAnimator.Play("Two");
            SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");

        }
        public void FlashOne(double beat)
        {

            numberbgAnimator.Play("FlashBG");
            numberAnimator.Play("One");
            SoundByte.PlayOneShotGame("rhythmTestGBA/blip2");

        }
        public void FlashZero(double beat)
        {

            numberbgAnimator.Play("FlashHit");
            numberAnimator.Play("Zero");
            SoundByte.PlayOneShotGame("rhythmTestGBA/blip3");

        }




        public void FlashNone(double beat)
        {

            numberbgAnimator.Play("Idle");
            numberAnimator.Play("Idle");

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
