using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlWorkingDoughLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("workingDough", "Working Dough \n<color=#eb5454>[WIP]</color>", "090909", false, false, new List<GameAction>()
            {
                new GameAction("beat intervals", "Start Interval")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.PreSetIntervalStart(e.beat, e.length);  },
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("small ball", "Small Ball")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.PreSpawnBall(e.beat, false);  },
                    defaultLength = 0.5f,
                },
                new GameAction("big ball", "Big Ball")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.PreSpawnBall(e.beat, true);  },
                    defaultLength = 0.5f,
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_WorkingDough;
    public class WorkingDough : Minigame
    {
        [Header("Components")]
        [SerializeField] GameObject doughDudesNPC; //Jump animations
        public GameObject doughDudesPlayer; //Jump animations
        [SerializeField] GameObject ballTransporterRightNPC; //Close and open animations
        [SerializeField] GameObject ballTransporterLeftNPC; //Close and open animations
        [SerializeField] GameObject ballTransporterRightPlayer; //Close and open animations
        [SerializeField] GameObject ballTransporterLeftPlayer; //Close and open animations
        [SerializeField] GameObject npcImpact;
        [SerializeField] GameObject playerImpact;
        [SerializeField] GameObject smallBallNPC;
        [SerializeField] GameObject bigBallNPC;
        [SerializeField] Transform ballHolder;
        [SerializeField] SpriteRenderer arrowSRLeftNPC;
        [SerializeField] SpriteRenderer arrowSRRightNPC;
        [SerializeField] SpriteRenderer arrowSRLeftPlayer;
        [SerializeField] SpriteRenderer arrowSRRightPlayer;
        [SerializeField] GameObject NPCBallTransporters;
        [SerializeField] GameObject PlayerBallTransporters;
        [SerializeField] GameObject playerEnterSmallBall;
        [SerializeField] GameObject playerEnterBigBall;
        [SerializeField] GameObject missImpact;

        [Header("Variables")]
        public bool intervalStarted;
        float intervalStartBeat;
        public float beatInterval = 4f;
        public bool bigMode;
        public bool bigModePlayer;
        static List<QueuedBall> queuedBalls = new List<QueuedBall>();
        struct QueuedBall
        {
            public float beat;
            public bool isBig;
        }
        static List<QueuedInterval> queuedIntervals = new List<QueuedInterval>();
        struct QueuedInterval
        {
            public float beat;
            public float interval;
        }
        private List<GameObject> currentBalls = new List<GameObject>();

        [Header("Curves")]
        public BezierCurve3D npcEnterUpCurve;
        public BezierCurve3D npcEnterDownCurve;
        public BezierCurve3D npcExitUpCurve;
        public BezierCurve3D npcExitDownCurve;
        public BezierCurve3D playerEnterUpCurve;
        public BezierCurve3D playerEnterDownCurve;
        public BezierCurve3D playerExitUpCurve;
        public BezierCurve3D playerExitDownCurve;
        public BezierCurve3D playerMissCurveFirst;
        public BezierCurve3D playerMissCurveSecond;

        [Header("Resources")]
        public Sprite whiteArrowSprite;
        public Sprite redArrowSprite;

        public static WorkingDough instance;

        void Awake()
        {
            instance = this;
        }

        public void SetIntervalStart(float beat, float interval)
        {
            if (!intervalStarted)
            {
                intervalStarted = true;
                bigMode = false;
                BeatAction.New(ballTransporterLeftNPC, new List<BeatAction.Action>()
                {
                    //Open player transporters
                    new BeatAction.Action(beat + interval - 1f, delegate {
                         ballTransporterLeftPlayer.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0);
                    }),
                    new BeatAction.Action(beat + interval - 1f, delegate {
                        ballTransporterRightPlayer.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                    }),

                    //End interval
                    new BeatAction.Action(beat + interval, delegate { intervalStarted = false; }),

                    //Close npc transporters
                    new BeatAction.Action(beat + interval, delegate {
                        if (bigMode)
                        {
                            NPCBallTransporters.GetComponent<Animator>().Play("NPCExitBigMode", 0, 0);
                            bigMode = false;
                        }
                    }),
                    new BeatAction.Action(beat + interval + 1, delegate { if (!intervalStarted) ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftClose", 0, 0); }),
                    new BeatAction.Action(beat + interval + 1, delegate { if (!intervalStarted) ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightClose", 0, 0); }),
                    //Close player transporters
                    new BeatAction.Action(beat + interval * 2 + 1, delegate { ballTransporterLeftPlayer.GetComponent<Animator>().Play("BallTransporterLeftClose", 0, 0); }),
                    new BeatAction.Action(beat + interval * 2 + 1, delegate { ballTransporterRightPlayer.GetComponent<Animator>().Play("BallTransporterRightClose", 0, 0); }),
                    new BeatAction.Action(beat + interval * 2 + 1, delegate {
                        if (bigModePlayer)
                        {
                            PlayerBallTransporters.GetComponent<Animator>().Play("PlayerExitBigMode", 0, 0);
                            bigModePlayer = false;
                        }
                    }),
                });
            }

            intervalStartBeat = beat;
            beatInterval = interval;
        }

        public void SpawnBall(float beat, bool isBig)
        {
            if (!intervalStarted)
            {
                SetIntervalStart(beat, beatInterval);
            }
            var objectToSpawn = isBig ? bigBallNPC : smallBallNPC;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<NPCDoughBall>();
            ballComponent.startBeat = beat;
            ballComponent.exitUpCurve = npcExitUpCurve;
            ballComponent.enterUpCurve = npcEnterUpCurve;
            ballComponent.exitDownCurve = npcExitDownCurve;
            ballComponent.enterDownCurve = npcEnterDownCurve;

            spawnedBall.SetActive(true);

            if (isBig && !bigMode)
            {
                NPCBallTransporters.GetComponent<Animator>().Play("NPCGoBigMode", 0, 0);
                bigMode = true;
            }

            arrowSRLeftNPC.sprite = redArrowSprite;
            BeatAction.New(doughDudesNPC, new List<BeatAction.Action>()
            {
                //Jump and play sound
                new BeatAction.Action(beat + 0.1f, delegate { arrowSRLeftNPC.sprite = whiteArrowSprite; }),
                new BeatAction.Action(beat + 1f, delegate { doughDudesNPC.GetComponent<Animator>().Play(isBig ? "BigDoughJump" :"SmallDoughJump", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { Jukebox.PlayOneShotGame(isBig ? "workingDough/NPCBigBall" : "workingDough/NPCSmallBall"); }),
                new BeatAction.Action(beat + 1f, delegate { npcImpact.SetActive(true); }),
                new BeatAction.Action(beat + 1.1f, delegate { npcImpact.SetActive(false); }),
                new BeatAction.Action(beat + 1.9f, delegate { arrowSRRightNPC.sprite = redArrowSprite; }),
                new BeatAction.Action(beat + 2f, delegate { arrowSRRightNPC.sprite = whiteArrowSprite; }),
            });
        }

        public void InstantExitBall(float beat, bool isBig, float offSet)
        {
            var objectToSpawn = isBig ? bigBallNPC : smallBallNPC;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<NPCDoughBall>();
            ballComponent.startBeat = beat - 1;
            ballComponent.exitUpCurve = npcExitUpCurve;
            ballComponent.enterUpCurve = npcEnterUpCurve;
            ballComponent.exitDownCurve = npcExitDownCurve;
            ballComponent.enterDownCurve = npcEnterDownCurve;
            ballComponent.currentFlyingStage = (FlyingStage)(2 - Mathf.Abs(offSet * 2));

            spawnedBall.SetActive(true);

            if (isBig && !bigMode)
            {
                bigMode = true;
            }
            
            BeatAction.New(doughDudesNPC, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { doughDudesNPC.GetComponent<Animator>().Play(isBig ? "BigDoughJump" : "SmallDoughJump", 0, 0); } ),
                new BeatAction.Action(beat, delegate { Jukebox.PlayOneShotGame(isBig ? "workingDough/NPCBigBall" : "workingDough/NPCSmallBall"); } ),
                new BeatAction.Action(beat, delegate { npcImpact.SetActive(true); } ),
                new BeatAction.Action(beat + 0.1f, delegate { npcImpact.SetActive(false); }),
                new BeatAction.Action(beat + 0.9f, delegate { arrowSRRightNPC.sprite = redArrowSprite; }),
                new BeatAction.Action(beat + 1f, delegate { arrowSRRightNPC.sprite = whiteArrowSprite; }),
            });
        }

        public void PreSpawnBall(float beat, bool isBig)
        {
            float spawnBeat = beat - 1f;
            beat -= 1f;
            if (GameManager.instance.currentGame == "workingDough")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(spawnBeat, delegate
                    {
                        if (!isPlaying(ballTransporterLeftNPC.GetComponent<Animator>(), "BallTransporterLeftOpened") && !intervalStarted)
                        {
                            ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0);
                            ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                        }
                    }),
                    new BeatAction.Action(spawnBeat, delegate { if (instance != null) instance.SpawnBall(beat, isBig); }),
                    new BeatAction.Action(spawnBeat + beatInterval, delegate { SpawnPlayerBall(beat + beatInterval, isBig); }),
                });
            }
            else
            {
                queuedBalls.Add(new QueuedBall()
                {
                    beat = beat + 1f,
                    isBig = isBig,
                });
            }
        }

        public void SpawnPlayerBall(float beat, bool isBig)
        {
            var objectToSpawn = isBig ? playerEnterBigBall : playerEnterSmallBall;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<PlayerEnterDoughBall>();
            ballComponent.startBeat = beat;
            ballComponent.firstCurve = playerEnterUpCurve;
            ballComponent.secondCurve = playerEnterDownCurve;
            ballComponent.deletingAutomatically = false;
            currentBalls.Add(spawnedBall);

            spawnedBall.SetActive(true);

            if (isBig && !bigModePlayer)
            {
                PlayerBallTransporters.GetComponent<Animator>().Play("PlayerGoBigMode", 0, 0);
                bigModePlayer = true;
            }

            if (isBig)
            {
                ScheduleInput(beat, 1, InputType.STANDARD_ALT_DOWN, JustBig, MissBig, Nothing);
                ScheduleUserInput(beat, 1, InputType.STANDARD_DOWN, WrongInputBig, Nothing, Nothing);
            }
            else
            {
                ScheduleInput(beat, 1, InputType.STANDARD_DOWN, JustSmall, MissSmall, Nothing);
                ScheduleUserInput(beat, 1, InputType.STANDARD_ALT_DOWN, WrongInputSmall, Nothing, Nothing);
            }

            arrowSRLeftPlayer.sprite = redArrowSprite;


            BeatAction.New(doughDudesPlayer, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.1f, delegate { arrowSRLeftPlayer.sprite = whiteArrowSprite; }),
            });
        }

        public void SpawnPlayerBallResult(float beat, bool isBig, BezierCurve3D firstCurve, BezierCurve3D secondCurve, float firstBeatsToTravel, float secondBeatsToTravel)
        {
            var objectToSpawn = isBig ? playerEnterBigBall : playerEnterSmallBall;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<PlayerEnterDoughBall>();
            ballComponent.startBeat = beat;
            ballComponent.firstCurve = firstCurve;
            ballComponent.secondCurve = secondCurve;
            ballComponent.firstBeatsToTravel = firstBeatsToTravel;
            ballComponent.secondBeatsToTravel = secondBeatsToTravel;

            spawnedBall.SetActive(true);
        }

        public void PreSetIntervalStart(float beat, float interval)
        {
            beat -= 1f;
            if (GameManager.instance.currentGame == "workingDough")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        if (!isPlaying(ballTransporterLeftNPC.GetComponent<Animator>(), "BallTransporterLeftOpened"))
                        {
                            ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0);
                            ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                        }
                    }),
                    new BeatAction.Action(beat + 1, delegate { if (instance != null) instance.SetIntervalStart(beat + 1, interval); }),
                });
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval()
                {
                    beat = beat + 1f,
                    interval = interval - 1f,
                });
            }
        }

        void Update()
        {
            Conductor cond = Conductor.instance;
            if (!cond.isPlaying || cond.isPaused) return;
            if (queuedIntervals.Count > 0)
            {
                foreach (var interval in queuedIntervals)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(interval.beat, delegate { SetIntervalStart(interval.beat, interval.interval); beatInterval += 1;  }),
                    });

                }
                queuedIntervals.Clear();
            }
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                doughDudesPlayer.GetComponent<Animator>().Play("SmallDoughJump", 0, 0);
                Jukebox.PlayOneShotGame("workingDough/PlayerSmallJump");
            }
            else if (PlayerInput.AltPressed() && !IsExpectingInputNow(InputType.STANDARD_ALT_DOWN))
            {
                doughDudesPlayer.GetComponent<Animator>().Play("BigDoughJump", 0, 0);
                Jukebox.PlayOneShotGame("workingDough/PlayerBigJump");
            }
        }
        
        void LateUpdate()
        {
            Conductor cond = Conductor.instance;
            if (!cond.isPlaying || cond.isPaused) return;
            if (queuedBalls.Count > 0)
            {
                foreach (var ball in queuedBalls)
                {
                    if (ball.isBig) NPCBallTransporters.GetComponent<Animator>().Play("BigMode", 0, 0);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(ball.beat - (ball.beat - intervalStartBeat), delegate { if (!intervalStarted) SetIntervalStart(ball.beat, beatInterval); }),
                        new BeatAction.Action(ball.beat - (ball.beat - intervalStartBeat), delegate { InstantExitBall(ball.beat, ball.isBig, ball.beat - intervalStartBeat); }),
                    });

                }
                queuedBalls.Clear();
            }
        }

        void WrongInputBig(PlayerActionEvent caller, float state)
        {

        }

        void WrongInputSmall(PlayerActionEvent caller, float state)
        {

        }

        void MissBig(PlayerActionEvent caller)
        {
            GameObject currentBall = currentBalls[0];
            currentBalls.Remove(currentBall);
            GameObject.Destroy(currentBall);
            
            float beat = caller.startBeat + caller.timer;
            SpawnPlayerBallResult(beat, true, playerMissCurveFirst, playerMissCurveSecond, 0.25f, 0.75f);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.25f, delegate { missImpact.SetActive(true); }),
                new BeatAction.Action(beat + 0.25f, delegate { Jukebox.PlayOneShotGame("workingDough/BallMiss"); }),
                new BeatAction.Action(beat + 0.35f, delegate { missImpact.SetActive(false); }),
            });
        }
        void MissSmall(PlayerActionEvent caller)
        {
            GameObject currentBall = currentBalls[0];
            currentBalls.Remove(currentBall);
            GameObject.Destroy(currentBall);
            float beat = caller.startBeat + caller.timer;
            SpawnPlayerBallResult(beat, false, playerMissCurveFirst, playerMissCurveSecond, 0.25f, 0.75f);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.25f, delegate { missImpact.SetActive(true); }),
                new BeatAction.Action(beat + 0.25f, delegate { Jukebox.PlayOneShotGame("workingDough/BallMiss"); }),
                new BeatAction.Action(beat + 0.35f, delegate { missImpact.SetActive(false); }),
            });
        }

        void JustSmall(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            Success(false, caller.startBeat + caller.timer);
        }

        void JustBig(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            { 
                return;
            }
            Success(true, caller.startBeat + caller.timer);
        }

        void Success(bool isBig, float beat)
        {
            if (isBig)
            {
                Jukebox.PlayOneShotGame("workingDough/rightBig");
                doughDudesPlayer.GetComponent<Animator>().Play("BigDoughJump", 0, 0);
            }
            else
            {
                Jukebox.PlayOneShotGame("workingDough/rightSmall");
                doughDudesPlayer.GetComponent<Animator>().Play("SmallDoughJump", 0, 0);
            }
            playerImpact.SetActive(true);
            SpawnPlayerBallResult(beat, isBig, playerExitUpCurve, playerExitDownCurve, 0.5f, 0.5f);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.1f, delegate { playerImpact.SetActive(false); }),
                new BeatAction.Action(beat + 0.9f, delegate { arrowSRRightPlayer.sprite = redArrowSprite; }),
                new BeatAction.Action(beat + 1f, delegate { arrowSRRightPlayer.sprite = whiteArrowSprite; }),
            });
        }

        void Nothing (PlayerActionEvent caller) {}

        //Function to make life for my fingers and my and your eyes easier
        bool isPlaying(Animator anim, string stateName)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                    anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                return true;
            else
                return false;
        }
    }
}
