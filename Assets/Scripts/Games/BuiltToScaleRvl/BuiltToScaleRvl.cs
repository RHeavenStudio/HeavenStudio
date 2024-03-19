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
                    function = delegate { var e = eventCaller.currentEntity; BuiltToScaleRvl.PreSpawnRod(e.beat, e.length, e["direction"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("direction", BuiltToScaleRvl.Direction.Left, "Direction", "Set the direction in which the stick will come out.")
                    },
                },
                new GameAction("shoot rod", "Shoot Rod")
                {
                    function = delegate { var e = eventCaller.currentEntity; BuiltToScaleRvl.instance.ShootRod(e.beat); },
                    defaultLength = 1f,
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

        public struct QueuedRod
        {
            public double beat;
            public double length;
            public int direction;
            // public int position;
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
                SpawnRod(queuedRod.beat, queuedRod.length, queuedRod.direction);
            }
        }

        public static void PreSpawnRod(double beat, double length, int direction)
        {
            if (GameManager.instance.currentGame == "builtToScaleRvl")
            {
                instance.SpawnRod(beat, length, direction);
            }
            else
            {
                queuedRod = new QueuedRod()
                {
                    beat = beat,
                    length = length,
                    direction= direction,
                };
            }
        }
        public void SpawnRod(double beat, double length, int direction)
        {
            var newRod = Instantiate(baseRod, widgetHolder).GetComponent<Rod>();
            newRod.startBeat = beat;
            newRod.lengthBeat = length;
            
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

            newRod.currentPos = currentPos;
            newRod.nextPos = nextPos;
            newRod.Init();
            newRod.gameObject.SetActive(true);
        }

        public void ShootRod(double beat)
        {
        }
    }
}