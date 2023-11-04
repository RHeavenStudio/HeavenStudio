using System.Collections.Generic;
using UnityEngine;
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
                        MrUpbeat.PrePrepare(e.beat, e.length, e["forceOnbeat"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("forceOnbeat", false, "Mr. Downbeat", "Force Mr. Upbeat to step on the beat of the block instead of on the offbeat (only use this if you know what you're doing)"),
                    },
                    preFunctionLength = 0.5f,
                    defaultLength = 4f,
                    resizable = true,
                },
                new GameAction("ding", "Ding!")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        MrUpbeat.Ding(e.beat, e["toggle"], e["stopBlipping"], e["playDing"]);
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Applause", "Plays an applause sound effect."),
                        new Param("stopBlipping", true, "Stop Blipping", "When the stepping stops, should the blipping stop too?"),
                        new Param("playDing", true, "Play Ding", "Should this block play a ding?"),
                    },
                    preFunctionLength = 0.5f,
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
                        new Param("start", new Color(0.878f, 0.878f, 0.878f), "Start Color", "The start color for the fade"),
                        new Param("end", new Color(0.878f, 0.878f, 0.878f), "End Color", "The end color for the fade or the color that will be switched to if -instant- is ticked on"),
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
                        new Param("setShadow", false, "Set Shadow Color", "Should Mr. Upbeat's shadow be custom?"),
                        new Param("shadowColor", new Color(1f, 1f, 1f, 0), "Shadow Color", "If \"Set Shadow Color\" is checked, this will set the shadow's color"),
                    }
                },
                new GameAction("blipEvents", "Blip Events")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MrUpbeat.instance.BlipEvents(e["letter"], e["shouldGrow"], e["resetBlip"], e["shouldBlip"]);
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("letter", "", "Letter To Appear", "Which letter to appear on the blip"),
                        new Param("shouldGrow", true, "Grow Antenna", "Should Mr. Upbeat's antenna grow every blip?"),
                        new Param("resetBlip", false, "Reset Antenna", "Should Mr. Upbeat's antenna reset?"),
                        new Param("shouldBlip", true, "Should Blip", "Should Mr. Upbeat blip every offbeat?"),
                    }
                },
                new GameAction("fourBeatCountInOffbeat", "4 Beat Count-In")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        MrUpbeat.CountIn(e.beat, e.length, e["a"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("a", true, "A", "A"),
                    },
                    defaultLength = 4f,
                    resizable = true,
                },
                new GameAction("countOffbeat", "Count")
                {
                    inactiveFunction = delegate { MrUpbeat.Count(eventCaller.currentEntity["number"]); },
                    parameters = new List<Param>()
                    {
                        new Param("number", MrUpbeat.Counts.One, "Number", "The sound to play"),
                    },
                },
                new GameAction("forceStepping", "Force Stepping")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MrUpbeat.instance.ForceStepping(e.beat, e.length);
                    },
                    defaultLength = 4f,
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
    using Jukebox;
    public class MrUpbeat : Minigame
    {
        public enum Counts
        {
            One,
            Two,
            Three,
            Four,
            A,
        }

        [Header("References")]
        [SerializeField] Animator metronomeAnim;
        [SerializeField] UpbeatMan man;
        [SerializeField] Material blipMaterial;
        [SerializeField] SpriteRenderer bg;
        [SerializeField] SpriteRenderer[] shadowSr;

        [Header("Properties")]
        private Tween bgColorTween;
        public int stepIterate = 0;
        private static double startSteppingBeat = double.MaxValue;
        private static double startBlippingBeat = double.MaxValue;
        private bool stopStepping;
        public bool stopBlipping;

        public static MrUpbeat instance;

        private void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            startSteppingBeat = double.MaxValue;
            startBlippingBeat = double.MaxValue;
            stepIterate = 0;
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        public override void OnGameSwitch(double beat)
        {
            if (beat >= startBlippingBeat) {
                double tempBeat = ((beat % 1 == 0.5) ? Mathf.Floor((float)beat) : Mathf.Round((float)beat)) + (startBlippingBeat % 1);
                BeatAction.New(instance, new List<BeatAction.Action>() {
                    new BeatAction.Action(tempBeat, delegate { man.RecursiveBlipping(tempBeat); })
                });
                startBlippingBeat = double.MaxValue;
            }

            // init background color/blip color stuff by getting the last of each of those blocks
            List<RiqEntity> prevEntities = GameManager.instance.Beatmap.Entities.FindAll(c => c.beat <= beat && c.datamodel.Split(0) == "mrUpbeat");
            var bgColorEntity = prevEntities.FindLast(x => x.datamodel.Split(1) == "changeBG" && x.beat <= beat);
            var upbeatColorEntity = prevEntities.FindLast(x => x.datamodel.Split(1) == "upbeatColors" && x.beat <= beat);

            if (bgColorEntity != null) {
                bg.color = bgColorEntity["end"];
            }
            
            if (upbeatColorEntity != null) {
                blipMaterial.SetColor("_ColorBravo", upbeatColorEntity["blipColor"]);
                Color shadowColor = upbeatColorEntity["shadowColor"];
                if (upbeatColorEntity["setShadow"]) foreach (var shadow in shadowSr) {
                    shadow.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, 1);
                }
            } else {
                blipMaterial.SetColor("_ColorBravo", new Color(0, 1f, 0));
            }
        }

        public void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused) {
                if (cond.songPositionInBeatsAsDouble >= startSteppingBeat) {
                    RecursiveStepping(startSteppingBeat);
                    startSteppingBeat = double.MaxValue;
                }

                if (cond.songPositionInBeats >= startBlippingBeat) {
                    man.RecursiveBlipping(startBlippingBeat);
                    startBlippingBeat = double.MaxValue;
                }
            }
        }

        public static void Ding(double beat, bool applause, bool stopBlipping, bool playDing)
        {
            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat - 0.5, delegate {
                    instance.stopStepping = true;
                    if (stopBlipping) instance.stopBlipping = true;
                }),
                new BeatAction.Action(beat + 0.5, delegate {
                    instance.stopStepping = false;
                }),
            });
            if (playDing) SoundByte.PlayOneShotGame("mrUpbeat/ding", beat: beat, forcePlay: true);
            if (applause) SoundByte.PlayOneShot("applause", beat: beat);
        }

        public static void PrePrepare(double beat, float length, bool forceOffbeat)
        {
            bool isGame = GameManager.instance.currentGame == "mrUpbeat";
            if (forceOffbeat) {
                startBlippingBeat = beat;
                startSteppingBeat = beat + length - 0.5f;
                if (!isGame) Blipping(beat, length);
            } else {
                startBlippingBeat = Mathf.Floor((float)beat) + 0.5;
                startSteppingBeat = Mathf.Floor((float)beat) + Mathf.Round(length);
                if (!isGame) Blipping(Mathf.Floor((float)beat) + 0.5f, length);
            }
        }

        private void RecursiveStepping(double beat)
        {
            if (stopStepping) {
                stopStepping = false;
                return;
            }
            string dir = (stepIterate % 2 == 1) ? "Right" : "Left";
            metronomeAnim.DoScaledAnimationAsync("MetronomeGo" + dir, 0.5f);
            SoundByte.PlayOneShotGame("mrUpbeat/metronome" + dir);
            ScheduleInput(beat, 0.5f, InputAction_BasicPress, Success, Miss, Nothing);
            BeatAction.New(this, new List<BeatAction.Action>() {
                new(beat + 1, delegate { RecursiveStepping(beat + 1); })
            });
            stepIterate++;
        }

        public void ForceStepping(double beat, float length)
        {
            var actions = new List<BeatAction.Action>();
            for (int i = 0; i < length; i++)
            {
                ScheduleInput(beat + i, 0.5f, InputAction_BasicPress, Success, Miss, Nothing);
                actions.Add(new BeatAction.Action(beat + i, delegate { 
                    string dir = (stepIterate % 2 == 1) ? "Right" : "Left";
                    metronomeAnim.DoScaledAnimationAsync("MetronomeGo" + dir, 0.5f);
                    SoundByte.PlayOneShotGame("mrUpbeat/metronome" + dir);
                    stepIterate++;
                }));
            }
            BeatAction.New(this, actions);
        }

        public static void Blipping(double beat, float length)
        {
            RiqEntity gameSwitch = GameManager.instance.Beatmap.Entities.Find(c => c.beat > beat && c.datamodel == "gameManager/switchGame/mrUpbeat");
            if (gameSwitch.beat <= beat || gameSwitch.beat >= beat + length + 1) return;

            List<MultiSound.Sound> inactiveBlips = new List<MultiSound.Sound>();
            for (int i = 0; i < gameSwitch.beat - beat; i++) {
                inactiveBlips.Add(new MultiSound.Sound("mrUpbeat/blip", beat + i));
            }

            MultiSound.Play(inactiveBlips.ToArray(), forcePlay: true);
        }

        public void Success(PlayerActionEvent caller, float state)
        {
            man.Step();
            if (state >= 1f || state <= -1f) SoundByte.PlayOneShot("nearMiss");
        }

        public void Miss(PlayerActionEvent caller)
        {
            man.Fall();
        }

        public void ChangeBackgroundColor(Color color1, Color color2, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (bgColorTween != null)
                bgColorTween.Kill(true);

            if (seconds == 0) {
                bg.color = color2;
            } else {
                bg.color = color1;
                bgColorTween = bg.DOColor(color2, seconds);
            }
        }

        public void FadeBackgroundColor(Color start, Color end, float beats, bool instant)
        {
            ChangeBackgroundColor(start, end, 0f);
            if (!instant) ChangeBackgroundColor(start, end, beats);
        }

        public void UpbeatColors(Color blipColor, bool setShadow, Color shadowColor)
        {
            blipMaterial.SetColor("_ColorBravo", blipColor);

            if (setShadow) foreach (var shadow in shadowSr) {
                shadow.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, 1);
            }
        }

        public void BlipEvents(string inputLetter, bool shouldGrow, bool resetBlip, bool shouldBlip)
        {
            if (resetBlip) man.blipSize = 0;
            man.shouldGrow = shouldGrow;
            man.blipString = inputLetter;
            man.shouldBlip = shouldBlip;
        }

        public static void Count(int number)
        {
            SoundByte.PlayOneShotGame("mrUpbeat/" + (number < 4 ? number + 1 : "a"), forcePlay: true);
        }

        public static void CountIn(double beat, float length, bool a)
        {
            var sound = new List<MultiSound.Sound>();
            if (a) sound.Add(new MultiSound.Sound("mrUpbeat/a", beat - (0.5f * (length/4))));
            for (int i = 0; i < 4; i++) {
                sound.Add(new MultiSound.Sound("mrUpbeat/" + (i + 1), beat + (i * (length / 4)), offset: (i == 3) ? 0.05 : 0));
            }
            
            MultiSound.Play(sound.ToArray(), forcePlay: true);
        }

        public void Nothing(PlayerActionEvent caller) { }
    }
}