using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbFireworkLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("fireworks", "Fireworks \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("firework", "Firework")
                {
                    function = delegate {var e = eventCaller.currentEntity; Fireworks.instance.SpawnFirework(e.beat, false, e["whereToSpawn"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("whereToSpawn", Fireworks.WhereToSpawn.Middle, "Where to spawn?", "Where should the firework spawn?")
                    }
                },
                new GameAction("sparkler", "Sparkler")
                {
                    function = delegate {var e = eventCaller.currentEntity; Fireworks.instance.SpawnFirework(e.beat, true, e["whereToSpawn"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("whereToSpawn", Fireworks.WhereToSpawn.Middle, "Where to spawn?", "Where should the firework spawn?")
                    }
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_Fireworks;
    public class Fireworks : Minigame
    {
        public enum WhereToSpawn
        {
            Left = 0,
            Right = 1,
            Middle = 2
        }
        [Header("Components")]
        [SerializeField] Transform spawnLeft;
        [SerializeField] Transform spawnRight;
        [SerializeField] Transform spawnMiddle;
        [SerializeField] Rocket firework;

        public static Fireworks instance;

        void Awake()
        {
            instance = this;
        }

        public void SpawnFirework(float beat, bool isSparkler, int whereToSpawn)
        {
            Transform spawnPoint = spawnMiddle;
            switch (whereToSpawn)
            {
                case (int)WhereToSpawn.Left:
                    spawnPoint = spawnLeft;
                    break;
                case (int)WhereToSpawn.Right:
                    spawnPoint = spawnRight;
                    break;
                default:
                    spawnPoint = spawnMiddle;
                    break;
            }
            Rocket spawnedRocket = Instantiate(firework, spawnPoint, false);
            spawnedRocket.isSparkler = isSparkler;
            spawnedRocket.Init(beat);
        }
    }
}
