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
            return new Minigame("rhythmTestGBA", "Rhythm Test (GBA)", "ffffff", false, false, new List<GameAction>()
            {

                new GameAction("countin", "Start Beeping")
                {
                    function = delegate { RhythmTestGBA.instance.KeepTheBeep(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); },
                    defaultLength = 1f,
                    resizable = false,
                },

                 new GameAction("button", "Start Keep-the-Beat")
                {
                    preFunction = delegate { RhythmTestGBA.PreStartKeepbeat(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); },
                    defaultLength = 1f,
                    resizable = false,

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
        public static RhythmTestGBA instance { get; set; }

        [Header("Animators")]
        [SerializeField] Animator buttonAnimator;
        [SerializeField] Animator flashAnimator;

        [Header("Properties")]
        private static double startBlippingBeat = double.MaxValue;

        GameEvent button = new GameEvent();

        double lastButton = double.MaxValue;
    
        public struct QueuedButton
        {
            public double beat;
            public float length;
        }
        static List<QueuedButton> queuedInputs = new List<QueuedButton>();

                private void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (queuedInputs.Count > 0) queuedInputs.Clear();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }
    
        public override void OnPlay(double beat)
        {
            queuedInputs.Clear();
        }    

        private void Update()
        {
            var cond = Conductor.instance;

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                PressButton();
                //print("unexpected input");
            }
            if (queuedInputs.Count > 0)
            {
                foreach (var input in queuedInputs)
                {
                    StartKeepbeat(input.beat, input.length);
                }
                queuedInputs.Clear();
            }        

            if (queuedInputs.Count > 0)
            {
                foreach (var input in queuedInputs)
                {
                    StartKeepbeat(input.beat, input.length);
                }
                queuedInputs.Clear();
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
                
                    })
                });
                
            }
        }



        public void PressButton()
        {
            SoundByte.PlayOneShot("count-ins/cowbell");

            buttonAnimator.Play("Press", 0, 0);

        }

                


        public static void PreStartKeepbeat(double beat, float length)
        {
            if (GameManager.instance.currentGame == "rhythmTestGBA")
            {
                instance.StartKeepbeat(beat, length);
                
            
            }
            else
            {
                queuedInputs.Add(new QueuedButton { beat = beat, length = length });
            }
        }


        public void StartKeepbeat(double beat, float length)
        {
            
            lastButton = beat - 1;
            ScheduleInput(lastButton, 1, InputAction_BasicPress, ButtonSuccess, ButtonFailure, ButtonEmpty);
            
        }

        public void ButtonSuccess(PlayerActionEvent caller, float state)
        {
            PressButton();
        }

        public void ButtonFailure(PlayerActionEvent caller)
        {

        }
        public void ButtonEmpty(PlayerActionEvent caller) { }
        
    }
}
