using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbFireworkLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("fireworks", "Fireworks", "0058CE", false, false, new List<GameAction>()
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
                },
                new GameAction("bomb", "Bomb")
                {
                    function = delegate {var e = eventCaller.currentEntity; Fireworks.instance.SpawnBomb(e.beat); },
                    defaultLength = 3f,
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
        [SerializeField] Transform bombSpawn;
        [SerializeField] Rocket firework;
        [SerializeField] FireworksBomb bomb;
        [SerializeField] BezierCurve3D bombCurve;
        [SerializeField] ParticleSystem particleEffect;

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
            spawnedRocket.particleEffect = particleEffect;
            spawnedRocket.Init(beat);
        }

        public void SpawnBomb(float beat)
        {
            FireworksBomb spawnedBomb = Instantiate(bomb, bombSpawn, false);
            spawnedBomb.curve = bombCurve;
            spawnedBomb.Init(beat);
        }
    }
}
