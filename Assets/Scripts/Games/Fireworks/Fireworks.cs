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
            return new Minigame("fireworks", "Fireworks \n<color=#adadad>(Hanabi)</color>", "000820", false, false, new List<GameAction>()
            {
                new GameAction("firework", "Firework")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; Fireworks.PreSpawnFirework(e.beat, false, e["whereToSpawn"], e["toggle"], e["explosionType"], e["applause"], e["offSet"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("whereToSpawn", Fireworks.WhereToSpawn.Middle, "Where to spawn?", "Where should the firework spawn?"),
                        new Param("explosionType", Fireworks.ExplosionType.MixedCircular, "Explosion Pattern", "What pattern should the firework explode with?"),
                        new Param("applause", false, "Applause", "Should an applause play after successfully hitting this cue?"),
                        new Param("offSet", new EntityTypes.Float(0, 4, 0), "Vertical Offset", "What vertical Offset should the rocket have?"),
                        new Param("toggle", false, "Practice Count-In", "Should the count-in from the fireworks practice play?")
                    }
                },
                new GameAction("sparkler", "Sparkler")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; Fireworks.PreSpawnFirework(e.beat, true, e["whereToSpawn"], e["toggle"], e["explosionType"], e["applause"], e["offSet"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("whereToSpawn", Fireworks.WhereToSpawn.Middle, "Where to spawn?", "Where should the firework spawn?"),
                        new Param("explosionType", Fireworks.ExplosionType.MixedCircular, "Explosion Pattern", "What pattern should the firework explode with?"),
                        new Param("applause", false, "Applause", "Should an applause play after successfully hitting this cue?"),
                        new Param("offSet", new EntityTypes.Float(0, 4, 0), "Vertical Offset", "What vertical Offset should the rocket have?"),
                        new Param("toggle", false, "Practice Count-In", "Should the count-in from the fireworks practice play?")
                    }
                },
                new GameAction("bomb", "Bomb")
                {
                    function = delegate {var e = eventCaller.currentEntity; Fireworks.instance.SpawnBomb(e.beat, e["toggle"], e["applause"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("applause", false, "Applause", "Should an applause play after successfully hitting this cue?"),
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
            },
            new List<string>() {"agb", "normal"},
            "agbexplode", "en",
            new List<string>() {}
            );
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
            public double beat;
            public bool isSparkler;
            public int whereToSpawn;
            public bool practice;
            public int explosionType;
            public bool applause;
            public float verticalOffset;
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
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
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
                        SpawnFirework(firework.beat, firework.isSparkler, firework.whereToSpawn, firework.practice, firework.explosionType, firework.applause, firework.verticalOffset);
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

        public static void CountIn(double beat, int count)
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

        public static void PreSpawnFirework(double beat, bool isSparkler, int whereToSpawn, bool practice, int explosionType, bool applause, float verticalOffset)
        {
            if (isSparkler)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("fireworks/nuei", beat, 1, 1, false, 0.223f)
                }, forcePlay: true);
            }
            else
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("fireworks/rocket_2", beat)
                }, forcePlay: true);
            }
            if (GameManager.instance.currentGame == "fireworks")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        Fireworks.instance.SpawnFirework(beat, isSparkler, whereToSpawn, practice, explosionType, applause, verticalOffset);
                    })
                });
            }
            else
            {
                queuedFireworks.Add(new QueuedFirework { beat = beat, isSparkler = isSparkler, whereToSpawn = whereToSpawn, practice = practice, explosionType = explosionType, applause = applause, verticalOffset = verticalOffset });
            }

        }

        void SpawnFirework(double beat, bool isSparkler, int whereToSpawn, bool practice, int explosionType, bool applause, float verticalOffset)
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
            spawnedRocket.offSet = verticalOffset;
            spawnedRocket.applause = applause;
            spawnedRocket.Init(beat, explosionType);
        }

        public void SpawnBomb(double beat, bool practice, bool applause)
        {
            SoundByte.PlayOneShotGame("fireworks/tamaya_4");
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
                    spawnedBomb.applause = applause;
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
