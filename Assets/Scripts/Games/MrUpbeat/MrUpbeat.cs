using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbUpbeatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("mrUpbeat", "Mr. Upbeat", "E0E0E0", false, false, new List<GameAction>()
            {
                
                new GameAction("prepare", "Prepare")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        MrUpbeat.StartStepping(e.beat, e.length);
                    },
                    defaultLength = 4f,
                    resizable = true,
                },
                new GameAction("ding", "Ding!")
                {
                    preFunction = delegate { 
                        var e = eventCaller.currentEntity; 
                        MrUpbeat.Ding(e.beat, e["toggle"], e["stopBlipping"]); 
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Applause", "Plays an applause sound effect."),
                        new Param("stopBlipping", true, "Stop Blipping?", "When the stepping stops, should the blipping stop too?"),
                    },
                    preFunctionLength = 1f,
                },
                new GameAction("changeBG", "Change Background Color")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MrUpbeat.instance.FadeBackgroundColor(e["start"], e["end"], e.length, e["toggle"]); 
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start", new Color(0.878f, 0.878f, 0.878f), "Start Color", "The start color for the fade or the color that will be switched to if -instant- is ticked on."),
                        new Param("end", new Color(0.878f, 0.878f, 0.878f), "End Color", "The end color for the fade."),
                        new Param("toggle", false, "Instant", "Should the background instantly change color?")
                    }
                },
                new GameAction("upbeatColors", "Upbeat Colors")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MrUpbeat.instance.UpbeatColors(e["blipColor"], e["setShadow"], e["shadowColor"]); 
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("blipColor", new Color(0, 1f, 0), "Blip Color", "Change blip color"),
                        new Param("setShadow", false, "Set Shadow Color?", "Should Mr. Upbeat's shadow be custom?"),
                        new Param("shadowColor", new Color(1f, 1f, 1f, 0), "Shadow Color", "If \"Set Shadow Color\" is checked, this will set the shadow's color."),
                    }
                },
                new GameAction("blipEvents", "Blip Events")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MrUpbeat.instance.BlipEvents(e["letter"], e["shouldGrow"], e["resetBlip"], e["blip"]); 
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("letter", "", "Letter To Appear", "Which letter to appear on the blip"),
                        new Param("shouldGrow", true, "Grow Antenna?", "Should Mr. Upbeat's antenna grow every blip?"),
                        new Param("resetBlip", false, "Reset Antenna?", "Should Mr. Upbeat's antenna reset?"),
                        new Param("blip", true, "Should Blip?", "Should Mr. Upbeat blip every offbeat?"),
                    }
                },
                // will implement these soon
                new GameAction("fourBeatCountInOffbeat", "4 Beat Count-In")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        //MrUpbeat.CountIn(e.beat, e.length);
                    },
                    defaultLength = 4f,
                    resizable = true,
                    hidden = true,
                },
                new GameAction("countOffbeat", "4 Beat Count-In")
                {
                    //function = delegate { MrUpbeat.Count(eventCaller.currentEntity["number"]); },
                    parameters = new List<Param>()
                    {
                        new Param("number", SoundEffects.CountNumbers.One, "Number", "The sound to play"),
                    },
                    hidden = true,
                },

                // backwards compatibility !!!!
                new GameAction("start stepping", "Start Stepping")
                {
                    hidden = true,
                    preFunction = delegate {var e = eventCaller.currentEntity; MrUpbeat.StartStepping(e.beat, e.length); },
                    resizable = true,
                },
            },
            new List<string>() {"agb", "keep"},
            "agboffbeat", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MrUpbeat;

    public class MrUpbeat : Minigame
    {
        static List<double> queuedInputs = new();

        [Header("References")]
        [SerializeField] Animator metronomeAnim;
        [SerializeField] UpbeatMan man;
        [SerializeField] Material blipMaterial;
        [SerializeField] SpriteRenderer bg;
        [SerializeField] SpriteRenderer[] shadowSr;

        [Header("Properties")]
        private Tween bgColorTween;
        public int stepIterate = 0;
        public static bool shouldBlip;
        static bool isStepping;
        static bool shouldntStop;

        public static MrUpbeat instance;

        private void Awake()
        {
            instance = this;
            isStepping = false;

            blipMaterial.SetColor("_ColorBravo", new Color(0, 1f, 0));
        }

        private void Start()
        {
            man.Blip();
        }

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused) {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }

            shouldBlip = false;
            isStepping = false;
            stepIterate = 0;
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        public void Update()
        {
            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                if (queuedInputs.Count > 0) {
                    foreach (var input in queuedInputs) {
                        string dir = stepIterate % 2 == 1 ? "Right" : "Left";
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                            new BeatAction.Action(input, delegate {
                                instance.metronomeAnim.DoScaledAnimationAsync("MetronomeGo" + dir, 0.5f);
                                SoundByte.PlayOneShotGame("mrUpbeat/metronome" + dir);
                                ScheduleInput(input, 0.5f, InputType.STANDARD_DOWN, Success, Miss, Nothing);
                                if (MrUpbeat.shouldntStop) queuedInputs.Add(input + 1);
                            }),
                        });
                        stepIterate++;
                    }
                    queuedInputs.Clear();
                }

                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN)) {
                    man.Step();
                }
            }
        }

        public static void Ding(double beat, bool applause, bool stopBlipping)
        {
            MrUpbeat.shouldntStop = false;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate {
                    MrUpbeat.isStepping = false;
                    SoundByte.PlayOneShotGame("mrUpbeat/ding");
                    if (applause) SoundByte.PlayOneShot("applause");
                    if (stopBlipping) MrUpbeat.shouldBlip = false;
                }),
            });
        }

        public static void StartStepping(double beat, float length)
        {
            if (MrUpbeat.isStepping) return;
            MrUpbeat.isStepping = true;
            if (GameManager.instance.currentGame != "mrUpbeat") {
                Blipping(beat, length);
                MrUpbeat.shouldBlip = true;
            } else {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                    new BeatAction.Action(Math.Floor(beat), delegate { 
                        MrUpbeat.shouldBlip = true; 
                    }),
                });
            }

            MrUpbeat.shouldntStop = true;
            queuedInputs.Add(Math.Floor(beat+length));
        }

        public static void Blipping(double beat, float length)
        {
            List<MultiSound.Sound> blips = new List<MultiSound.Sound>();
            var switchGames = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" });
            int whichSwitch = 0;
            if (switchGames.Count != 0) {
                for (int i = 0; i < switchGames.Count; i++) {
                    if (switchGames[i].beat > beat) {
                        whichSwitch = i;
                        break;
                    }
                }
            }

            for (int i = 0; i < switchGames[whichSwitch].beat - Math.Floor(beat) - 0.5f; i++) {
                blips.Add(new MultiSound.Sound("mrUpbeat/blip", Math.Floor(beat) + 0.5f + i));
            }

            MultiSound.Play(blips.ToArray(), forcePlay: true);
        }

        public void Success(PlayerActionEvent caller, float state)
        {
            man.Step();
        }

        public void Miss(PlayerActionEvent caller)
        {
            man.Fall();
        }

        public void ChangeBackgroundColor(Color color, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (bgColorTween != null)
                bgColorTween.Kill(true);

            if (seconds == 0) {
                bg.color = color;
            } else {
                bgColorTween = bg.DOColor(color, seconds);
            }
        }

        public void FadeBackgroundColor(Color start, Color end, float beats, bool instant)
        {
            ChangeBackgroundColor(start, 0f);
            if (!instant) ChangeBackgroundColor(end, beats);
        }

        public void UpbeatColors(Color blipColor, bool setShadow, Color shadowColor)
        {
            blipMaterial.SetColor("_ColorBravo", blipColor);

            if (setShadow) foreach (var shadow in shadowSr) {
                shadow.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, 1);
            }
        }

        public void BlipEvents(string inputLetter, bool shouldGrow, bool resetBlip, bool blip)
        {
            man.shouldGrow = shouldGrow;
            if (resetBlip) {
                man.blipSize = 0;
                man.shouldGrow = false;
            }
            man.blipString = inputLetter;
            shouldBlip = blip;
        }

        /*
        public static void Count(int number)
        {
            Jukebox.PlayOneShotGame("mrUpbeat/count"+(number + 1), forcePlay: true);
        }

        public static void CountIn(float beat, float length)
        {
            var sound = new List<MultiSound.Sound>() {
                
            };
            
            MultiSound.Play(sound.ToArray(), forcePlay: true);
        }
        */

        public void Nothing(PlayerActionEvent caller) {}
    }
}