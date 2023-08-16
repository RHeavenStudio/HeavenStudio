using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlDoubleDateLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("doubleDate", "Double Date", "ef854a", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; DoubleDate.instance.Bop(e.beat, e.length, e["bop"], e["autoBop"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Should the two couples bop?"),
                        new Param("autoBop", false, "Bop (Auto)", "Should the two couples auto bop?")
                    }
                },
                new GameAction("soccer", "Soccer Ball")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; DoubleDate.QueueSoccerBall(e.beat); },
                    preFunctionLength = 1f,
                    defaultLength = 2f,
                },
                new GameAction("basket", "Basket Ball")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; DoubleDate.QueueBasketBall(e.beat); },
                    preFunctionLength = 1f,
                    defaultLength = 2f,
                },
                new GameAction("football", "Football")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; DoubleDate.QueueFootBall(e.beat); },
                    preFunctionLength = 1f,
                    defaultLength = 2.5f,
                },
            },
            new List<string>() {"rvl", "normal"},
            "rvldate", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_DoubleDate;

    public class DoubleDate : Minigame
    {
        [Header("Prefabs")]
        [SerializeField] GameObject soccer;
        [SerializeField] GameObject basket;
        [SerializeField] GameObject football;
        [SerializeField] GameObject dropShadow;
        [SerializeField] ParticleSystem leaves;

        [Header("Components")]
        [SerializeField] Animator boyAnim;
        [SerializeField] Animator girlAnim;
        [SerializeField] DoubleDateWeasels weasels;
        [SerializeField] Animator treeAnim;
        [SerializeField] GameObject clouds;

        [Header("Variables")]
        [SerializeField] public float cloudSpeed;
        [SerializeField] public float cloudDistance;
        [SerializeField] public float floorHeight;
        [SerializeField] public float shadowDepthScaleMin;
        [SerializeField] public float shadowDepthScaleMax;
        [SerializeField] SuperCurveObject.Path[] ballBouncePaths;
        double lastGirlGacha = double.MinValue;
        bool shouldBop = true;
        bool canBop = true;
        GameEvent bop = new GameEvent();
        public static DoubleDate instance;
        public static List<QueuedBall> queuedBalls = new List<QueuedBall>();
        [NonSerialized] public double lastHitWeasel = double.MinValue;

        public enum BallType
        {
            Soccer,
            Basket,
            Football
        }

        public struct QueuedBall
        {
            public double beat;
            public BallType type;
        }

        // Editor gizmo to draw trajectories
        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in ballBouncePaths)
            {
                if (path.preview)
                {
                    soccer.GetComponent<SoccerBall>().DrawEditorGizmo(path);
                }
            }
        }

        public override void OnPlay(double beat)
        {
            queuedBalls.Clear();
        }

        private void OnDestroy() {
            queuedBalls.Clear();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start() {
            clouds.transform.position = Vector3.left * ((Time.realtimeSinceStartup * cloudSpeed) % cloudDistance);
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedBalls.Count != 0)
                {
                    foreach (QueuedBall ball in queuedBalls)
                    {
                        switch (ball.type)
                        {
                            case BallType.Soccer:
                                SpawnSoccerBall(ball.beat);
                                break;
                            case BallType.Basket:
                                SpawnBasketBall(ball.beat);
                                break;
                            case BallType.Football:
                                SpawnFootBall(ball.beat);
                                break;
                        }
                    }
                    queuedBalls.Clear();
                }
                if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1) && shouldBop)
                {
                    SingleBop();
                }
            }
            else
            {
                if ((!cond.isPaused) && queuedBalls.Count != 0)
                {
                    queuedBalls.Clear();
                }
            }
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                SoundByte.PlayOneShotGame("doubleDate/kick_whiff");
                Kick(true, true, false);
            }
            clouds.transform.position = Vector3.left * ((Time.realtimeSinceStartup * cloudSpeed) % cloudDistance);
        }

        public void ToggleBop(bool go)
        {
            canBop = go;
        }

        public void Bop(double beat, float length, bool goBop, bool autoBop)
        {
            shouldBop = autoBop;
            if (goBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate { SingleBop(); })
                    });
                }
            }
        }

        void SingleBop()
        {
            if (canBop)
            {
                boyAnim.DoScaledAnimationAsync("IdleBop", 1f);
            }
            if (Conductor.instance.songPositionInBeatsAsDouble > lastGirlGacha)
                girlAnim.DoScaledAnimationAsync("GirlBop", 1f);
            weasels.Bop();
        }

        public void Kick(bool hit = true, bool forceNoLeaves = false, bool weaselsHappy = true, bool jump = false)
        {
            if (hit)
            {
                boyAnim.DoScaledAnimationAsync("Kick", 0.5f);
                if (jump)
                {
                    weasels.Jump();
                    lastGirlGacha = Conductor.instance.songPositionInBeatsAsDouble + 0.5f;
                    girlAnim.DoScaledAnimationAsync("GirlLookUp", 0.5f);
                }
                else if (weaselsHappy) weasels.Happy();
                if (!forceNoLeaves)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + 1f, delegate
                        {
                            leaves.Play();
                            treeAnim.DoScaledAnimationAsync("TreeRustle", 1f);
                        })
                    });
                }
            }
            else
            {
                boyAnim.DoScaledAnimationAsync("Barely", 0.5f);
                weasels.Surprise();
            }
        }

        public static void QueueSoccerBall(double beat)
        {
            if (GameManager.instance.currentGame != "doubleDate")
            {
                queuedBalls.Add(new QueuedBall()
                {
                    beat = beat,
                    type = BallType.Soccer
                });
            }
            else
            {
                instance.SpawnSoccerBall(beat);
            }
            SoundByte.PlayOneShotGame("doubleDate/soccerBounce", beat, forcePlay: true);
        }

        public static void QueueBasketBall(double beat)
        {
            if (GameManager.instance.currentGame != "doubleDate")
            {
                queuedBalls.Add(new QueuedBall()
                {
                    beat = beat,
                    type = BallType.Basket
                });
            }
            else
            {
                instance.SpawnBasketBall(beat);
            }
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("doubleDate/basketballBounce", beat),
                new MultiSound.Sound("doubleDate/basketballBounce", beat + 0.75f),
            }, forcePlay: true);
        }

        public static void QueueFootBall(double beat)
        {
            if (GameManager.instance.currentGame != "doubleDate")
            {
                queuedBalls.Add(new QueuedBall()
                {
                    beat = beat,
                    type = BallType.Football
                });
            }
            else
            {
                instance.SpawnFootBall(beat);
            }
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("doubleDate/footballBounce", beat),
                new MultiSound.Sound("doubleDate/footballBounce", beat + 0.75f),
            }, forcePlay: true);
        }

        public void SpawnSoccerBall(double beat)
        {
            SoccerBall spawnedBall = Instantiate(soccer, instance.transform).GetComponent<SoccerBall>();
            spawnedBall.Init(beat);
        }

        public void SpawnBasketBall(double beat)
        {
            Basketball spawnedBall = Instantiate(basket, instance.transform).GetComponent<Basketball>();
            spawnedBall.Init(beat);
        }

        public void SpawnFootBall(double beat)
        {
            Football spawnedBall = Instantiate(football, instance.transform).GetComponent<Football>();
            spawnedBall.Init(beat);
        }

        public void MissKick(double beat, bool hit = false)
        {
            lastGirlGacha = Conductor.instance.songPositionInBeatsAsDouble + 1.5f;
            girlAnim.DoScaledAnimationAsync("GirlSad", 0.5f);
            if (hit)
            {
                lastHitWeasel = Conductor.instance.songPositionInBeatsAsDouble;
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - (0.25f/3f), delegate { weasels.Hit(beat); }),
                });
            }
            else
            {
                lastHitWeasel = Conductor.instance.songPositionInBeatsAsDouble;
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.25, delegate { weasels.Hide(beat + 0.25f); }),
                });
            }
        }

        public SuperCurveObject.Path GetPath(string name)
        {
            foreach (SuperCurveObject.Path path in ballBouncePaths)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(SuperCurveObject.Path);
        }

        public GameObject MakeDropShadow()
        {
            GameObject shadow = Instantiate(dropShadow, transform);
            return shadow;
        }
    }
}