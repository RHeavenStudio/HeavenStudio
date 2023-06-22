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
                new GameAction("countIn", "8 Beat Count-In")
                {
                    preFunction = delegate { if (!eventCaller.currentEntity["mute"] && AgbNightWalk.IsValidCountIn(eventCaller.currentEntity)) AgbNightWalk.CountInSound(eventCaller.currentEntity.beat); },
                    defaultLength = 8,
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
                    parameters = new List<Param>()
                    {
                        new Param("type", AgbNightWalk.PlatformType.Lollipop, "Type")
                    }
                },
                new GameAction("noJump", "No Jumping")
                {
                    defaultLength = 4,
                    resizable = true
                }
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
        public static AgbNightWalk instance;
        public AgbPlayYan playYan;
        [SerializeField] private AgbPlatformHandler platformHandler;
        [NonSerialized] public double countInBeat = -1;
        [Header("Curves")]
        [SerializeField] SuperCurveObject.Path[] jumpPaths;
        private struct HeightEvent
        {
            public double beat;
            public int value;
        }
        List<HeightEvent> heightEntityEvents = new();
        [NonSerialized] public Dictionary<double, PlatformType> platformTypes = new();

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
                    platformTypes.Add(typeEvent.beat, type);
                }
                
            }
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
            platformHandler.SpawnPlatforms(beat);
        }

        public override void OnPlay(double beat)
        {
            SetCountInBeat(beat);
            platformHandler.SpawnPlatforms(beat);
        }

        private void SetCountInBeat(double beat)
        {
            List<RiqEntity> countInEvents = EventCaller.GetAllInGameManagerList("nightWalkAgb", new string[] { "countIn" });
            if (countInEvents.Count > 0)
            {
                var allEnds = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" });
                if (allEnds.Count == 0)
                {
                    countInBeat = countInEvents[0].beat;
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
                    }
                }
            }
            UpdateBalloons(beat);
        }

        public static bool IsValidCountIn(RiqEntity countInEntity)
        {
            List<RiqEntity> countInEvents = EventCaller.GetAllInGameManagerList("nightWalkAgb", new string[] { "countIn" });
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
                    double beat = -1;
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
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(countInBeat, delegate { playYan.PopBalloon(0, beat >= countInBeat); }),
                    new BeatAction.Action(countInBeat + 2, delegate { playYan.PopBalloon(1, beat >= countInBeat + 2); }),
                    new BeatAction.Action(countInBeat + 4, delegate { playYan.PopBalloon(2, beat >= countInBeat + 4); }),
                    new BeatAction.Action(countInBeat + 5, delegate { playYan.PopBalloon(3, beat >= countInBeat + 5); }),
                    new BeatAction.Action(countInBeat + 6, delegate { playYan.PopBalloon(4, beat >= countInBeat + 6); }),
                    new BeatAction.Action(countInBeat + 7, delegate { playYan.PopBalloon(5, beat >= countInBeat + 7); }),
                    new BeatAction.Action(countInBeat + 8, delegate { playYan.PopBalloon(6, beat >= countInBeat + 8); }),
                });
            }
            else
            {
                playYan.PopAll();
            }
        }

        public static void CountInSound(double beat)
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
    }
}


