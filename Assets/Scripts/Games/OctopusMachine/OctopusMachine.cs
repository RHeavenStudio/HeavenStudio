using HeavenStudio.Util;
using System;
using System.Collections.Generic;
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
                        new Param("singleBop", true, "Bop", "Toggle if the octopodes should bop for the duration of this event. Since this event is not stretchable, they will only bop once."),
                        new Param("keepBop", false, "Bop (Auto)", "Toggle if the octopodes should automatically bop until another Bop event is reached."),
                        new Param("whichBop", OctopusMachine.Bops.Bop, "Which Bop", "Set the type of bop."),
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
                        new Param("shouldPrep", true, "Prepare", "Toggle if the octopodes should automatically prepare for this cue.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "prepBeats" })
                        }),
                        new Param("prepBeats", new EntityTypes.Float(0, 4, 1), "Prepare Beats", "Set how many beats before the cue the octopodes should prepare."),
                    },
                    preFunctionLength = 4f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (e["shouldPrep"]) OctopusMachine.queuePrepare = e.beat - e["prepBeats"];
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
                        OctopusMachine.instance.AutoAction(e["forceBop"], e["autoBop"], e["autoText"], e["hitText"], e["missText"]); 
                    },
                    parameters = new List<Param>() {
                        new Param("forceBop", true, "Force Bop", "Toggle if a bop should be forced to play, even if an animation is already playing."),
                        new Param("autoBop", true, "Hit/Miss Bop", "Toggle if the octopodes should bop depending on if you hit or missed the cues."),
                        new Param("autoText", true, "Display Text", "Toggle if text should display depending on if you hit or missed the cues.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "hitText", "missText" })
                        }),
                        new Param("hitText", "Good!", "Hit Text", "Set the text to display if you hit the cues."),
                        new Param("missText", "Wrong! Try again!", "Miss Text", "Set the text to display if you missed the cues."),
                    },
                },
                new GameAction("forceSqueeze", "Force Squeeze")
                {
                    function = delegate { OctopusMachine.instance.ForceSqueeze(); }
                },
                new GameAction("prepare", "Prepare")
                {
                    function = delegate { OctopusMachine.queuePrepare = eventCaller.currentEntity.beat; },
                    defaultLength = 0.5f,
                },
                new GameAction("bubbles", "Bubbles")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.BubbleToggle(e["isInstant"], e["setActive"], e["particleStrength"], e["particleSpeed"]);
                    },
                    parameters = new List<Param>() {
                        new Param("isInstant", true, "Instant", "Toggle if the particles should start or stop instantly."),
                        new Param("setActive", OctopusMachine.Actives.Activate, "State", "Set if the bubbles should appear or disappear.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)OctopusMachine.Actives.Activate, new string[] { "particleStrength" })
                        }),
                        new Param("particleStrength", new EntityTypes.Float(0, 25, 3), "Intensity", "Set the intensity of the bubbles."),
                        new Param("particleSpeed", new EntityTypes.Float(0, 25, 5), "Speed", "Set the speed of the bubbles."),
                    },
                },
                new GameAction("changeText", "Text")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.ChangeText(e["text"], e["youText"]);
                    },
                    parameters = new List<Param>() {
                        new Param("text", "Do what the others do.", "Text", "Set the text on the screen."),
                        new Param("youText", "You", "Player Text", "Set the text that points to the player."),
                    },
                },
                new GameAction("changeColor", "Change Color")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.BackgroundColor(e.beat, e.length, e["color1"], e["color2"], e["octoColor"], e["squeezedColor"], e["ease"]);
                    },
                    parameters = new List<Param>() {
                        new Param("color1", new Color(1f, 0.87f, 0.24f), "Start BG Color", "Set the color at the start of the event."),
                        new Param("color2", new Color(1f, 0.87f, 0.24f), "End BG Color", "Set the color at the end of the event."),
                        new Param("octoColor", new Color(0.97f, 0.235f, 0.54f), "Octopodes' Color", "Set the octopodes' colors."),
                        new Param("squeezedColor", new Color(1f, 0f, 0f), "Squeezed Color", "Set the octopodes' colors when they're squeezed."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action."),
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
                        new Param("oct1", true, "Show Octopus 1", "Toggle if the first octopus should be enabled.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "oct1x", "oct1y" })
                        }),
                        new Param("oct1x", new EntityTypes.Float(-10, 10, -4.64f), "X Octopus 1", "Set the position on the X axis of octopus 1."),
                        new Param("oct1y", new EntityTypes.Float(-10, 10, 2.5f), "Y Octopus 1", "Set the position on the Y axis of octopus 1."),
                        new Param("oct2", true, "Show Octopus 2", "Toggle if the second octopus should be enabled.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "oct2x", "oct2y" })
                        }),
                        new Param("oct2x", new EntityTypes.Float(-10, 10, -0.637f), "X Octopus 2", "Set the position on the X axis of octopus 2."),
                        new Param("oct2y", new EntityTypes.Float(-10, 10, 0f), "Y Octopus 2", "Set the position on the Y axis of octopus 2."),
                        new Param("oct3", true, "Show Octopus 3", "Toggle if the third octopus should be enabled.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "oct3x", "oct3y" })
                        }),
                        new Param("oct3x", new EntityTypes.Float(-10, 10, 3.363f), "X Octopus 3", "Set the position on the X axis of octopus 3."),
                        new Param("oct3y", new EntityTypes.Float(-10, 10, -2.5f), "Y Octopus 3", "Set the position on the Y axis of octopus 3."),
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
        public static double queuePrepare = double.MaxValue;

        [Header("Variables")]
        public bool hasMissed;
        public int bopStatus = 0;
        int bopIterate = 0;
        bool intervalStarted;
        bool autoAction;
        double intervalStartBeat;
        float beatInterval = 1f;

        static List<double> queuedSqueezes = new();
        static List<double> queuedReleases = new();
        static List<double> queuedPops = new();

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
            SetupBopRegion("octopusMachine", "bop", "keepBop");
        }

        private void Start() 
        {
            foreach (var octo in octopodes) octo.AnimationColor(0);
            bopStatus = 0;
        }

        void OnDestroy()
        {
            if (queuedSqueezes.Count > 0) queuedSqueezes.Clear();
            if (queuedReleases.Count > 0) queuedReleases.Clear();
            if (queuedPops.Count > 0) queuedPops.Clear();
            queuePrepare = double.MaxValue;
            
            mat.SetColor("_ColorAlpha", new Color(0.97f, 0.235f, 0.54f));

            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        public override void OnBeatPulse(double beat)
        {
            if (bopIterate >= 3)
            {
                bopStatus =
                bopIterate = 0;
                autoAction = false;
            }

            if (autoAction) bopIterate++;

            bool keepBop = BeatIsInBopRegion(beat);

            foreach (var octo in octopodes)
            {
                octo.cantBop = !keepBop;
                octo.RequestBop();
            }
        }

        private void Update() 
        {
            BackgroundColorUpdate();
            if (queuePrepare <= Conductor.instance.songPositionInBeatsAsDouble) {
                foreach (var octo in octopodes) octo.queuePrepare = queuePrepare;
                if (Text.text is "Wrong! Try Again!" or "Good!") Text.text = "";
                queuePrepare = double.MaxValue;
            }
        }

        public void ChangeText(string text, string youText)
        {
            Text.text = text;
            YouText.text = youText;
            YouArrow.SetActive(youText != "");
        }

        public void AutoAction(bool forceBop, bool autoBop, bool autoText, string hitText, string missText)
        {
            autoAction = true;
            if (autoBop) bopStatus = hasMissed ? 2 : 1;
            if (autoText) Text.text = hasMissed ? missText : hitText;
            foreach (var octo in octopodes) {
                if (forceBop) octo.PlayAnimation(bopStatus);
                octo.cantBop = false;
            }
            hasMissed = false;
        }

        public void BubbleToggle(bool isInstant, int setActive, float particleStrength, float particleSpeed)
        {
            foreach (var bubble in Bubbles) {
                bubble.gameObject.SetActive(true);

                var main = bubble.main;
                main.prewarm = isInstant;
                main.simulationSpeed = particleSpeed / 10;

                var emm = bubble.emission;
                emm.rateOverTime = particleStrength;
                
                if (setActive == 1) bubble.Stop(true, isInstant ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
                else bubble.Play();
            }
        }

        public void OctoAction(double beat, float length, string action)
        {
            if (action != "Squeeze" && !octopodes[0].isSqueezed) return;
            if (!intervalStarted) StartInterval(beat, length);
            octopodes[0].OctoAction(action);

            // var queuedList = queuedSqueezes;
            // if (action == "Release") queuedList = queuedReleases;
            // else if (action == "Pop") queuedList = queuedPops;

            var queuedList = action switch {
                "Release" => queuedReleases,
                "Pop" => queuedPops,
                _ => queuedSqueezes,
            };

            queuedList.Add(beat - intervalStartBeat);
        }

        public void Bop(double length, int whichBop, bool singleBop, bool keepBop)
        {
            foreach (var octo in octopodes) {
                if (singleBop) octo.PlayAnimation(whichBop);
                if (keepBop) bopStatus = whichBop;
            }
        }

        private double colorStartBeat = -1;
        private float colorLength = 0f;
        private Color colorStart = new Color(1, 0.87f, 0.24f); //obviously put to the default color of the game
        private Color colorEnd = new Color(1, 0.87f, 0.24f);
        private Util.EasingFunction.Ease colorEase; //putting Util in case this game is using jukebox

        //call this in update
        private void BackgroundColorUpdate()
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeat, colorLength));

            var func = Util.EasingFunction.GetEasingFunction(colorEase);

            float newR = func(colorStart.r, colorEnd.r, normalizedBeat);
            float newG = func(colorStart.g, colorEnd.g, normalizedBeat);
            float newB = func(colorStart.b, colorEnd.b, normalizedBeat);

            bg.color = new Color(newR, newG, newB);
        }

        public void BackgroundColor(double beat, float length, Color colorStartSet, Color colorEndSet, Color octoColor, Color octoSqueezeColor, int ease)
        {
            colorStartBeat = beat;
            colorLength = length;
            colorStart = colorStartSet;
            colorEnd = colorEndSet;
            colorEase = (Util.EasingFunction.Ease)ease;
            octopodesColor = octoColor;
            octopodesSqueezedColor = octoSqueezeColor;
            foreach (var octo in octopodes) octo.AnimationColor(octo.isSqueezed ? 1 : 0);
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var bgColor = GameManager.instance.Beatmap.Entities.FindLast(c => c.datamodel == "octopusMachine/changeColor" && c.beat < beat);
            if (bgColor != null) {
                BackgroundColor(bgColor.beat, bgColor.length, bgColor["color1"], bgColor["color2"], bgColor["octoColor"], bgColor["squeezedColor"], bgColor["ease"]);
            }
        }

        public override void OnPlay(double beat)
        {
            PersistColor(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            PersistColor(beat);
        }

        public void OctopusModifiers(double beat, float oct1x, float oct2x, float oct3x, float oct1y, float oct2y, float oct3y, bool oct1, bool oct2, bool oct3)
        {
            octopodes[0].OctopusModifiers(oct1x, oct1y, oct1);
            octopodes[1].OctopusModifiers(oct2x, oct2y, oct2);
            octopodes[2].OctopusModifiers(oct3x, oct3y, oct3);
        }

        public void ForceSqueeze()
        {
            foreach (var octo in octopodes) octo.ForceSqueeze();
        }
        
        public void StartInterval(double beat, float length)
        {
            intervalStartBeat = beat;
            beatInterval = length;
            intervalStarted = true;
            BeatAction.New(this, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + length, delegate {
                    PassTurn(beat + length);
                }),
            });
        }

        public void PassTurn(double beat)
        {
            intervalStarted = false;
            var queuedInputs = new List<BeatAction.Action>();
            foreach (var squeeze in queuedSqueezes) {
                queuedInputs.Add(new BeatAction.Action(beat + squeeze, delegate { octopodes[1].OctoAction("Squeeze"); }));
                ScheduleInput(beat, beatInterval + squeeze, InputAction_BasicPress, SqueezeHit, Miss, Miss);
            }
            foreach (var release in queuedReleases) {
                queuedInputs.Add(new BeatAction.Action(beat + release, delegate { octopodes[1].OctoAction("Release"); }));
                ScheduleInput(beat, beatInterval + release, InputAction_BasicRelease, ReleaseHit, Miss, Miss);
            }
            foreach (var pop in queuedPops) {
                queuedInputs.Add(new BeatAction.Action(beat + pop, delegate { octopodes[1].OctoAction("Pop"); }));
                ScheduleInput(beat, beatInterval + pop, InputAction_FlickRelease, PopHit, Miss, Miss);
            }
            queuedSqueezes.Clear();
            queuedReleases.Clear();
            queuedPops.Clear();

            // thanks to ras for giving me this line of code
            // i do NOT understand how it works
            queuedInputs.Sort((s1, s2) => s1.beat.CompareTo(s2.beat));
            BeatAction.New(this, queuedInputs);
        }
        
        private void SqueezeHit(PlayerActionEvent caller, float state)
        {
            octopodes[2].OctoAction("Squeeze");
            if (state <= -1f || state >= 1f) SoundByte.PlayOneShotGame("nearMiss");
        }

        private void ReleaseHit(PlayerActionEvent caller, float state)
        {
            octopodes[2].OctoAction("Release");
            if (state <= -1f || state >= 1f) SoundByte.PlayOneShotGame("nearMiss");
        }

        private void PopHit(PlayerActionEvent caller, float state)
        {
            octopodes[2].OctoAction("Pop");
            if (state <= -1f || state >= 1f) SoundByte.PlayOneShotGame("nearMiss");
        }

        private void Miss(PlayerActionEvent caller) { }
    }
}