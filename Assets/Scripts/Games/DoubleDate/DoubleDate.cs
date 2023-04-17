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
                    defaultLength = 2f,
                },
                new GameAction("basket", "Basket Ball")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; DoubleDate.QueueBasketBall(e.beat); },
                    defaultLength = 2f,
                },
                new GameAction("football", "Football")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; DoubleDate.QueueFootBall(e.beat); },
                    defaultLength = 2.5f,
                },
            });
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
        [SerializeField] ParticleSystem leaves;

        [Header("Components")]
        [SerializeField] Animator boyAnim;
        [SerializeField] Animator girlAnim;
        [SerializeField] DoubleDateWeasels weasels;
        [SerializeField] Animator treeAnim;
        [SerializeField] FollowPath.Path[] ballBouncePaths;

        [Header("Variables")]
        bool shouldBop = true;
        bool canBop = true;
        GameEvent bop = new GameEvent();
        public static DoubleDate instance;
        public static List<QueuedBall> queuedBalls = new List<QueuedBall>();

        public enum BallType
        {
            Soccer,
            Basket,
            Football
        }

        public struct QueuedBall
        {
            public float beat;
            public BallType type;
        }

        // Editor gizmo to draw trajectories
        const float TRAJECTORY_STEP = 0.1f;
        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (FollowPath.Path path in ballBouncePaths)
            {
                if (path.preview)
                {
                    if (path.positions.Length > 1)
                    {
                        for (int i = 0; i < path.positions.Length - 1; i++)
                        {
                            FollowPath.PathPos pos = path.positions[i];
                            FollowPath.PathPos nextPos = path.positions[i + 1];
                            if (pos.pos == null || nextPos.pos == null)
                                return;
                            Vector3 startPos = pos.pos.position;
                            Vector3 endPos = nextPos.pos.position;
                            // draw a curve between the two points using the path height
                            List<Vector3> points = new List<Vector3>();
                            for (float t = 0; t < 1; t += TRAJECTORY_STEP)
                            {
                                float yMul = t * 2f - 1f;
                                float yWeight = -(yMul*yMul) + 1f;
                                Vector3 p = Vector3.LerpUnclamped(startPos, endPos, t);
                                p.y += yWeight * pos.height;
                                points.Add(p);
                            }
                            points.Add(endPos);
                            for (int j = 0; j < points.Count - 1; j++)
                            {
                                Gizmos.color = Color.blue;
                                Gizmos.DrawLine(points[j], points[j + 1]);
                            }
                            Gizmos.DrawSphere(startPos, 0.1f);
                            Gizmos.DrawSphere(endPos, 0.1f);
                        }
                    }
                }
            }
        }

        public override void OnPlay(float beat)
        {
            queuedBalls.Clear();
        }

        private void Awake()
        {
            instance = this;
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
                Jukebox.PlayOneShotGame("doubleDate/kick_whiff");
                Kick(true, true, false);
            }
        }

        public void ToggleBop(bool go)
        {
            canBop = go;
        }

        public void Bop(float beat, float length, bool goBop, bool autoBop)
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
            girlAnim.DoScaledAnimationAsync("GirlBop", 1f);
            weasels.Bop();
        }

        public void Kick(bool hit = true, bool forceNoLeaves = false, bool weaselsHappy = true)
        {
            if (hit)
            {
                boyAnim.DoScaledAnimationAsync("Kick", 1f);
                if (weaselsHappy) weasels.Happy();
                if (!forceNoLeaves)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(Conductor.instance.songPositionInBeats + 1f, delegate
                        {
                            leaves.Play();
                            treeAnim.DoScaledAnimationAsync("TreeRustle", 1f);
                        })
                    });
                }
            }
            else
            {
                boyAnim.DoScaledAnimationAsync("Barely", 1f);
            }
        }

        public static void QueueSoccerBall(float beat)
        {
            if (instance == null)
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
            Jukebox.PlayOneShotGame("doubleDate/soccerBounce", beat, forcePlay: true);
        }

        public static void QueueBasketBall(float beat)
        {
            if (instance == null)
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
            });
        }

        public static void QueueFootBall(float beat)
        {
            if (instance == null)
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
            });
        }

        public void SpawnSoccerBall(float beat)
        {
            SoccerBall spawnedBall = Instantiate(soccer, instance.transform).GetComponent<SoccerBall>();
            spawnedBall.Init(beat);
        }

        public void SpawnBasketBall(float beat)
        {
            Basketball spawnedBall = Instantiate(basket, instance.transform).GetComponent<Basketball>();
            spawnedBall.Init(beat);
        }

        public void SpawnFootBall(float beat)
        {
            Football spawnedBall = Instantiate(football, instance.transform).GetComponent<Football>();
            spawnedBall.Init(beat);
        }

        public FollowPath.Path GetPath(string name)
        {
            foreach (FollowPath.Path path in ballBouncePaths)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(FollowPath.Path);
        }
    }
}