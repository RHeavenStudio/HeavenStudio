using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

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
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("direction", BuiltToScaleRvl.Direction.Left, "Direction", "Set the direction in which the rod will come out."),
                        new Param("id", new EntityTypes.Integer(1, 4, 0), "Rod ID", "Set the ID of the rod to spawn. Rods with the same ID cannot spawn at the same time."),
                    },
                },
                new GameAction("shoot rod", "Shoot Rod")
                {
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("id", new EntityTypes.Integer(1, 4, 0), "Rod ID", "Set the ID of the rod to shoot."),
                    },
                },
                new GameAction("out sides", "Bounce Out Sides")
                {
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("id", new EntityTypes.Integer(1, 4, 0), "Rod ID", "Set the ID of the rod to out."),
                    },
                },
                new GameAction("custom spawn", "Custom Spawn Rod")
                {
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("direction", BuiltToScaleRvl.Direction.Left, "Direction", "Set the direction in which the rod will come out."),
                        new Param("target", BuiltToScaleRvl.TargetBox.First, "Target", "Set the target in which the rod will bounce."),
                        new Param("id", new EntityTypes.Integer(1, 4, 0), "Rod ID", "Set the ID of the rod to spawn. Rods with the same ID cannot spawn at the same time."),
                    },
                },
                new GameAction("custom bounce", "Custom Bounce")
                {
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("target", BuiltToScaleRvl.Target.First, "Target", "Set the target in which the rod will bounce."),
                        new Param("id", new EntityTypes.Integer(1, 4, 0), "Rod ID", "Set the ID of the rod to bounce."),
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
        [SerializeField] Block[] blocks;
        [SerializeField] GameObject baseRod;
        [SerializeField] GameObject baseLeftSquare;
        [SerializeField] GameObject baseRightSquare;
        [SerializeField] GameObject baseAssembled;
        [SerializeField] Transform widgetHolder;

        public enum Direction {
            Left,
            Right,
        }
        public enum Target {
            OuterLeft = 0,
            First,
            Second,
            Third,
            Fourth,
            OuterRight,
        }
        public enum TargetBox {
            First = 1,
            Second,
            Third,
            Fourth,
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
        List<ScheduledWidget> scheduledWidgets = new List<ScheduledWidget>();
        int widgetIndex;

        public BezierCurve3D[] curve;
        public static readonly Dictionary<(int, int), int> curveMap = new Dictionary<(int, int), int> {
            {(-1, 0), 0}, {(0, -1), 0},     // 01 in
            {(0, 1), 2}, {(1, 0), 2},       // 12
            {(1, 2), 3}, {(2, 1), 3},       // 23
            {(2, 3), 4}, {(3, 2), 4},       // 34
            {(3, 4), 5}, {(4, 3), 5},       // 45 in
            {(0, 0), 7},                    // 11
            {(1, 1), 8},                    // 22
            {(2, 2), 9},                    // 33
            {(3, 3), 10},                   // 44
            {(-1, 1), 11}, {(1, -1), 11},   // 02 in
            {(0, 2), 13}, {(2, 0), 13},     // 13
            {(1, 3), 14}, {(3, 1), 14},     // 24
            {(2, 4), 15}, {(4, 2), 15},     // 35 in
            {(-1, 2), 17}, {(2, -1), 17},   // 03 in
            {(0, 3), 19}, {(3, 0), 19},     // 14
            {(1, 4), 20}, {(4, 1), 20},     // 25
            {(-1, 3), 22}, {(3, -1), 22},   // 04 in
            {(0, 4), 24}, {(4, 0), 24},     // 15 in
        };
        public static readonly Dictionary<(int, int), int> curveMapOut = new Dictionary<(int, int), int> {
            {(-1, 0), 1}, {(0, -1), 1},     // 01 out
            {(3, 4), 6}, {(4, 3), 6},       // 45 out
            {(-1, 1), 12}, {(1, -1), 12},   // 02 out
            {(2, 4), 16}, {(4, 2), 16},     // 35 out
            {(-1, 2), 18}, {(2, -1), 18},   // 03 out
            {(-1, 3), 23}, {(3, -1), 23},   // 04 out
            {(0, 4), 25}, {(4, 0), 25},     // 15 out
        };

        const double WIDGET_SEEK_TIME = 10.0;

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

        public class CustomBounceItem
        {
            public int time = -1;
            public int pos;
        }

        public override void OnGameSwitch(double beat)
        {
            gameStartBeat = beat;
            var firstEnd = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).Find(x => x.beat > gameStartBeat);
            gameEndBeat = firstEnd?.beat ?? gameEndBeat;

            scheduledWidgets.Clear();
            widgetIndex = 0;
            var events = EventCaller.GetAllInGameManagerList("builtToScaleRvl", new string[] { "spawn rod", "custom spawn" }).FindAll(x => x.beat + x.length >= gameStartBeat && x.beat < gameEndBeat);
            foreach (var evt in events)
            {
                if (evt.length == 0) continue;
                int currentPos, nextPos;
                currentPos = evt["direction"] switch {
                    (int)Direction.Left => -1,
                    (int)Direction.Right => 4,
                    _ => throw new System.NotImplementedException()
                };
                switch (evt.datamodel){
                    case "builtToScaleRvl/spawn rod":
                        nextPos = evt["direction"] switch {
                            (int)Direction.Left => 0,
                            (int)Direction.Right => 3,
                            _ => throw new System.NotImplementedException()
                        };
                        break;
                    case "builtToScaleRvl/custom spawn":
                        nextPos = evt["target"] switch {
                            (int)TargetBox.First => 0,
                            (int)TargetBox.Second => 1,
                            (int)TargetBox.Third => 2,
                            (int)TargetBox.Fourth => 3,
                            _ => throw new System.NotImplementedException()
                        };
                        break;
                    default:
                        throw new System.NotImplementedException();
                        break;
                }

                List<CustomBounceItem> bounceItems = CalcRodBounce(evt.beat, evt.length, evt["id"]);
                AddBounceOutSides(evt.beat, evt.length, currentPos, nextPos, evt["id"], ref bounceItems);
                bool isShoot;
                int rodEndTime = CalcRodEndTime(evt.beat, evt.length, currentPos, nextPos, evt["id"], ref bounceItems, out isShoot);
                var widget = new ScheduledWidget
                {
                    beat = evt.beat, 
                    length = evt.length,
                    currentPos = currentPos,
                    nextPos = nextPos,
                    id =  evt["id"],
                    bounceItems = bounceItems.ToArray(),
                    endTime = rodEndTime,
                    isShoot = isShoot,
                };
                scheduledWidgets.Add(widget);
            }
            scheduledWidgets.Sort((x, y) => x.beat.CompareTo(y.beat));
        }
        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        public bool isPlayerOpen { get { return blocks[2].isOpen; } }
        public bool isPlayerPrepare { get { return blocks[2].isPrepare; } }

        void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying || cond.isPaused) return;

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                if (!isPlayerPrepare)
                {
                    PlayBlockOpen(2);
                }
            }
            if (PlayerInput.GetIsAction(InputAction_FlickAltPress) && !IsExpectingInputNow(InputAction_FlickAltPress))
            {
                if (isPlayerPrepare)
                {
                    PlayBlockShootMiss(2);
                }
            }
            UpdateWidgets();
        }

        void UpdateWidgets()
        {
            double beat = conductor.songPositionInBeatsAsDouble;
            while(widgetIndex < scheduledWidgets.Count)
            {
                var widget = scheduledWidgets[widgetIndex];
                if (widget.beat < beat + WIDGET_SEEK_TIME)
                {
                    SpawnRod(widget.beat, widget.length, widget.currentPos, widget.nextPos, widget.id, widget.bounceItems, widget.endTime, widget.isShoot);
                    widgetIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        public void SpawnRod(double beat, double length, int currentPos, int nextPos, int id, CustomBounceItem[] bounceItems, int endTime, bool isShoot)
        {            
            var newRod = Instantiate(baseRod, widgetHolder).GetComponent<Rod>();

            newRod.startBeat = beat;
            newRod.lengthBeat = length;
            newRod.currentPos = currentPos;
            newRod.nextPos = nextPos;
            newRod.ID = id;
            newRod.customBounce = bounceItems;
            newRod.endTime = endTime;
            newRod.isShoot = isShoot;
            if (isShoot)
            {
                double endBeat = beat + length * endTime;
                SoundByte.PlayOneShotGame("builtToScaleRvl/prepare", endBeat - 2*length);
                newRod.Squares = SpawnSquare(endBeat, id);
            }
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    newRod.Init();
                    newRod.gameObject.SetActive(true);
                })
            });

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
            var firstOut = EventCaller.GetAllInGameManagerList("builtToScaleRvl", new string[] { "out sides" }).Find(x => x.beat >= beat + length && x["id"] == id);
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
            var firstShoot = EventCaller.GetAllInGameManagerList("builtToScaleRvl", new string[] { "shoot rod" }).Find(x => x.beat >= beat + length && x["id"] == id);
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

        public void PlayBlockBounce(int position, double beat)
        {
            if (!IsPositionInRange(position)) return;
            blocks[position].Bounce(beat);
        }
        public void PlayBlockBounceNearlyMiss(int position)
        {
            if (!IsPositionInRange(position)) return;
            blocks[position].BounceNearlyMiss();
        }
        public void PlayBlockBounceMiss(int position)
        {
            if (!IsPositionInRange(position)) return;
            blocks[position].BounceMiss();
        }

        public void PlayBlockPrepare(int position)
        {
            if (!IsPositionInRange(position)) return;
            blocks[position].Prepare();
        }
        public void PlayBlockShoot(int position)
        {
            if (!IsPositionInRange(position)) return;
            blocks[position].Shoot();
        }
        public void PlayBlockShootNearlyMiss(int position)
        {
            if (!IsPositionInRange(position)) return;
            blocks[position].ShootNearlyMiss();
        }
        public void PlayBlockShootMiss(int position)
        {
            if (!IsPositionInRange(position)) return;
            blocks[position].ShootMiss();
        }
        
        public void PlayBlockOpen(int position)
        {
            if (!IsPositionInRange(position)) return;
            blocks[position].Open();
        }
        public void PlayBlockIdle(int position, double beat = double.MinValue)
        {
            if (!IsPositionInRange(position)) return;
            blocks[position].Idle(beat);
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
        public static bool IsPositionInRange(int position)
        {
            return position >= 0 && position <= 3;
        }
    }
}