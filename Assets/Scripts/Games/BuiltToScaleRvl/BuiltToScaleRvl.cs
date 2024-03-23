using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlBuiltLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("builtToScaleRvl", "Built To Scale (Wii)", "1ad21a", false, false, new List<GameAction>()
            {
                new GameAction("spawn rod", "Spawn Rod")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; BuiltToScaleRvl.PreSpawnRodfromDirection(e.beat, e.length, e["direction"], e["id"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("direction", BuiltToScaleRvl.Direction.Left, "Direction", "Set the direction in which the rod will come out."),
                        new Param("id", new EntityTypes.Integer(0, 4, 0), "Rod ID", "Set the ID of the rod to spawn. Rods with the same ID cannot spawn at the same time."),
                    },
                },
                new GameAction("shoot rod", "Shoot Rod")
                {
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("id", new EntityTypes.Integer(0, 4, 0), "Rod ID", "Set the ID of the rod to shoot."),
                    },
                },
                new GameAction("out sides", "Bounce Out Sides")
                {
                    // function = delegate { var e = eventCaller.currentEntity; BuiltToScaleRvl.instance.OutRod(e.beat, e["id"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("id", new EntityTypes.Integer(0, 4, 0), "Rod ID", "Set the ID of the rod to out."),
                    },
                },
                new GameAction("custom bounce", "Custom Bounce")
                {
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("target", BuiltToScaleRvl.Target.First, "Target", "Set the target in which the rod will bounce."),
                        new Param("id", new EntityTypes.Integer(0, 4, 0), "Rod ID", "Set the ID of the rod to bounce."),
                    },
                },
            }, new List<string>() { "rvl", "normal" }, "rvlbuilt", "en", new List<string>() { });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_BuiltToScaleRvl;
    public class BuiltToScaleRvl : Minigame
    {
        public Animator[] blockAnims;
        [SerializeField] GameObject baseRod;
        [SerializeField] GameObject baseLeftSquare;
        [SerializeField] GameObject baseRightSquare;
        [SerializeField] GameObject baseAssembled;
        public Transform widgetHolder;

        const double WIDGET_SEEK_TIME = 16.0;

        public enum Direction {
            Left,
            Right,
        }
        public enum Target {
            OuterLeft,
            First,
            Second,
            Third,
            Fourth,
            OuterRight,
        }

        public static BuiltToScaleRvl instance;

        const int IAAltDownCat = IAMAXCAT;

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }

        public static PlayerInput.InputAction InputAction_FlickAltPress =
            new("NtrBuiltAltFlickAltPress", new int[] { IAAltDownCat, IAFlickCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchFlick, IA_BatonAltPress);
        
        private void Awake()
        {
            instance = this;
        }

        private double gameStartBeat = double.MinValue, gameEndBeat = double.MaxValue;
        public static List<QueuedRod> queuedRods = new List<QueuedRod>();
        List<ScheduledWidget> scheduledWidgets = new List<ScheduledWidget>();
        public List<Rod> spawnedRods = new List<Rod>();

        public struct QueuedRod
        {
            public double beat;
            public double length;
            public int currentPos;
            public int nextPos;
            public int id;
        }

        struct ScheduledWidget
        {
            public double beat;
            public double length;
            public int currentPos;
            public int nextPos;
            public int id;
            public CustomBounceItem[] bounceItems;
            public int endTime;
            public bool isShoot;
        }

        struct ScheduledSquare
        {
            public double targetBeat;
            public double lengthBeat;
        }

        public class CustomBounceItem
        {
            public int time = -1;
            public int pos;
        }

        public override void OnPlay(double beat)
        {
            queuedRods.Clear();
            OnBeginning(beat);
        }
        public override void OnGameSwitch(double beat)
        {
            OnBeginning(beat);
        }
        private void OnDestroy() {
            queuedRods.Clear();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }
        private void OnBeginning(double beat)
        {
            gameStartBeat = beat;
            var firstEnd = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).Find(x => x.beat > gameStartBeat);
            gameEndBeat = firstEnd?.beat ?? gameEndBeat;

            scheduledWidgets.Clear();
            // foreach (var evt in events)
            // {
            //     if (evt.length == 0) continue;
            //     int patternDivisions = (int)Math.Ceiling(evt.length / WIDGET_SEEK_TIME);
            //     var pattern = new ScheduledPattern
            //     {
            //         beat = evt.beat + (PATTERN_SEEK_TIME * i),
            //         length = Math.Min(evt.length - (PATTERN_SEEK_TIME * i), PATTERN_SEEK_TIME),
            //         type = patternType
            //     };
            //     scheduledSquares.Add(pattern);
            // }
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedRods.Count != 0)
                {
                    foreach (QueuedRod rod in queuedRods)
                    {
                        SpawnRod(rod.beat, rod.length, rod.currentPos, rod.nextPos, rod.id);
                    }
                    queuedRods.Clear();
                }
            }
            else
            {
                if ((!cond.isPaused) && queuedRods.Count != 0)
                {
                    queuedRods.Clear();
                }
            }
        }

        public static void PreSpawnRodfromDirection(double beat, double length, int direction, int id)
        {
            int currentPos = direction switch {
                (int)Direction.Left => -1,
                (int)Direction.Right => 4,
                _ => throw new System.NotImplementedException()
            };
            int nextPos = direction switch {
                (int)Direction.Left => 0,
                (int)Direction.Right => 3,
                _ => throw new System.NotImplementedException()
            };
            if (GameManager.instance.currentGame == "builtToScaleRvl")
            {
                instance.SpawnRod(beat, length, currentPos, nextPos, id);
            }
            else
            {
                queuedRods.Add(new QueuedRod()
                {
                    beat = beat,
                    length = length,
                    currentPos = currentPos,
                    nextPos = nextPos,
                    id = id,
                });
            }
        }
        
        public void SpawnRod(double beat, double length, int currentPos, int nextPos, int id)
        {
            if (spawnedRods.Any(x => x.ID == id)) return;            
            var newRod = Instantiate(baseRod, widgetHolder).GetComponent<Rod>();
            spawnedRods.Add(newRod);

            newRod.startBeat = beat;
            newRod.lengthBeat = length;
            newRod.currentPos = currentPos;
            newRod.nextPos = nextPos;
            newRod.ID = id;

            List<CustomBounceItem> bounceItems = CalcRodBounce(beat, length, id);
            AddBounceOutSides(beat, length, currentPos, nextPos, id, ref bounceItems);

            bool isShoot;
            int rodEndTime = CalcRodEndTime(beat, length, currentPos, nextPos, id, ref bounceItems, out isShoot);
            newRod.customBounce = bounceItems.ToArray();
            newRod.shootTime = rodEndTime;
            newRod.isShoot = isShoot;
            if (isShoot)
            {
                double rodEndBeat = beat + length * rodEndTime;
                SoundByte.PlayOneShotGame("builtToScaleRvl/prepare", rodEndBeat - 2*length);
                newRod.Squares = SpawnSquare(rodEndBeat, id);
            }

            newRod.Init();
            newRod.gameObject.SetActive(true);
        }

        private Square[] SpawnSquare(double targetBeat, int id)
        {
            var newLeftSquare = Instantiate(baseLeftSquare, widgetHolder).GetComponent<Square>();
            var newRightSquare = Instantiate(baseRightSquare, widgetHolder).GetComponent<Square>();
            newLeftSquare.startBeat = this.gameStartBeat;
            newRightSquare.startBeat = this.gameStartBeat;
            newLeftSquare.targetBeat = targetBeat;
            newRightSquare.targetBeat = targetBeat;
            newLeftSquare.gameObject.SetActive(true);
            newRightSquare.gameObject.SetActive(true);
            newLeftSquare.Init();
            newRightSquare.Init();

            return new Square[]{newLeftSquare, newRightSquare};
        }

        public void SpawnAssembled()
        {
            var newAssembled =Instantiate(baseAssembled, widgetHolder);
            newAssembled.SetActive(true);
        }

        private List<CustomBounceItem> CalcRodBounce(double beat, double length, int id)
        {
            var bounceItems = new List<CustomBounceItem>();
            var events = EventCaller.GetAllInGameManagerList("builtToScaleRvl", new string[] { "custom bounce" }).FindAll(x => x.beat > beat && x["id"] == id);
            
            foreach(var evt in events)
            {
                var bounceEventTime = (int)Math.Ceiling((evt.beat-beat)/length);
                bounceItems.Add(new CustomBounceItem{
                    time = bounceEventTime,
                    pos = evt["target"] switch {
                        (int)Target.OuterLeft => -1,
                        (int)Target.First => 0,
                        (int)Target.Second => 1,
                        (int)Target.Third => 2,
                        (int)Target.Fourth => 3,
                        (int)Target.OuterRight => 4,
                        _ => throw new System.NotImplementedException()
                    },
                });
            }
            return bounceItems;
        }

        private void AddBounceOutSides(double beat, double length, int currentPos, int nextPos, int id, ref List<CustomBounceItem> bounceItems)
        {
            var firstOut = EventCaller.GetAllInGameManagerList("builtToScaleRvl", new string[] { "out sides" }).Find(x => x.beat > beat && x["id"] == id);
            if (firstOut is not null)
            {
                int earliestOutTime = (int)Math.Ceiling((firstOut.beat - beat)/length);
                int current = currentPos, next = nextPos;
                int outTime;
                var bounceItemsArray = bounceItems.ToArray();
                for (int time = 0; ; time++) {
                    if (current is 0 or 3 && time >= earliestOutTime) {
                        bounceItems.Add(new CustomBounceItem{
                            time = time,
                            pos = current switch {
                                0 => -1,
                                3 => 4,
                                _ => throw new System.NotImplementedException()
                            },
                        });
                        break;
                    }
                    int following = getFollowingPos(current, next, time+1, bounceItemsArray);
                    current = next;
                    next = following;
                }
            }
        }

        private int CalcRodEndTime(double beat, double length, int currentPos, int nextPos, int id, ref List<CustomBounceItem> bounceItems, out bool isShoot)
        {
            isShoot = false;
            int earliestEndTime = int.MaxValue;
            var firstShoot = EventCaller.GetAllInGameManagerList("builtToScaleRvl", new string[] { "shoot rod" }).Find(x => x.beat > beat && x["id"] == id);
            if (firstShoot is not null)
            {
                earliestEndTime = (int)Math.Ceiling((firstShoot.beat - beat)/length);
                isShoot = true;
            }
            
            bounceItems = bounceItems.FindAll(x => x.time < earliestEndTime);
            bounceItems.Sort((x, y) => x.time.CompareTo(y.time));
            var bounceOutSide = bounceItems.Find(x => x.pos is -1 or 4);
            if (bounceOutSide is not null)
            {
                earliestEndTime = bounceOutSide.time;
                isShoot = false;
            }
            if (!isShoot) return earliestEndTime;

            int current = currentPos, next = nextPos;
            int shootTime;
            var bounceItemsArray = bounceItems.ToArray();
            for (int time = 0; ; time++) {
                if (current == 2 && time >= earliestEndTime) {
                    shootTime = time;
                    break;
                }
                int following = getFollowingPos(current, next, time+1, bounceItemsArray);
                current = next;
                next = following;
            }
            return shootTime;
        }

        public static int getFollowingPos(int currentPos, int nextPos, int nextTime, CustomBounceItem[] bounceItems)
        {
            var bounce = Array.Find(bounceItems, x => x.time == nextTime);
            if (bounce is not null) return bounce.pos;

            if (nextPos == 0) return 1;
            else if (nextPos == 3) return 2;
            else if (currentPos <= nextPos) return nextPos + 1;
            else if (currentPos > nextPos) return nextPos - 1;
            return nextPos;
        }
    }
}