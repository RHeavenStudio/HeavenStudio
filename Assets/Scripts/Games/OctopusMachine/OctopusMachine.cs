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
            return new Minigame("octopusMachine", "Octopus Machine", "FFf362B", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Bop(e.length, e["whichBop"], e["singleBop"], e["keepBop"]);
                    },
                    parameters = new List<Param>() {
                        new Param("whichBop", OctopusMachine.Bops.Bop, "Which Bop", "Plays a specific bop type"),
                        new Param("singleBop", true, "Single Bop", "Plays one bop"),
                        new Param("keepBop", false, "Keep Bopping", "Keeps playing the specified bop type"),
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
                        OctopusMachine.instance.OctoAction(e.beat, e.length, "Squeeze");
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
                        OctopusMachine.instance.OctoAction(e.beat, e.length, "Release");
                    },
                    resizable = true,
                    priority = 1,
                },
                new GameAction("pop", "Pop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.OctoAction(e.beat, e.length, "Pop");
                    },
                    resizable = true,
                    priority = 1,
                },
                new GameAction("automaticActions", "Automatic Actions")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.AutoAction(e["autoBop"], e["autoText"], e["hitText"], e["missText"]); 
                    },
                    parameters = new List<Param>() {
                        new Param("autoBop", true, "Hit/Miss Bop", "Plays a bop depending on if you hit or missed the cues."),
                        new Param("autoText", true, "Display Text", "Displays text depending on if you hit or missed the cues."),
                        new Param("hitText", "Good!", "Hit Text", "The text to display if you hit the cues."),
                        new Param("missText", "Wrong! n/ Try again!", "Miss Text", "The text to display if you missed the cues."),
                    },
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
                new GameAction("bubbles", "Bubbles")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.BubbleToggle(e["isInstant"], e["setActive"], e["particleStrength"], e["particleSpeed"]);
                    },
                    parameters = new List<Param>() {
                        new Param("isInstant", true, "Instant", "Will the bubbles disappear appear?"),
                        new Param("setActive", OctopusMachine.Actives.Activate, "Activate or Deactivate", "Will the bubbles disappear or appear?"),
                        new Param("particleStrength", new EntityTypes.Float(0, 25, 3), "Bubble Intensity", "The amount of bubbles"),
                        new Param("particleSpeed", new EntityTypes.Float(0, 25, 5), "Bubble Speed", "The speed of the bubbles"),
                    },
                },
                new GameAction("changeText", "Change Text")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.ChangeText(e["text"], e["youText"]);
                    },
                    parameters = new List<Param>() {
                        new Param("text", "Do what the others do.", "Text", "Set the text on the screen"),
                        new Param("youText", "You", "You Text", "Set the text that orginally says \"You\""),
                    },
                },
                new GameAction("changeColor", "Change Color")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.ChangeColor(e["color1"], e["color2"], e["octoColor"], e["squeezedColor"], e.length, e["bgInstant"]);
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
                new GameAction("octopusModifiers", "Octopus Positions")
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
                        new Param("oct2y", new EntityTypes.Float(-10, 10, 0f), "Y Octopus 2", "Change Octopus 2's Y"),
                        new Param("oct3", true, "Show Octopus 3", "Should the third octopus be enabled?"),
                        new Param("oct3x", new EntityTypes.Float(-10, 10, 3.363f), "X Octopus 3", "Change Octopus 3's X"),
                        new Param("oct3y", new EntityTypes.Float(-10, 10, -2.5f), "Y Octopus 3", "Change Octopus 3's Y"),
                    },
                    defaultLength = 0.5f,
                },
            },
            new List<string>() {"ntr", "repeat"},
            "ntrcork", "en",
            new List<string>() {}
            );
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
        [SerializeField] ParticleSystem[] Bubbles;
        [SerializeField] GameObject YouArrow;
        [SerializeField] TMP_Text YouText;
        [SerializeField] TMP_Text Text;
        [SerializeField] Octopus[] octopodes;

        [Header("Static Variables")]
        static Color backgroundColor = new Color(1, 0.87f, 0.24f);
        public static Color octopodesColor = new Color(0.97f, 0.235f, 0.54f);
        public static Color octopodesSqueezedColor = new Color(1f, 0f, 0f);
        public static bool queuePrepare;

        [Header("Variables")]
        public bool hasMissed;
        public int bopStatus = 0;
        Tween bgColorTween;
        int bopIterate = 0;
        bool intervalStarted;
        bool autoAction;
        float intervalStartBeat;
        float beatInterval = 1f;
        float lastReportedBeat;

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

        public enum Actives
        {
            Activate,
            Deactivate,
        }

        void Awake()
        {
            instance = this;
        }

        private void Start() 
        {
            bg.color = backgroundColor;
            foreach (var octo in octopodes) octo.AnimationColor(0);
            bopStatus = 0;
        }

        void OnDestroy()
        {
            if (queuedSqueezes.Count > 0) queuedSqueezes.Clear();
            if (queuedReleases.Count > 0) queuedReleases.Clear();
            if (queuedPops.Count > 0) queuedPops.Clear();
            
            mat.SetColor("_ColorAlpha", new Color(0.97f, 0.235f, 0.54f));

            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        private void Update() 
        {
            if (queuePrepare) {
                foreach (var octo in octopodes) octo.queuePrepare = true;
                if (Text.text is "Wrong! \nTry Again!" or "Good!") Text.text = "";
                queuePrepare = false;
            }

            if (Conductor.instance.ReportBeat(ref lastReportedBeat))
            {
                if (bopIterate >= 3) {
                    bopStatus =
                    bopIterate = 0;
                    autoAction = false;
                }
                
                if (autoAction) bopIterate++;
            }
        }

        public static void Prepare(float beat, float prepBeats)
        {
            if (GameManager.instance.currentGame != "octopusMachine") {
                OctopusMachine.queuePrepare = true;
            } else {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                    new BeatAction.Action(beat - prepBeats, delegate { 
                        OctopusMachine.queuePrepare = true;
                    })
                });
            }
        }

        public void ChangeText(string text, string youText)
        {
            Text.text = text;
            YouText.text = youText;
            YouArrow.SetActive(youText != "");
        }

        public void AutoAction(bool autoBop, bool autoText, string hitText, string missText)
        {
            autoAction = true;
            if (autoBop) bopStatus = hasMissed ? 2 : 1;
            if (autoText) Text.text = hasMissed ? missText : hitText;
            foreach (var octo in octopodes) octo.cantBop = false;
            hasMissed = false;
        }

        public void BubbleToggle(bool isInstant, int setActive, float particleStrength, float particleSpeed)
        {
            foreach (var bubble in Bubbles) {
                bubble.gameObject.SetActive(true);

                var main = bubble.main;
                main.prewarm = isInstant;
                main.simulationSpeed = particleSpeed/10;

                var emm = bubble.emission;
                emm.rateOverTime = particleStrength;
                
                if (setActive == 1) bubble.Stop(true, isInstant ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
                else bubble.Play();
            }
        }

        public void OctoAction(float beat, float length, string action)
        {
            if (action != "Squeeze" && !octopodes[0].isSqueezed) return;
            if (!intervalStarted) StartInterval(beat, length);
            octopodes[0].OctoAction(action);

            var queuedList = queuedSqueezes;
            if (action == "Release") queuedList = queuedReleases;
            else if (action == "Pop") queuedList = queuedPops;

            queuedList.Add(beat - intervalStartBeat);
        }

        public void Bop(float length, int whichBop, bool singleBop, bool keepBop)
        {
            foreach (var octo in octopodes) {
                if (singleBop) octo.PlayAnimation(whichBop);
                if (keepBop) bopStatus = whichBop;
                octo.cantBop = !keepBop;
            }
        }

        public void FadeBackgroundColor(Color color, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (bgColorTween != null)
                bgColorTween.Kill(true);

            if (seconds == 0) bg.color = color;
            else bgColorTween = bg.DOColor(color, seconds);
        }

        public void ChangeColor(Color bgStart, Color bgEnd, Color octoColor, Color octoSqueezedColor, float beats, bool bgInstant)
        {
            FadeBackgroundColor(bgStart, 0f);
            if (!bgInstant) FadeBackgroundColor(bgEnd, beats);
            backgroundColor = bgEnd;
            octopodesColor = octoColor;
            octopodesSqueezedColor = octoSqueezedColor;
            foreach (var octo in octopodes) octo.AnimationColor(octo.isSqueezed ? 1 : 0);
        }

        public void OctopusModifiers(float beat, float oct1x, float oct2x, float oct3x, float oct1y, float oct2y, float oct3y, bool oct1, bool oct2, bool oct3)
        {
            octopodes[0].OctopusModifiers(oct1x, oct1y, oct1);
            octopodes[1].OctopusModifiers(oct2x, oct2y, oct2);
            octopodes[2].OctopusModifiers(oct3x, oct3y, oct3);
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
            intervalStarted = false;
            var queuedInputs = new List<BeatAction.Action>();
            foreach (var squeeze in queuedSqueezes) {
                queuedInputs.Add(new BeatAction.Action(beat + squeeze, delegate { octopodes[1].OctoAction("Squeeze"); }));
                ScheduleInput(beat, beatInterval + squeeze, InputType.STANDARD_DOWN, SqueezeHit, Miss, Miss);
            }
            foreach (var release in queuedReleases) {
                queuedInputs.Add(new BeatAction.Action(beat + release, delegate { octopodes[1].OctoAction("Release"); }));
                ScheduleInput(beat, beatInterval + release, InputType.STANDARD_UP, ReleaseHit, Miss, Miss);
            }
            foreach (var pop in queuedPops) {
                queuedInputs.Add(new BeatAction.Action(beat + pop, delegate { octopodes[1].OctoAction("Pop"); }));
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
            octopodes[2].OctoAction("Squeeze");
        }

        private void ReleaseHit(PlayerActionEvent caller, float state)
        {
            octopodes[2].OctoAction("Release");
        }

        private void PopHit(PlayerActionEvent caller, float state)
        {
            octopodes[2].OctoAction("Pop");
        }

        private void Miss(PlayerActionEvent caller)
        {
            hasMissed = true;
        }
    }
}