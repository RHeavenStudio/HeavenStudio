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
            return new Minigame("octopusMachine", "Octopus Machine \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "FFf362B", false, false, new List<GameAction>()
            {
                new GameAction("Bop", "Bop")
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
                    defaultLength = 0.5f,
                },
                new GameAction("StartInterval", "Start Interval")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.StartInterval(e.beat, e.length);
                    },
                    defaultLength = 1f,
                    resizable = true,
                    priority = 5,
                },
                new GameAction("Squeeze", "Squeeze")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Squeeze(e.beat, e.length);
                    },
                    resizable = true,
                    parameters = new List<Param>() {
                        new Param("shouldPrep", true, "Prepare?", "Plays a prepare animation before the cue."),
                        new Param("prepBeats", new EntityTypes.Float(0, 5, 1), "Prepare Beats", "How many beats before the cue does the octopus prepare?"),
                    },
                    preFunctionLength = 1f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.PlayAnimation(e.beat, 4, true);
                    },
                    priority = 1,
                },
                new GameAction("Release", "Release")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Release(e.beat, e.length);
                    },
                    resizable = true,
                    priority = 1,
                },
                new GameAction("Pop", "Pop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Pop(e.beat, e.length);
                    },
                    resizable = true,
                    priority = 1,
                },
                new GameAction("ForceSqueeze", "Force Squeeze")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.ForceSqueeze();
                    },
                    defaultLength = 1f,
                },
                new GameAction("Prepare", "Prepare")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.PlayAnimation(e.beat, 4, true);
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("ChangeText", "Change Text")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.ChangeText(e["failText"], e["text"], e["youText"]);
                    },
                    parameters = new List<Param>() {
                        new Param("failText", true, "Game-Accurate Text", "Display text depending on if you hit an input or not"),
                        new Param("text", "Do what the others do.", "Text", "Set the text on the screen"),
                        new Param("youText", "You", "You Text", "Set the text that orginally says \"You\""),
                    },
                },
                new GameAction("GameplayModifiers", "Gameplay Modifiers")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.GameplayModifiers(e.beat, e["color"], e["octoColor"], e["oct1"], e["oct2"], e["oct3"]);
                    },
                    parameters = new List<Param>() {
                        new Param("color", new Color(1f, 0.87f, 0.24f), "Background Color", "Set the background color"),
                        new Param("octoColor", new Color(1f, 0.34f, 0.62f), "Octopodes Color", "Set the octopodes' colors"),
                        new Param("oct1", true, "Show Octopus 1", "Should the first octopus be enabled?"),
                        new Param("oct2", true, "Show Octopus 2", "Should the second octopus be enabled?"),
                        new Param("oct3", true, "Show Octopus 3", "Should the third octopus be enabled?"),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("MoveOctopodes", "Move Octopodes")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.MoveOctopodes(e.beat, e["oct1x"], e["oct2x"], e["oct3x"], e["oct1y"], e["oct2y"], e["oct3y"]);
                    },
                    parameters = new List<Param>() {
                        new Param("oct1x", new EntityTypes.Float(-10, 10, -4.5f), "X Octopus 1", "Change Octopus 1's X"),
                        new Param("oct1y", new EntityTypes.Float(-10, 10, 2.5f), "Y Octopus 1", "Change Octopus 1's Y"),
                        new Param("oct2x", new EntityTypes.Float(-10, 10, -0.5f), "X Octopus 2", "Change Octopus 2's X"),
                        new Param("oct2y", new EntityTypes.Float(-10, 10, 0f), "Y Octopus 1", "Change Octopus 2's Y"),
                        new Param("oct3x", new EntityTypes.Float(-10, 10, 3.5f), "X Octopus 3", "Change Octopus 3's X"),
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
        [SerializeField] SpriteRenderer Background;
        [SerializeField] TMP_Text Text;
        [SerializeField] TMP_Text YouText;
        
        [Header("Octopodes")]
        public Octopus Octopus1;
        public Octopus Octopus2;
        public Octopus Octopus3;

        [Header("Variables")]
        bool intervalStarted;
        bool autoText = true;
        float intervalStartBeat;
        float beatInterval = 1f;
        public bool hasHit;
        public bool hasMissed;
        public bool bopOn = true;
        Octopus[] octopodes;

        static List<float> queuedSqueezes = new List<float>();
        static List<float> queuedReleases = new List<float>();
        static List<float> queuedPops = new List<float>();

        private List<DynamicBeatmap.DynamicEntity> allIntervalEvents = new List<DynamicBeatmap.DynamicEntity>();

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
            var intervalEvents = EventCaller.GetAllInGameManagerList("octopusMachine", new string[] { "StartInterval" });
            List<DynamicBeatmap.DynamicEntity> tempEvents = new List<DynamicBeatmap.DynamicEntity>();
            for (int i = 0; i < intervalEvents.Count; i++)
            {
                if (intervalEvents[i].beat + intervalEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    tempEvents.Add(intervalEvents[i]);
                }
            }

            allIntervalEvents = tempEvents;
        }

        private void Start() 
        {
            octopodes = FindObjectsByType<Octopus>(FindObjectsSortMode.InstanceID);
            Debug.Log(octopodes[0]);
            Debug.Log(octopodes[1]);
            Debug.Log(octopodes[2]);
        }

        private void Update()
        {
            /*
            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(intervalStartBeat, beatInterval);
                if (normalizedBeat >= 1f && intervalStarted)
                {
                    PassTurn(intervalStartBeat + beatInterval, 0f);
                }
                if (allIntervalEvents.Count > 0)
                {
                    if (startIntervalIndex < allIntervalEvents.Count && startIntervalIndex >= 0)
                    {
                        if (Conductor.instance.songPositionInBeats >= allIntervalEvents[startIntervalIndex].beat)
                        {
                            StartInterval(allIntervalEvents[startIntervalIndex].beat, allIntervalEvents[startIntervalIndex].length);
                            startIntervalIndex++;
                        }
                    }
                }
            }
            */
        }

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedSqueezes.Count > 0) queuedSqueezes.Clear();
                if (queuedReleases.Count > 0) queuedReleases.Clear();
                if (queuedPops.Count > 0) queuedPops.Clear();
            }
        }

        public void PlayAnimation(float beat, int whichBop, bool prepare = false)
        {
            Octopus1.PlayAnimation(whichBop);
            Octopus2.PlayAnimation(whichBop);
            Octopus3.PlayAnimation(whichBop);
        }

        public void ChangeText(bool failText, string text, string youText)
        {
            autoText = failText;
            Text.text = text;
            YouText.text = youText;
        }

        public void Squeeze(float beat, float length)
        {
            if (!intervalStarted) StartInterval(beat, length);
            Octopus1.Squeeze();
            queuedSqueezes.Add(beat - intervalStartBeat);
        }

        public void Release(float beat, float length)
        {
            if (!intervalStarted) StartInterval(beat, length);
            Octopus1.Release();
            queuedReleases.Add(beat - intervalStartBeat);
        }

        public void Pop(float beat, float length)
        {
            if (!intervalStarted) StartInterval(beat, length);
            Octopus1.Pop();
            queuedPops.Add(beat - intervalStartBeat);
        }

        public void Bop(float beat, float length, int whichBop, bool doesBop, bool autoBop)
        {
            bopOn = autoBop;
            if (autoBop) {
                hasHit = whichBop == 1 ? true : false;
                hasMissed = whichBop == 2 ? true : false;
            }
            foreach (var item in octopodes) {
                item.Bop(autoBop);
            }
        }

        public void GameplayModifiers(float beat, Color bgColor, Color octoColor, bool oct1, bool oct2, bool oct3)
        {
            Background.color = bgColor;
            
            Octopus1.GameplayModifiers(oct1, octoColor);
            Octopus2.GameplayModifiers(oct2, octoColor);
            Octopus3.GameplayModifiers(oct3, octoColor);
        }

        public void MoveOctopodes(float beat, float oct1x, float oct2x, float oct3x, float oct1y, float oct2y, float oct3y)
        {
            Octopus1.MoveOctopodes(oct1x, oct1y);
            Octopus2.MoveOctopodes(oct2x, oct2y);
            Octopus3.MoveOctopodes(oct3x, oct3y);
        }

        public void ForceSqueeze()
        {
            Octopus1.ForceSqueeze();
            Octopus2.ForceSqueeze();
            Octopus3.ForceSqueeze();
        }
        
        public void StartInterval(float beat, float length)
        {
            intervalStartBeat = beat;
            beatInterval = length;
            intervalStarted = true;
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate
                {
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
                queuedInputs.Add(new BeatAction.Action(beat + squeeze, delegate {
                    Octopus2.Squeeze();
                    ScheduleInput(beat, beatInterval + squeeze, InputType.STANDARD_DOWN, SqueezeHit, Miss, Miss);
                }));
            }
            foreach (var release in queuedReleases) {
                queuedInputs.Add(new BeatAction.Action(beat + release, delegate {
                    Octopus2.Release();
                    ScheduleInput(beat, beatInterval + release, InputType.STANDARD_DOWN, ReleaseHit, Miss, Miss);
                }));
            }
            foreach (var pop in queuedPops) {
                queuedInputs.Add(new BeatAction.Action(beat + pop, delegate {
                    Octopus2.Pop();
                    ScheduleInput(beat, beatInterval + pop, InputType.STANDARD_DOWN, PopHit, Miss, Miss);
                }));
            }
            queuedSqueezes.Clear();
            queuedReleases.Clear();
            queuedPops.Clear();
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(queuedInputs[queuedInputs.Count - 1].beat + 1f, delegate {
                    if (autoText) Text.text = (hasMissed ? "Wrong! n/ Try again!" : "Good!");
                }),
            });
            BeatAction.New(gameObject, queuedInputs);
        }
        
        private void SqueezeHit(PlayerActionEvent caller, float state)
        {
            Octopus3.Squeeze();
        }

        private void ReleaseHit(PlayerActionEvent caller, float state)
        {
            Octopus3.Release();
        }

        private void PopHit(PlayerActionEvent caller, float state)
        {
            Octopus3.Pop();
        }

        private void Miss(PlayerActionEvent caller)
        {
            hasMissed = true;
        }
    }
}