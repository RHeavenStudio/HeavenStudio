using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
// using GhostlyGuy's Balls;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrOctopusMachineLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("OctopusMachine", "Octopus Machine \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "FFf362B", false, false, new List<GameAction>()
            {
                new GameAction("Bop", "Bop")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.Bop(e.beat, e.length, e["whichBop"], e["bop"], e["autoBop"]); 
                    },
                    parameters = new List<Param>()                     
                    {
                        new Param("whichBop", OctopusMachine.Bops.Bop, "Which Bop?", "Plays a specific bop type"),
                        new Param("bop", true, "Single Bop", "Plays one bop"),
                        new Param("autoBop", false, "Keep Bopping?", "Keeps playing the specified bop type"),
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
                },
                new GameAction("Squeeze", "Squeeze")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Squeeze(e.beat, e.length);
                    },
                    resizable = true,
                    parameters = new List<Param>()                     
                    {
                        new Param("shouldPrep", true, "Prepare?", "Plays a prepare animation before the cue."),
                        new Param("prepBeats", new EntityTypes.Float(0, 5, 1), "Prepare Beats", "How many beats before the cue does the octopus prepare?"),
                    },
                    preFunctionLength = 1f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (e["shouldPrep"]) {
                        float offset = e.beat - e["prepBeats"];
                        BeatAction.New(OctopusMachine.instance.gameObject, new List<BeatAction.Action>() {
                            new BeatAction.Action(offset, delegate {
                                OctopusMachine.instance.PlayAnimation(e.beat, 4, true);
                            })
                        }); };
                    },
                },
                new GameAction("Release", "Release")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.Release(e.beat); 
                    },
                    resizable = true,
                },
                new GameAction("Pop", "Pop")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.Pop(e.beat); 
                    },
                    resizable = true,
                },
                new GameAction("ForceClose", "Force Close")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.ForceClose(e.beat); 
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
                new GameAction("GameplayModifiers", "Gameplay Modifiers")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.GameplayModifiers(e.beat, e["color"], e["octoColor"], e["oct1"], e["oct2"], e["oct3"]);
                    },
                    parameters = new List<Param>()                     
                    {
                        new Param("color", new Color(1f, 0.84f, 0), "Background Color", "Set the background color"),
                        new Param("octoColor", new Color(1f, 0.145f, 0.5f), "Octopodes Color", "Set the octopode's colors"),
                        new Param("oct1", true, "Show Octopus 1?", "Keep bopping using the selected bop"),
                        new Param("oct2", true, "Show Octopus 2?", "Keep bopping using the selected bop"),
                        new Param("oct3", true, "Show Octopus 3?", "Keep bopping using the selected bop"),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("MoveOctopodes", "Move Octopodes")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.MoveOctopodes(e.beat, e["oct1x"], e["oct2x"], e["oct3x"], e["oct1y"], e["oct2y"], e["oct3y"]);
                    },
                    parameters = new List<Param>()                     
                    {
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
        [Header("Sprite Renderers")]
        [SerializeField] SpriteRenderer Background;
        
        [Header("Octopodes")]
        public Octopus Octopus1;
        public Octopus Octopus2;
        public Octopus Octopus3;

        [Header("Variables")]
        bool intervalStarted;
        float intervalStartBeat;
        float beatInterval = 4f;
        public bool hasHit;
        public bool hasMissed;
        public bool bopOn = true;
        public float lastReportedBeat = 0f;
        Octopus[] octopodes;

        static List<QueuedInputs> queuedSqueezes = new List<QueuedInputs>();
        static List<QueuedInputs> queuedReleases = new List<QueuedInputs>();
        static List<QueuedInputs> queuedPops = new List<QueuedInputs>();
        public struct QueuedInputs
        {
            public float startBeat;
            public float length;
        }

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
            octopodes = FindObjectsByType<Octopus>(FindObjectsSortMode.None);
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

        public void Squeeze(float beat, float length)
        {
            if (!intervalStarted)
            {
                StartInterval(beat, length);
            }
            Octopus1.Squeeze();
            queuedSqueezes.Add(new QueuedInputs
            {
                startBeat = beat - intervalStartBeat,
                length = length,
            });
        }

        public void Release(float beat)
        {
            Debug.Log("release");
        }

        public void Pop(float beat)
        {
            Debug.Log("pop");
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
            /*
            if (doesBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            
                        })
                    });
                }
            }
            */
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

        public void ForceClose(float beat)
        {
            
        }

        
        public void StartInterval(float beat, float length)
        {
            intervalStartBeat = beat;
            beatInterval = length;
            intervalStarted = true;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length + beatInterval, delegate
                {
                    PassTurn(beat + length);
                }),
            });
        }

        public void PassTurn(float beat)
        {
            if (queuedSqueezes.Count == 0) return;
            intervalStarted = false;
            hasMissed = false;
            foreach (var item in queuedSqueezes)
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        Octopus2.Squeeze();
                        ScheduleInput(beat, beatInterval, InputType.STANDARD_DOWN, Hit, Miss, Out);
                    }),
                });
            }
            queuedSqueezes.Clear();
        }
        
        private void Hit(PlayerActionEvent caller, float state)
        {
            Octopus3.Squeeze();
        }

        private void Miss(PlayerActionEvent caller)
        {
            
        }

        private void Out(PlayerActionEvent caller) 
        {

        }
    }
}