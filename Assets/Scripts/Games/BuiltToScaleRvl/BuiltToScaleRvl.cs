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
                    function = delegate { var e = eventCaller.currentEntity; BuiltToScaleRvl.PreSpawnRod(e.beat, e.length, e["direction"], e["id"]); },
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
                    function = delegate { var e = eventCaller.currentEntity; BuiltToScaleRvl.instance.ShootRod(e.beat, e["id"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("id", new EntityTypes.Integer(0, 4, 0), "Rod ID", "Set the ID of the rod to shoot."),
                    },
                },
                new GameAction("off sides", "Bounce Off Sides")
                {
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

        private double endBeat = double.MaxValue;
        private static QueuedRod queuedRod;
        public List<Rod> spawnedRods = new List<Rod>();

        public struct QueuedRod
        {
            public double beat;
            public double length;
            public int direction;
            public int id;
        }

        public override void OnPlay(double beat)
        {
            var firstEnd = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).Find(x => x.beat > beat);
            endBeat = firstEnd?.beat ?? endBeat;
        }
        public override void OnGameSwitch(double beat)
        {
            OnPlay(beat);

            if (queuedRod.beat >= beat && queuedRod.beat < endBeat)
            {
                SpawnRod(queuedRod.beat, queuedRod.length, queuedRod.direction, queuedRod.id);
            }
        }

        public static void PreSpawnRod(double beat, double length, int direction, int id)
        {
            if (GameManager.instance.currentGame == "builtToScaleRvl")
            {
                instance.SpawnRod(beat, length, direction, id);
            }
            else
            {
                queuedRod = new QueuedRod()
                {
                    beat = beat,
                    length = length,
                    direction = direction,
                    id = id,
                };
            }
        }
        public void SpawnRod(double beat, double length, int direction, int id)
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
            GenerateRod(beat, length, currentPos, nextPos, id);
        }
        public void GenerateRod(double beat, double length, int currentPos, int nextPos, int id)
        {
            var newRod = Instantiate(baseRod, widgetHolder).GetComponent<Rod>();
            spawnedRods.Add(newRod);
            newRod.startBeat = beat;
            newRod.lengthBeat = length;
            newRod.ID = id;

            newRod.currentPos = currentPos;
            newRod.nextPos = nextPos;
            newRod.Init();
            newRod.gameObject.SetActive(true);
        }

        public void ShootRod(double beat, int id)
        {
            var rod = spawnedRods.Find(x => x.ID == id);
            if (rod is not null)
            {
                rod.PreShoot(beat);
            }
        }
    }
}