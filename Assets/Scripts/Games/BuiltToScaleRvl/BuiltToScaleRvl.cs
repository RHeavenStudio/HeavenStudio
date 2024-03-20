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
                        new Param("direction", BuiltToScaleRvl.Direction.Left, "Direction", "Set the direction in which the stick will come out."),
                        new Param("id", new EntityTypes.Integer(0, 4, 0), "Rod ID", "Set the ID of the rod to spawn. Rods with the same ID cannot spawn at the same time."),
                    },
                },
                new GameAction("shoot rod", "Shoot Rod")
                {
                    // function = delegate { var e = eventCaller.currentEntity; BuiltToScaleRvl.instance.ShootRod(e.beat, e["id"]); },
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

        public enum Direction {
            Left,
            Right,
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
        
        // 1.05(3,1)
        private void Awake()
        {
            instance = this;
        }

        private double gameStartBeat = double.MinValue;
        private double gameEndBeat = double.MaxValue;
        public static List<QueuedRod> queuedRods = new List<QueuedRod>();
        public List<Rod> spawnedRods = new List<Rod>();

        public struct QueuedRod
        {
            public double beat;
            public double length;
            public int currentPos;
            public int nextPos;
            public int id;
        }

        public override void OnPlay(double beat)
        {
            queuedRods.Clear();
            gameStartBeat = beat;
            var firstEnd = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).Find(x => x.beat > gameStartBeat);
            gameEndBeat = firstEnd?.beat ?? gameEndBeat;
        }
        public override void OnGameSwitch(double beat)
        {
            gameStartBeat = beat;
            var firstEnd = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).Find(x => x.beat > gameStartBeat);
            gameEndBeat = firstEnd?.beat ?? gameEndBeat;
        }
        private void OnDestroy() {
            queuedRods.Clear();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
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

            bool isShoot;
            double rodEndBeat = CalcRodEndBeat(beat, length, currentPos, nextPos, id, out isShoot);
            newRod.endBeat = rodEndBeat;
            newRod.isShoot = isShoot;
            if (rodEndBeat != double.MaxValue)
            {
                newRod.Squares = SpawnSquare(beat, rodEndBeat, id);
            }

            newRod.Init();
            newRod.gameObject.SetActive(true);
        }

        private Square[] SpawnSquare(double startBeat, double endBeat, int id)
        {
            var newLeftSquare = Instantiate(baseLeftSquare, widgetHolder).GetComponent<Square>();
            var newRightSquare = Instantiate(baseRightSquare, widgetHolder).GetComponent<Square>();
            newLeftSquare.startBeat = startBeat;
            newRightSquare.startBeat = startBeat;
            newLeftSquare.endBeat = endBeat;
            newRightSquare.endBeat = endBeat;
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

        private double CalcRodEndBeat(double beat, double length, int currentPos, int nextPos, int id, out bool isShoot)
        {
            isShoot = false;
            var firstShoot = EventCaller.GetAllInGameManagerList("builtToScaleRvl", new string[] { "shoot rod" }).Find(x => x.beat > beat && x["id"] == id);
            if (firstShoot is null)
                return double.MaxValue;
            double shootEventBeat = firstShoot.beat;

            if (EventCaller.GetAllInGameManagerList("builtToScaleRvl", new string[] { "out sides" }).Any(x => x.beat > beat && x.beat < shootEventBeat && x["id"] == id))
                return double.MaxValue;
            
            var n = (int)Math.Ceiling((shootEventBeat-beat)/length);
            int current = currentPos, next = nextPos;
            int shootTiming;
            for (int i = 0; ; i++) {
                if (current == 2 && i >= n) {
                    shootTiming = i;
                    break;
                }
                int following = getFollowingPos(current, next);
                current = next;
                next = following;
            }
            isShoot = true;
            return beat + length * shootTiming;
        }

        int getFollowingPos(int currentPos, int nextPos)
        {
            if (nextPos == 0) return 1;
            else if (nextPos == 3) return 2;
            else if (currentPos < nextPos) return nextPos + 1;
            else if (currentPos > nextPos) return nextPos - 1;
            return nextPos;
        }

        public void ShootRod(double beat, int id)
        {
            // var newLeftSquare = Instantiate(baseLeftSquare, widgetHolder).GetComponent<Square>();
            // var newRightSquare = Instantiate(baseRightSquare, widgetHolder).GetComponent<Square>();
            // newLeftSquare.beat = beat;
            // newRightSquare.beat = beat;
            // newLeftSquare.Init();
            // newRightSquare.Init();
            // newLeftSquare.gameObject.SetActive(true);
            // newRightSquare.gameObject.SetActive(true);
            var rod = spawnedRods.Find(x => x.ID == id);
            if (rod is not null)
            {
                rod.PreShoot(beat);
            }
        }

        public void OutRod(double beat, int id)
        {
            var rod = spawnedRods.Find(x => x.ID == id);
            if (rod is not null)
            {
                rod.PreOut(beat);
            }
        }
    }
}