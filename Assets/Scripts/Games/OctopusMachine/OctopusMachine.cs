using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;
// using GhostlyGuy's Balls;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrOctopusMachineLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("octopusMachine", "Octopus Machine \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "FFf362B", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Bop(e.beat, e.length, e["whichBop"], e["bop"], e["autoBop"]);
                    },
                    parameters = new List<Param>() {
                        new Param("whichBop", OctopusMachine.Bops.Bop, "Which Bop", "Plays a specific bop type"),
                        new Param("bop", true, "Single Bop", "Plays one bop"),
                        new Param("autoBop", false, "Keep Bopping", "Keeps playing the specified bop type"),
                    },
                },
                new GameAction("startInterval", "Start Interval")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.StartInterval(e.beat, e.length);
                    },
                    resizable = true,
                    priority = 5,
                },
                new GameAction("squeeze", "Squeeze")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Squeeze(e.beat, e.length);
                    },
                    resizable = true,
                    parameters = new List<Param>() {
                        new Param("shouldPrep", true, "Prepare?", "Plays a prepare animation before the cue."),
                        new Param("prepBeats", new EntityTypes.Float(0, 4, 1), "Prepare Beats", "How many beats before the cue does the octopus prepare?"),
                    },
                    preFunctionLength = 4f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (e["shouldPrep"]) OctopusMachine.Prepare(e.beat, e["prepBeats"]);
                    },
                    priority = 1,
                },
                new GameAction("release", "Release")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Release(e.beat, e.length);
                    },
                    resizable = true,
                    priority = 1,
                },
                new GameAction("pop", "Pop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Pop(e.beat, e.length);
                    },
                    resizable = true,
                    priority = 1,
                },
                new GameAction("forceSqueeze", "Force Squeeze")
                {
                    function = delegate { OctopusMachine.instance.ForceSqueeze(); }
                },
                new GameAction("prepare", "Prepare")
                {
                    function = delegate { OctopusMachine.queuePrepare = true; },
                    defaultLength = 0.5f,
                },
                new GameAction("changeText", "Change Text")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.ChangeText(e["failText"], e["text"], e["youText"]);
                    },
                    parameters = new List<Param>() {
                        new Param("failText", true, "Automatic Text", "Display text depending on if you hit an input or not"),
                        new Param("text", "Do what the others do.", "Text", "Set the text on the screen"),
                        new Param("youText", "You", "You Text", "Set the text that orginally says \"You\""),
                    },
                },
                new GameAction("changeColor", "Change Color")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.ChangeColor(e["color1"], e["color2"], e["octoColor"], e.length, e["bgInstant"]);
                    },
                    parameters = new List<Param>() {
                        new Param("color1", new Color(1f, 0.87f, 0.24f), "Background Start Color", "Set the beginning background color"),
                        new Param("color2", new Color(1f, 0.87f, 0.24f), "Background End Color", "Set the end background color"),
                        new Param("bgInstant", false, "Instant Background?", "Set the end background color instantly"),
                        new Param("octoColor", new Color(0.97f, 0.235f, 0.54f), "Octopodes Color", "Set the octopodes' colors"),
                        new Param("squeezedColor", new Color(1f, 0f, 0f), "Squeezed Color", "Set the octopodes' colors when they're squeezed"),
                    },
                    resizable = true,
                },
                new GameAction("octopusModifiers", "Octopus Modifiers")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.OctopusModifiers(e.beat, e["oct1x"], e["oct2x"], e["oct3x"], e["oct1y"], e["oct2y"], e["oct3y"], e["oct1"], e["oct2"], e["oct3"]);
                    },
                    parameters = new List<Param>() {
                        new Param("oct1", true, "Show Octopus 1", "Should the first octopus be enabled?"),
                        new Param("oct1x", new EntityTypes.Float(-10, 10, -4.64f), "X Octopus 1", "Change Octopus 1's X"),
                        new Param("oct1y", new EntityTypes.Float(-10, 10, 2.5f), "Y Octopus 1", "Change Octopus 1's Y"),
                        new Param("oct2", true, "Show Octopus 2", "Should the second octopus be enabled?"),
                        new Param("oct2x", new EntityTypes.Float(-10, 10, -0.637f), "X Octopus 2", "Change Octopus 2's X"),
                        new Param("oct2y", new EntityTypes.Float(-10, 10, 0f), "Y Octopus 1", "Change Octopus 2's Y"),
                        new Param("oct3", true, "Show Octopus 3", "Should the third octopus be enabled?"),
                        new Param("oct3x", new EntityTypes.Float(-10, 10, 3.363f), "X Octopus 3", "Change Octopus 3's X"),
                        new Param("oct3y", new EntityTypes.Float(-10, 10, -2.5f), "Y Octopus 1", "Change Octopus 3's Y"),
                    },
                    defaultLength = 0.5f,
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_OctopusMachine;
    public partial class OctopusMachine : Minigame
    {
        [Header("Objects")]
        [SerializeField] SpriteRenderer bg;
        [SerializeField] Material mat;
        [SerializeField] TMP_Text Text;
        [SerializeField] TMP_Text YouText;
        [SerializeField] GameObject YouArrow;
        
        [Header("Octopodes")]
        [SerializeField] Octopus Octopus1;
        [SerializeField] Octopus Octopus2;
        [SerializeField] Octopus Octopus3;

        [Header("Static Variables")]
        static Color backgroundColor = new Color(1, 0.87f, 0.24f);
        public static Color octopodesColor = new Color(0.97f, 0.235f, 0.54f);
        public static Color octopodesSqueezedColor = new Color(1f, 0f, 0f);
        public static bool queuePrepare;
        public static bool canPrepare = true;

        [Header("Variables")]
        Tween bgColorTween;
        bool intervalStarted;
        float intervalStartBeat;
        float beatInterval = 1f;
        public bool hasHit;
        public bool hasMissed;
        public int bopIterate = 0;
        Octopus[] octopodes = new Octopus[3];

        static List<float> queuedSqueezes = new List<float>();
        static List<float> queuedReleases = new List<float>();
        static List<float> queuedPops = new List<float>();

        public static OctopusMachine instance;

        public enum Bops
        {
            Bop,
            Happy,
            Angry,
        }

        void Awake()
        {
            instance = this;

            octopodes[0] = Octopus1;
            octopodes[1] = Octopus2;
            octopodes[2] = Octopus3;
        }

        private void Start() 
        {
            bg.color = backgroundColor;
            foreach (var octo in octopodes) octo.AnimationColor(0);
            canPrepare = true;
        }

        void OnDestroy()
        {
            if (queuedSqueezes.Count > 0) queuedSqueezes.Clear();
            if (queuedReleases.Count > 0) queuedReleases.Clear();
            if (queuedPops.Count > 0) queuedPops.Clear();
            
            mat.SetColor("_ColorAlpha", new Color(0.97f, 0.235f, 0.54f));
        }

        private void Update() 
        {
            if (queuePrepare) {
                foreach (var octo in octopodes) octo.queuePrepare = true;
                queuePrepare = false;
            }
        }

        public static void Prepare(float beat, float prepBeats)
        {
            if (!canPrepare) return;
            if (GameManager.instance.currentGame != "octopusMachine") {
                OctopusMachine.queuePrepare = true;
                OctopusMachine.canPrepare = false;
            } else {
                if (instance.Octopus3.isPreparing) return;
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                    new BeatAction.Action(beat - prepBeats, delegate { 
                        OctopusMachine.queuePrepare = true;
                        OctopusMachine.canPrepare = false;
                    })
                });
            }
        }

        public void ChangeText(bool autoText, string text, string youText)
        {
            if (autoText) Text.text = hasMissed ? "Wrong! \nTry Again!" : "Good!";
            Text.text = text;
            YouText.text = youText;
            YouArrow.SetActive(youText != "");
        }

        public void Squeeze(float beat, float length)
        {
            if (!intervalStarted) StartInterval(beat, length);
            Octopus1.OctoAction("Squeeze");
            queuedSqueezes.Add(beat - intervalStartBeat);
        }

        public void Release(float beat, float length)
        {
            if (!Octopus1.isSqueezed) return;
            if (!intervalStarted) StartInterval(beat, length);
            Octopus1.OctoAction("Release");
            queuedReleases.Add(beat - intervalStartBeat);
        }

        public void Pop(float beat, float length)
        {
            if (!Octopus1.isSqueezed) return;
            if (!intervalStarted) StartInterval(beat, length);
            Octopus1.OctoAction("Pop");
            queuedPops.Add(beat - intervalStartBeat);
        }

        public void Bop(float beat, float length, int whichBop, bool doesBop, bool autoBop)
        {
            foreach (var octo in octopodes) {
                octo.Bop(autoBop);
                octo.cantBop = autoBop;
            }
        }

        public void FadeBackgroundColor(Color color, float beats)
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

        public void ChangeColor(Color bgStart, Color bgEnd, Color octoColor, float beats, bool bgInstant)
        {
            FadeBackgroundColor(bgStart, 0f);
            if (!bgInstant) FadeBackgroundColor(bgEnd, beats);
            backgroundColor = bgEnd;
            octopodesColor = octoColor;
            mat.SetColor("_ColorAlpha", octoColor);
        }

        public void OctopusModifiers(float beat, float oct1x, float oct2x, float oct3x, float oct1y, float oct2y, float oct3y, bool oct1, bool oct2, bool oct3)
        {
            Octopus1.OctopusModifiers(oct1x, oct1y, oct1);
            Octopus2.OctopusModifiers(oct2x, oct2y, oct2);
            Octopus3.OctopusModifiers(oct3x, oct3y, oct3);
        }

        public void ForceSqueeze()
        {
            foreach (var octo in octopodes) octo.ForceSqueeze();
        }
        
        public void StartInterval(float beat, float length)
        {
            intervalStartBeat = beat;
            beatInterval = length;
            intervalStarted = true;
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + length, delegate {
                    PassTurn(beat + length);
                }),
            });
        }

        public void PassTurn(float beat)
        {
            //if (queuedSqueezes.Count == 0) return;
            intervalStarted = false;
            hasMissed = false;
            var queuedInputs = new List<BeatAction.Action>();
            foreach (var squeeze in queuedSqueezes) {
                queuedInputs.Add(new BeatAction.Action(beat + squeeze, delegate { Octopus2.OctoAction("Squeeze"); }));
                ScheduleInput(beat, beatInterval + squeeze, InputType.STANDARD_DOWN, SqueezeHit, Miss, Miss);
            }
            foreach (var release in queuedReleases) {
                queuedInputs.Add(new BeatAction.Action(beat + release, delegate { Octopus2.OctoAction("Release"); }));
                ScheduleInput(beat, beatInterval + release, InputType.STANDARD_UP, ReleaseHit, Miss, Miss);
            }
            foreach (var pop in queuedPops) {
                queuedInputs.Add(new BeatAction.Action(beat + pop, delegate { Octopus2.OctoAction("Pop"); }));
                ScheduleInput(beat, beatInterval + pop, InputType.STANDARD_UP, PopHit, Miss, Miss);
            }
            queuedSqueezes.Clear();
            queuedReleases.Clear();
            queuedPops.Clear();

            // thanks to ras for giving me this line of code
            // i do NOT understand how it works
            queuedInputs.Sort((s1, s2) => s1.beat.CompareTo(s2.beat));
            BeatAction.New(gameObject, queuedInputs);
        }
        
        private void SqueezeHit(PlayerActionEvent caller, float state)
        {
            Octopus3.OctoAction("Squeeze");
        }

        private void ReleaseHit(PlayerActionEvent caller, float state)
        {
            Octopus3.OctoAction("Release");
        }

        private void PopHit(PlayerActionEvent caller, float state)
        {
            Octopus3.OctoAction("Pop");
        }

        private void Miss(PlayerActionEvent caller)
        {
            hasMissed = true;
        }
    }
}