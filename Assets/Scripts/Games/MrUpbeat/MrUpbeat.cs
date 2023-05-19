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
            return new Minigame("mrUpbeat", "Mr. Upbeat", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("start stepping", "Start Stepping")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; MrUpbeat.StartStepping(e.beat, e.length, e["force"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("force", false, "Force Mr. Downbeat", "Forces inputs to not be only on the offbeats"),
                    }
                },
                new GameAction("ding", "Ding!")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        MrUpbeat.instance.Ding(eventCaller.currentEntity["toggle"], e["stopBlipping"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Applause", "Plays an applause sound effect."),
                        new Param("stopBlipping", true, "Stop Blipping?", "When the stepping stops, should the blipping stop too?"),
                    }
                },
                new GameAction("changeBG", "Change Background Color")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MrUpbeat.instance.FadeBackgroundColor(e["start"], e["end"], e.length, e["toggle"]); },
                    defaultLength = 1f,
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
                        new Param("shouldGrow", true, "Grow Antenna?", "Should Mr. Upbeat's antenna grow?"),
                        new Param("resetBlip", false, "Reset Antenna?", "Should Mr. Upbeat's antenna reset?"),
                        new Param("blip", true, "Should Blip?", "Should Mr. Upbeat blip every offbeat?"),
                    }
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MrUpbeat;

    public class MrUpbeat : Minigame
    {
        static List<float> queuedInputs = new List<float>();

        [Header("References")]
        [SerializeField] Animator metronomeAnim;
        [SerializeField] UpbeatMan man;
        [SerializeField] Material blipMaterial;
        [SerializeField] SpriteRenderer bg;
        [SerializeField] SpriteRenderer[] shadowSr;

        [Header("Properties")]
        private Tween bgColorTween;
        public int stepIterate = 0;
        public static float downbeatMod = 0.5f;
        public static bool shouldBlip;
        static bool noDing;

        public static MrUpbeat instance;

        private void Awake()
        {
            instance = this;

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

            // these variables wouldn't get reset, even when you go in and out of unity play mode???
            shouldBlip = false;
            stepIterate = 0;
        }

        public void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused) {
                if (queuedInputs.Count > 0) {
                    foreach (var input in queuedInputs) {
                        string dir = stepIterate % 2 == 1 ? "Right" : "Left";
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                            new BeatAction.Action(input, delegate { 
                                instance.metronomeAnim.DoScaledAnimationAsync("MetronomeGo" + dir, 0.5f);
                                Jukebox.PlayOneShotGame("mrUpbeat/metronome" + dir);
                                ScheduleInput(input, 0.5f, InputType.STANDARD_DOWN, Success, Miss, Nothing);
                                if (MrUpbeat.noDing) queuedInputs.Add(input + 1);
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

        public void Ding(bool applause, bool stopBlipping)
        {
            Jukebox.PlayOneShotGame("mrUpbeat/ding");
            if (applause) Jukebox.PlayOneShot("applause");
            if (stopBlipping) shouldBlip = false;
        }

        public static void StartStepping(float beat, float length, bool force)
        {
            // mr. downbeat stuff. god i hate mr. downbeat
            // force != true means that mr. upbeat will always blip/step on the offbeats
            beat = force ? beat - 0.5f : MathF.Floor(beat);
            downbeatMod = force ? (beat % 1) : 0.5f;
            
            if (GameManager.instance.currentGame != "mrUpbeat") {
                Blipping(beat, length);
                MrUpbeat.shouldBlip = true;
            } else {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                    new BeatAction.Action(beat, delegate {
                        MrUpbeat.shouldBlip = true;
                    }),
                });
            }
            var dings = EventCaller.GetAllInGameManagerList("mrUpbeat", new string[] { "ding" });
            if (dings.Count == 0) {
                MrUpbeat.noDing = true;
                queuedInputs.Add(beat + (force ? length : MathF.Floor(length)));
                return;
            }
            MrUpbeat.noDing = false;
            int whichDing = 0;
            for (int i = 0; i < dings.Count; i++) {
                if (dings[i].beat > beat) {
                    whichDing = i;
                    break;
                }
            }
            for (int i = (int)length; i < dings[whichDing].beat - beat; i++) {
                queuedInputs.Add(beat + i - (force ? downbeatMod : 0));
            }
        }

        public static void Blipping(float beat, float length)
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

            for (int i = 0; i < switchGames[whichSwitch].beat - beat - 0.5f; i++) {
                blips.Add(new MultiSound.Sound("mrUpbeat/blip", beat + 0.5f + i));
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
            if (shouldGrow && man.blipSize < 4) man.blipSize++;
            if (resetBlip) man.blipSize = 0;
            man.blipString = inputLetter;
            shouldBlip = blip;
        }

        public void Nothing(PlayerActionEvent caller) {}
    }
}