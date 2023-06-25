using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using Jukebox;
using System;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class AgbNightWalkLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("nightWalkAgb", "Night Walk (GBA)", "FFFFFF", false, false, new List<GameAction>()
            {
                new GameAction("countIn8", "8 Beat Count-In")
                {
                    preFunction = delegate { if (!eventCaller.currentEntity["mute"] && AgbNightWalk.IsValidCountIn(eventCaller.currentEntity)) AgbNightWalk.CountInSound8(eventCaller.currentEntity.beat); },
                    defaultLength = 8,
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Cowbell")
                    }
                },
                new GameAction("countIn4", "4 Beat Count-In")
                {
                    preFunction = delegate { if (!eventCaller.currentEntity["mute"] && AgbNightWalk.IsValidCountIn(eventCaller.currentEntity)) AgbNightWalk.CountInSound4(eventCaller.currentEntity.beat); },
                    defaultLength = 4,
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Cowbell")
                    }
                },
                new GameAction("height", "Platform Height")
                {
                    parameters = new List<Param>()
                    {
                        new Param("value", new EntityTypes.Integer(-10, 10, 1), "Height Units"),
                        new Param("rmin", new EntityTypes.Integer(-10, 10, 0), "Random Units (Minimum)"),
                        new Param("rmax", new EntityTypes.Integer(-10, 10, 0), "Random Units (Maximum)"),
                    }
                },
                new GameAction("type", "Platform Type")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        if (e["type"] == (int)AgbNightWalk.PlatformType.Umbrella)
                        {
                            AgbNightWalk.FillSound(e.beat, (AgbNightWalk.FillType)e["fill"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("type", AgbNightWalk.PlatformType.Lollipop, "Type"),
                        new Param("fill", AgbNightWalk.FillType.None, "Umbrella Drum Pattern")
                    },
                    preFunctionLength = 1
                },
                new GameAction("fish", "Electric Fish")
                {
                    preFunction = delegate { if (!eventCaller.currentEntity["mute"]) AgbNightWalk.FishSound(eventCaller.currentEntity.beat); },
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Cue")
                    },
                    preFunctionLength = 1
                },
                new GameAction("end", "End")
                {
                    parameters = new List<Param>()
                    {
                        new Param("minAmount", new EntityTypes.Integer(0, 10000, 20), "Minimum Jumps Required"),
                        new Param("minAmountP", new EntityTypes.Integer(0, 10000, 100), "Minimum Jumps Required (Persistent)"),
                    }
                },
                new GameAction("noJump", "No Jumping")
                {
                    defaultLength = 4,
                    resizable = true
                },
                new GameAction("walkingCountIn", "Walking Count-In")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; AgbNightWalk.WalkingCountIn(e.beat, e.length); },
                    defaultLength = 4,
                    resizable = true
                },
                new GameAction("evolveAmount", "Star Evolve Amount")
                {
                    function = delegate { AgbNightWalk.instance.evolveAmount = eventCaller.currentEntity["am"]; },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("am", new EntityTypes.Integer(0, 100, 1), "Amount", "How many stars will evolve when play-yan jumps?"),
                    }
                },
                new GameAction("forceEvolve", "Force Star Evolve")
                {
                    function = delegate { var e = eventCaller.currentEntity; AgbNightWalk.instance.ForceEvolve(e.beat, e.length, e["am"], e["repeat"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("am", new EntityTypes.Integer(0, 100, 1), "Star Amount", "How many stars will evolve?"),
                        new Param("repeat", new EntityTypes.Integer(0, 100, 1), "Repeat Amount", "How many times will this event repeat?"),
                    }
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_AgbNightWalk;

    public class AgbNightWalk : Minigame
    {
        public enum PlatformType
        {
            Lollipop = 2,
            Umbrella = 3
        }

        public enum FillType
        {
            None,
            Pattern1,
            Pattern2,
            Pattern3,
        }

        public static AgbNightWalk instance;
        public AgbPlayYan playYan;
        [SerializeField] private AgbPlatformHandler platformHandler;
        public AgbStarHandler starHandler;
        [NonSerialized] public double countInBeat = double.MinValue;
        [NonSerialized] public float countInLength = 8;
        [Header("Curves")]
        [SerializeField] SuperCurveObject.Path[] jumpPaths;
        private struct HeightEvent
        {
            public double beat;
            public int value;
        }
        List<HeightEvent> heightEntityEvents = new();
        [NonSerialized] public Dictionary<double, TypeEvent> platformTypes = new();
        private List<double> fishBeats = new();
        public struct TypeEvent
        {
            public PlatformType platformType;
            public FillType fillType;
        }

        [NonSerialized] public int evolveAmount = 1;
        [NonSerialized] public int hitJumps;
        [NonSerialized] public static int hitJumpsPersist;

        [NonSerialized] public double endBeat = double.MaxValue;
        [NonSerialized] public int requiredJumps;
        [NonSerialized] public int requiredJumpsP;

        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in jumpPaths)
            {
                if (path.preview)
                {
                    playYan.DrawEditorGizmo(path);
                }
            }
        }

        public SuperCurveObject.Path GetPath(string name)
        {
            foreach (SuperCurveObject.Path path in jumpPaths)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(SuperCurveObject.Path);
        }

        private void Awake()
        {
            instance = this;
            List<RiqEntity> heightEvents = EventCaller.GetAllInGameManagerList("nightWalkAgb", new string[] { "height" });
            foreach (var heightEvent in heightEvents)
            {
                heightEntityEvents.Add(new HeightEvent()
                {
                    beat = heightEvent.beat,
                    value = heightEvent["value"] + UnityEngine.Random.Range(heightEvent["rmin"], heightEvent["rmax"] + 1)
                });
            }
            List<RiqEntity> typeEvents = EventCaller.GetAllInGameManagerList("nightWalkAgb", new string[] { "type" });
            foreach (var typeEvent in typeEvents)
            {
                if (!platformTypes.ContainsKey(typeEvent.beat)) 
                {
                    PlatformType type = (PlatformType)typeEvent["type"];
                    FillType fill = (FillType)typeEvent["fill"];
                    platformTypes.Add(typeEvent.beat, new TypeEvent
                    {
                        fillType = fill,
                        platformType = type
                    });
                }
                
            }
            List<RiqEntity> fishEvents = EventCaller.GetAllInGameManagerList("nightWalkAgb", new string[] { "fish" });
            foreach(var fishEvent in fishEvents)
            {
                fishBeats.Add(fishEvent.beat);
            }
        }

        private void OnDestroy()
        {
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
            if (!Conductor.instance.isPlaying) hitJumpsPersist = 0;
        }

        public void ForceEvolve(double beat, float length, int starAmount, int repeatAmount)
        {
            List<BeatAction.Action> actions = new();
            for (int i = 0; i < repeatAmount; i++)
            {
                actions.Add(new BeatAction.Action(beat + (length * i), delegate
                {
                    starHandler.Evolve(starAmount);
                }));
            }
            BeatAction.New(instance.gameObject, actions);
        }

        public bool FishOnBeat(double beat)
        {
            return fishBeats.Contains(beat);
        }

        public int FindHeightUnitsAtBeat(double beat)
        {
            List<HeightEvent> tempEvents = heightEntityEvents.FindAll(e => e.beat <= beat);
            int height = 0;
            foreach (var heightEvent in tempEvents)
            {
                height += heightEvent.value;
            }
            return height;
        }

        public bool ShouldNotJumpOnBeat(double beat)
        {
            List<RiqEntity> heightEvents = EventCaller.GetAllInGameManagerList("nightWalkAgb", new string[] { "noJump" });
            return heightEvents.Find(x => beat >= x.beat && beat < x.beat + x.length) != null;
        }

        public override void OnGameSwitch(double beat)
        {
            SetCountInBeat(beat);
            SetEndValues(beat);
            platformHandler.SpawnPlatforms(beat);
        }

        public override void OnPlay(double beat)
        {
            SetCountInBeat(beat);
            SetEndValues(beat);
            platformHandler.SpawnPlatforms(beat);
            hitJumpsPersist = 0;
        }

        public void SetEndValues(double beat)
        {
            List<RiqEntity> endEvents = EventCaller.GetAllInGameManagerList("nightWalkAgb", new string[] { "end" });
            if (endEvents.Count > 0)
            {
                var allEnds = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" });
                if (allEnds.Count == 0)
                {
                    endBeat = endEvents[^1].beat;
                    requiredJumps = endEvents[^1]["minAmount"];
                    requiredJumpsP = endEvents[^1]["minAmountP"];
                }
                else
                {
                    allEnds.Sort((x, y) => x.beat.CompareTo(y.beat));
                    double nextSwitchBeat = double.MaxValue;
                    foreach (var end in allEnds)
                    {
                        if (end.datamodel.Split(2) == "nightWalkAgb") continue;
                        if (end.beat > beat)
                        {
                            nextSwitchBeat = end.beat;
                            break;
                        }
                    }
                    var tempEvents = endEvents.FindAll(e => e.beat >= beat && e.beat < nextSwitchBeat);
                    if (tempEvents.Count > 0)
                    {
                        endBeat = tempEvents[^1].beat;
                        requiredJumps = tempEvents[^1]["minAmount"];
                        requiredJumpsP = tempEvents[^1]["minAmountP"];
                    }
                }
            }
        }

        public static void FishSound(double beat)
        {
            if (GameManager.instance.currentGame == "nightWalkAgb" && instance.platformHandler.PlatformsStopped()) return;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("nightWalkAgb/fish1", beat - 1),
                new MultiSound.Sound("nightWalkAgb/fish2", beat - 0.75),
                new MultiSound.Sound("nightWalkAgb/fish3", beat - 0.5),
            }, forcePlay: true);
        }

        public static void WalkingCountIn(double beat, float length)
        {
            List<MultiSound.Sound> sounds = new();
            for (int i = 0; i < length; i++)
            {
                sounds.Add(new MultiSound.Sound("nightWalkAgb/boxKick", beat + i));
                sounds.Add(new MultiSound.Sound("nightWalkAgb/open1", beat + 0.5 + i));
            }
            MultiSound.Play(sounds.ToArray(), forcePlay: true);
        }

        public static void FillSound(double beat, FillType fill)
        {
            if (GameManager.instance.currentGame == "nightWalkAgb" && instance.platformHandler.PlatformsStopped()) return;
            double third = 1.0 / 3.0;
            switch (fill)
            {
                case FillType.None:
                    break;
                case FillType.Pattern1:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("nightWalkAgb/fill1A", beat - (third * 2)),
                        new MultiSound.Sound("nightWalkAgb/fill1B", beat - 0.5),
                        new MultiSound.Sound("nightWalkAgb/fill1C", beat - third),
                        new MultiSound.Sound("nightWalkAgb/fill1D", beat - (third * 0.5)),
                    }, forcePlay: true);
                    break;
                case FillType.Pattern2:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("nightWalkAgb/fill2A", beat - (third * 2)),
                        new MultiSound.Sound("nightWalkAgb/fill2B", beat - 0.5),
                        new MultiSound.Sound("nightWalkAgb/fill2C", beat - (third * 0.5)),
                    }, forcePlay: true);
                    break;
                case FillType.Pattern3:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("nightWalkAgb/fill3A", beat - (third * 2)),
                        new MultiSound.Sound("nightWalkAgb/fill3B", beat - 0.5),
                    }, forcePlay: true);
                    break;
            }
        }

        private void SetCountInBeat(double beat)
        {
            List<RiqEntity> countInEvents = EventCaller.GetAllInGameManagerList("nightWalkAgb", new string[] { "countIn8", "countIn4" });
            if (countInEvents.Count > 0)
            {
                var allEnds = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" });
                if (allEnds.Count == 0)
                {
                    countInBeat = countInEvents[^1].beat;
                    countInLength = countInEvents[^1].length;
                }
                else
                {
                    allEnds.Sort((x, y) => x.beat.CompareTo(y.beat));
                    double nextSwitchBeat = double.MaxValue;
                    foreach (var end in allEnds)
                    {
                        if (end.datamodel.Split(2) == "nightWalkAgb") continue;
                        if (end.beat > beat)
                        {
                            nextSwitchBeat = end.beat;
                            break;
                        }
                    }
                    List<RiqEntity> tempEvents = new();
                    foreach (var countIn in countInEvents)
                    {
                        if (countIn.beat < nextSwitchBeat)
                        {
                            tempEvents.Add(countIn);
                        }
                    }
                    if (tempEvents.Count > 0)
                    {
                        tempEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
                        countInBeat = tempEvents[tempEvents.Count - 1].beat;
                        countInLength = tempEvents[tempEvents.Count - 1].length;
                    }
                }
            }
            UpdateBalloons(beat);
        }

        public static bool IsValidCountIn(RiqEntity countInEntity)
        {
            List<RiqEntity> countInEvents = EventCaller.GetAllInGameManagerList("nightWalkAgb", new string[] { "countIn8", "countIn4" });
            if (countInEvents.Count > 0)
            {
                var allEnds = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" });
                if (allEnds.Count == 0)
                {
                    return countInEntity == countInEvents[0];
                }
                else
                {
                    allEnds.Sort((x, y) => x.beat.CompareTo(y.beat));
                    List<RiqEntity> tempEnds = new();
                    double beat = double.MinValue;
                    for (int i = 0; i < allEnds.Count; i++)
                    {
                        if (allEnds[i].datamodel.Split(2) == "nightWalkAgb")
                        {
                            if (allEnds[i].beat >= countInEntity.beat)
                            {
                                tempEnds.Add(allEnds[i]);
                            }
                        }
                    }
                    if (tempEnds.Count > 0) beat = tempEnds[0].beat;
                    double nextSwitchBeat = double.MaxValue;
                    foreach (var end in allEnds)
                    {
                        if (end.datamodel.Split(2) == "nightWalkAgb") continue;
                        if (end.beat > beat)
                        {
                            nextSwitchBeat = end.beat;
                            break;
                        }
                    }
                    List<RiqEntity> tempEvents = new();
                    foreach (var countIn in countInEvents)
                    {
                        if (countIn.beat < nextSwitchBeat)
                        {
                            tempEvents.Add(countIn);
                        }
                    }
                    if (tempEvents.Count == 0) return true;
                    tempEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
                    return countInEntity == tempEvents[tempEvents.Count - 1];
                }
            }
            return false;
        }

        private void UpdateBalloons(double beat)
        {
            if (countInBeat != -1)
            {
                if (countInLength == 8)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(countInBeat, delegate { playYan.PopBalloon(0, beat > countInBeat); }),
                        new BeatAction.Action(countInBeat + 2, delegate { playYan.PopBalloon(1, beat > countInBeat + 2); }),
                        new BeatAction.Action(countInBeat + 4, delegate { playYan.PopBalloon(2, beat > countInBeat + 4); }),
                        new BeatAction.Action(countInBeat + 5, delegate { playYan.PopBalloon(3, beat > countInBeat + 5); }),
                        new BeatAction.Action(countInBeat + 6, delegate { playYan.PopBalloon(4, beat > countInBeat + 6); }),
                        new BeatAction.Action(countInBeat + 7, delegate { playYan.PopBalloon(5, beat > countInBeat + 7); }),
                        new BeatAction.Action(countInBeat + 8, delegate { playYan.PopBalloon(6, beat > countInBeat + 8); }),
                    });
                }
                else
                {
                    playYan.PopBalloon(0, true);
                    playYan.PopBalloon(1, true);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(countInBeat, delegate { playYan.PopBalloon(2, beat > countInBeat); }),
                        new BeatAction.Action(countInBeat + 1, delegate { playYan.PopBalloon(3, beat > countInBeat + 1); }),
                        new BeatAction.Action(countInBeat + 2, delegate { playYan.PopBalloon(4, beat > countInBeat + 2); }),
                        new BeatAction.Action(countInBeat + 3, delegate { playYan.PopBalloon(5, beat > countInBeat + 3); }),
                        new BeatAction.Action(countInBeat + 4, delegate { playYan.PopBalloon(6, beat > countInBeat + 4); }),
                    });
                }
            }
            else
            {
                playYan.PopAll();
            }
        }

        public static void CountInSound8(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("count-ins/cowbell", beat),
                new MultiSound.Sound("count-ins/cowbell", beat + 2),
                new MultiSound.Sound("count-ins/cowbell", beat + 4),
                new MultiSound.Sound("count-ins/cowbell", beat + 5),
                new MultiSound.Sound("count-ins/cowbell", beat + 6),
                new MultiSound.Sound("count-ins/cowbell", beat + 7),
            }, false, true);
        }

        public static void CountInSound4(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("count-ins/cowbell", beat),
                new MultiSound.Sound("count-ins/cowbell", beat + 1),
                new MultiSound.Sound("count-ins/cowbell", beat + 2),
                new MultiSound.Sound("count-ins/cowbell", beat + 3),
            }, false, true);
        }
    }
}


