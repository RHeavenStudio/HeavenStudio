using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using DG.Tweening;

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
                    preFunction = delegate {var e = eventCaller.currentEntity; Fireworks.PreSpawnFirework(e.beat, false, e["whereToSpawn"], e["toggle"], e["explosionType"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("whereToSpawn", Fireworks.WhereToSpawn.Middle, "Where to spawn?", "Where should the firework spawn?"),
                        new Param("explosionType", Fireworks.ExplosionType.MixedCircular, "Explosion Pattern", "What pattern should the firework explode with?"),
                        new Param("toggle", false, "Practice Count-In", "Should the count-in from the fireworks practice play?")
                    }
                },
                new GameAction("sparkler", "Sparkler")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; Fireworks.PreSpawnFirework(e.beat, true, e["whereToSpawn"], e["toggle"], e["explosionType"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("whereToSpawn", Fireworks.WhereToSpawn.Middle, "Where to spawn?", "Where should the firework spawn?"),
                        new Param("explosionType", Fireworks.ExplosionType.MixedCircular, "Explosion Pattern", "What pattern should the firework explode with?"),
                        new Param("toggle", false, "Practice Count-In", "Should the count-in from the fireworks practice play?")
                    }
                },
                new GameAction("bomb", "Bomb")
                {
                    function = delegate {var e = eventCaller.currentEntity; Fireworks.instance.SpawnBomb(e.beat, e["toggle"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Practice Count-In", "Should the count-in from the fireworks practice play?")
                    }
                },
                new GameAction("countIn", "Count-In")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; Fireworks.CountIn(e.beat, e["count"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("count", Fireworks.CountInType.CountOne, "Count", "Which count should be said?")
                    }
                },
                new GameAction("altBG", "Background Appearance")
                {
                    function = delegate {var e = eventCaller.currentEntity; Fireworks.instance.ChangeBackgroundAppearance(e["toggle"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Remix 5", "Should the background from Remix 5 tengoku appear?")
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
        public enum ExplosionType
        {
            UniformBig = 0,
            UniformDonut = 1,
            UniformSwirl = 2,
            UniformSmile = 3,
            MixedCircular = 4
        }
        public struct QueuedFirework
        {
            public float beat;
            public bool isSparkler;
            public int whereToSpawn;
            public bool practice;
            public int explosionType;
        }
        public enum WhereToSpawn
        {
            Left = 0,
            Right = 1,
            Middle = 2
        }
        public enum CountInType
        {
            CountOne = 0,
            CountTwo = 1,
            CountThree = 2,
            CountHey = 3
        }
        [Header("Components")]
        [SerializeField] Transform spawnLeft;
        [SerializeField] Transform spawnRight;
        [SerializeField] Transform spawnMiddle;
        [SerializeField] Transform bombSpawn;
        [SerializeField] Rocket firework;
        [SerializeField] FireworksBomb bomb;
        [SerializeField] BezierCurve3D bombCurve;
        [SerializeField] SpriteRenderer flashWhite;
        [SerializeField] GameObject faces;
        [SerializeField] GameObject stars;
        [Header("Properties")]
        Tween flashTween;
        public static List<QueuedFirework> queuedFireworks = new List<QueuedFirework>();

        public static Fireworks instance;

        void OnDestroy()
        {
            if (queuedFireworks.Count > 0) queuedFireworks.Clear();
        }

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedFireworks.Count > 0)
                {
                    foreach (var firework in queuedFireworks)
                    {
                        SpawnFirework(firework.beat, firework.isSparkler, firework.whereToSpawn, firework.practice, firework.explosionType);
                    }
                    queuedFireworks.Clear();
                }
            }
        }

        public void ChangeBackgroundAppearance(bool doIt)
        {
            faces.SetActive(doIt);
            stars.SetActive(!doIt);
        }

        public static void CountIn(float beat, int count)
        {
            switch (count)
            {
                case (int)CountInType.CountOne:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("fireworks/count1", beat)
                    }, forcePlay: true);
                    break;
                case (int)CountInType.CountTwo:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("fireworks/count2", beat)
                    }, forcePlay: true);
                    break;
                case (int)CountInType.CountThree:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("fireworks/count3", beat)
                    }, forcePlay: true);
                    break;
                case (int)CountInType.CountHey:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("fireworks/countHey", beat)
                    }, forcePlay: true);
                    break;
            }
        }

        public static void PreSpawnFirework(float beat, bool isSparkler, int whereToSpawn, bool practice, int explosionType)
        {
            if (isSparkler)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("fireworks/sparkler", beat, 1, 1, false, 0.223f)
                }, forcePlay: true);
            }
            else
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("fireworks/rocket", beat)
                }, forcePlay: true);
            }
            if (GameManager.instance.currentGame == "fireworks")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        Fireworks.instance.SpawnFirework(beat, isSparkler, whereToSpawn, practice, explosionType);
                    })
                });
            }
            else
            {
                queuedFireworks.Add(new QueuedFirework { beat = beat, isSparkler = isSparkler, whereToSpawn = whereToSpawn, practice = practice, explosionType = explosionType });
            }

        }

        void SpawnFirework(float beat, bool isSparkler, int whereToSpawn, bool practice, int explosionType)
        {
            if (isSparkler && practice)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("fireworks/practiceHai", beat + 1),
                }, forcePlay: true);
            }
            else if (practice)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("fireworks/practice1", beat),
                    new MultiSound.Sound("fireworks/practice2", beat + 1),
                    new MultiSound.Sound("fireworks/practice3", beat + 2),
                    new MultiSound.Sound("fireworks/practiceHai", beat + 3),
                }, forcePlay: true);
            }

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
            spawnedRocket.Init(beat, explosionType);
        }

        public void SpawnBomb(float beat, bool practice)
        {
            Jukebox.PlayOneShotGame("fireworks/bomb");
            if (practice)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("fireworks/practiceHai", beat + 2),
                }, forcePlay: true);
            }
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1, delegate
                {
                    FireworksBomb spawnedBomb = Instantiate(bomb, bombSpawn, false);
                    spawnedBomb.curve = bombCurve;
                    spawnedBomb.Init(beat + 1);
                })
            });
        }

        public void ChangeFlashColor(Color color, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (flashTween != null)
                flashTween.Kill(true);

            if (seconds == 0)
            {
                flashWhite.color = color;
            }
            else
            {
                flashTween = flashWhite.DOColor(color, seconds);
            }
        }

        public void FadeFlashColor(Color start, Color end, float beats)
        {
            ChangeFlashColor(start, 0f);
            ChangeFlashColor(end, beats);
        }
    }
}
