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
            return new Minigame("workingDough", "Working Dough", "090909", false, false, new List<GameAction>()
            {
                new GameAction("beat intervals", "Start Interval")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.SetIntervalStart(e.beat, e.length);  },
                    preFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.PreSetIntervalStart(e.beat, e.length);  },
                    defaultLength = 8f,
                    resizable = true,
                    priority = 2
                },
                new GameAction("small ball", "Small Ball")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.OnSpawnBall(e.beat, false);  },
                    preFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.PreSpawnBall(e.beat, false);  },
                    defaultLength = 0.5f,
                    priority = 1
                },
                new GameAction("big ball", "Big Ball")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.OnSpawnBall(e.beat, true);  },
                    preFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.PreSpawnBall(e.beat, true);  },
                    defaultLength = 0.5f,
                    priority = 1
                },
                new GameAction("launch spaceship", "Launch Spaceship")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.LaunchShip(e.beat, e.length);  },
                    defaultLength = 4f,
                    resizable = true,
                    priority = 0
                },
                new GameAction("rise spaceship", "Rise Up Spaceship")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.RiseUpShip(e.beat, e.length);  },
                    defaultLength = 4f,
                    resizable = true,
                    priority = 0
                },
                new GameAction("lift dough dudes", "Lift Dough Dudes")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.Elevate(e.beat, e.length, e["toggle"]);  },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                    new Param("toggle", false, "Go Up?", "Toggle to go Up or Down.")
                    },
                    resizable = true,
                    priority = 0
                },
                new GameAction("instant lift", "Instant Lift")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.InstantElevation(e["toggle"]);  },
                    parameters = new List<Param>()
                    {
                    new Param("toggle", true, "Go Up?", "Toggle to go Up or Down.")
                    },
                    defaultLength = 0.5f,
                    priority = 0
                },
                new GameAction("mr game and watch enter or exit", "Mr. G&W Enter or Exit")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.GANDWEnterOrExit(e.beat, e.length, e["toggle"]);  },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                    new Param("toggle", false, "Should exit?", "Toggle to make him leave or enter.")
                    },
                    resizable = true,
                    priority = 0
                },
                new GameAction("instant game and watch", "Instant Mr. G&W Enter or Exit")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.InstantGANDWEnterOrExit(e["toggle"]);  },
                    parameters = new List<Param>()
                    {
                    new Param("toggle", false, "Exit?", "Toggle to make him leave or enter.")
                    },
                    defaultLength = 0.5f,
                    priority = 0
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
        [SerializeField] Transform breakParticleHolder;
        [SerializeField] GameObject breakParticleEffect;
        [SerializeField] Animator backgroundAnimator;
        [SerializeField] Animator conveyerAnimator;
        [SerializeField] GameObject smallBGBall;
        [SerializeField] GameObject bigBGBall;
        [SerializeField] Animator spaceshipAnimator;
        [SerializeField] GameObject spaceshipLights;
        [SerializeField] Animator doughDudesHolderAnim;
        [SerializeField] Animator gandwAnim;

        [Header("Variables")]
        public bool intervalStarted;
        float intervalStartBeat;
        float risingLength = 4f;
        float risingStartBeat;
        float liftingLength = 4f;
        float liftingStartBeat;
        public static float beatInterval = 8f;
        float gandMovingLength = 4f;
        float gandMovingStartBeat;
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
        public bool shouldMiss = true;
        public bool spaceshipRisen = false;
        public bool spaceshipRising = false;
        bool liftingDoughDudes;
        string liftingAnimName;
        bool ballTriggerSetInterval = true;
        bool gandwHasEntered = true;
        bool gandwMoving;
        string gandwMovingAnimName;

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
        public BezierCurve3D playerBarelyCurveFirst;
        public BezierCurve3D playerBarelyCurveSecond;
        public BezierCurve3D playerWrongInputTooWeakFirstCurve;
        public BezierCurve3D playerWrongInputTooWeakSecondCurve;
        public BezierCurve3D firstBGCurveBig;
        public BezierCurve3D secondBGCurveBig;
        public BezierCurve3D thirdBGCurveBig;
        public BezierCurve3D firstBGCurveSmall;
        public BezierCurve3D secondBGCurveSmall;
        public BezierCurve3D thirdBGCurveSmall;

        [Header("Resources")]
        public Sprite whiteArrowSprite;
        public Sprite redArrowSprite;

        public static WorkingDough instance;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            shouldMiss = true;
            ballTriggerSetInterval = true;
            conveyerAnimator.Play("ConveyerBelt", 0, 0);
            doughDudesHolderAnim.Play("OnGround", 0, 0);
        }

        public void SetIntervalStart(float beat, float interval)
        {
            Debug.Log("Start Interval");
            if (!intervalStarted)
            {
                instance.ballTriggerSetInterval = false;
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
                    new BeatAction.Action(beat + interval, delegate {ballTriggerSetInterval = true; }),

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
                    new BeatAction.Action(beat + interval + 1, delegate { if (gandwHasEntered) gandwAnim.Play("MrGameAndWatchLeverDown", 0, 0); }),
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
            beatInterval = interval;
            intervalStartBeat = beat;
        }

        public void SpawnBall(float beat, bool isBig)
        {
            if (!intervalStarted && ballTriggerSetInterval)
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

            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound(isBig ? "workingDough/NPCBigBall" : "workingDough/NPCSmallBall", beat + 1f),
            });

            arrowSRLeftNPC.sprite = redArrowSprite;
            BeatAction.New(doughDudesNPC, new List<BeatAction.Action>()
            {
                //Jump and play sound
                new BeatAction.Action(beat + 0.1f, delegate { arrowSRLeftNPC.sprite = whiteArrowSprite; }),
                new BeatAction.Action(beat + 1f, delegate { doughDudesNPC.GetComponent<Animator>().Play(isBig ? "BigDoughJump" :"SmallDoughJump", 0, 0); }),
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
            ballComponent.startBeat = beat - 1f;
            ballComponent.exitUpCurve = npcExitUpCurve;
            ballComponent.enterUpCurve = npcEnterUpCurve;
            ballComponent.exitDownCurve = npcExitDownCurve;
            ballComponent.enterDownCurve = npcEnterDownCurve;
            ballComponent.currentFlyingStage = (FlyingStage)(2 - Mathf.Abs(offSet));

            if (isBig && !bigMode)
            {
                bigMode = true;
            }

            if (beat >= Conductor.instance.songPositionInBeatsAsDouble)
            {
                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound(isBig ? "workingDough/NPCBigBall" : "workingDough/NPCSmallBall", beat),
                });
            }
            
            BeatAction.New(doughDudesNPC, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - offSet, delegate { spawnedBall.SetActive(true); }),
                new BeatAction.Action(beat, delegate { doughDudesNPC.GetComponent<Animator>().Play(isBig ? "BigDoughJump" : "SmallDoughJump", 0, 0); } ),
                new BeatAction.Action(beat, delegate { npcImpact.SetActive(true); } ),
                new BeatAction.Action(beat + 0.1f, delegate { npcImpact.SetActive(false); }),
                new BeatAction.Action(beat + 0.9f, delegate { arrowSRRightNPC.sprite = redArrowSprite; }),
                new BeatAction.Action(beat + 1f, delegate { arrowSRRightNPC.sprite = whiteArrowSprite; }),
            });
        }

        public static void PreSpawnBall(float beat, bool isBig)
        {
            float spawnBeat = beat - 1f;
            beat -= 1f;
            if (GameManager.instance.currentGame == "workingDough")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(spawnBeat, delegate
                    {
                        if (!instance.isPlaying(instance.ballTransporterLeftNPC.GetComponent<Animator>(), "BallTransporterLeftOpened") && !instance.intervalStarted && instance.ballTriggerSetInterval)
                        {
                            instance.ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0);
                            instance.ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                            if (instance.gandwHasEntered) instance.gandwAnim.Play("GANDWLeverUp", 0, 0);
                        }
                    }),
                    new BeatAction.Action(spawnBeat, delegate { if (instance != null) instance.SpawnBall(beat, isBig); }),
                    // new BeatAction.Action(spawnBeat + instance.beatInterval, delegate { instance.SpawnPlayerBall(beat + instance.beatInterval, isBig); }),
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

        public void OnSpawnBall(float beat, bool isBig)
        {
            beat -= 1f;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + beatInterval, delegate { instance.SpawnPlayerBall(beat + beatInterval, isBig); }),
            });
        }

        public void OnSpawnBallInactive(float beat, bool isBig)
        {
            queuedBalls.Add(new QueuedBall()
            {
                beat = beat + 1f,
                isBig = isBig,
            });
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

            if (isBig && !bigModePlayer)
            {
                PlayerBallTransporters.GetComponent<Animator>().Play("PlayerGoBigMode", 0, 0);
                bigModePlayer = true;
            }

            //shouldMiss = true;
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


            BeatAction.New(doughDudesPlayer, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { spawnedBall.SetActive(true); }),
                new BeatAction.Action(beat, delegate { arrowSRLeftPlayer.sprite = redArrowSprite; }),
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

        public static void PreSetIntervalStart(float beat, float interval)
        {
            beat -= 1f;
            if (GameManager.instance.currentGame == "workingDough")
            {
                // instance.ballTriggerSetInterval = false;
                // beatInterval = interval;
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        if (!instance.isPlaying(instance.ballTransporterLeftNPC.GetComponent<Animator>(), "BallTransporterLeftOpened"))
                        {
                            instance.ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0);
                            instance.ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                            if (instance.gandwHasEntered) instance.gandwAnim.Play("GANDWLeverUp", 0, 0);
                        }
                    }),
                    // new BeatAction.Action(beat + 1, delegate { if (instance != null) instance.SetIntervalStart(beat + 1, interval); }),
                });
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval()
                {
                    beat = beat + 1f,
                    interval = interval,
                });
            }
        }

        void OnDestroy()
        {
            if (queuedIntervals.Count > 0) queuedIntervals.Clear();
            if (queuedBalls.Count > 0) queuedBalls.Clear();
        }

        void Update()
        {
            Conductor cond = Conductor.instance;
            if (!cond.isPlaying || cond.isPaused)
            {
                if (queuedIntervals.Count > 0) queuedIntervals.Clear();
                if (queuedBalls.Count > 0) queuedBalls.Clear();
            }

            if (spaceshipRising) spaceshipAnimator.DoScaledAnimation("RiseSpaceship", risingStartBeat, risingLength);
            if (liftingDoughDudes) doughDudesHolderAnim.DoScaledAnimation(liftingAnimName, liftingStartBeat, liftingLength);
            if (gandwMoving) gandwAnim.DoScaledAnimation(gandwMovingAnimName, gandMovingStartBeat, gandMovingLength);
            if (queuedIntervals.Count > 0)
            {
                foreach (var interval in queuedIntervals)
                {
                    ballTriggerSetInterval = false;
                    beatInterval = interval.interval;
                    ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftOpened", 0, 0);
                    ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightOpened", 0, 0);
                    if (gandwHasEntered) gandwAnim.Play("GANDWLeverUp", 0, 0);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(interval.beat, delegate { SetIntervalStart(interval.beat, interval.interval); }),
                    });

                }
                queuedIntervals.Clear();
            }
            if (queuedBalls.Count > 0)
            {
                foreach (var ball in queuedBalls)
                {
                    float offSet = ball.beat - cond.songPositionInBeats;
                    float spawnOffset = offSet > 1f ? offSet - 1 : 0;
                    if (ball.isBig) NPCBallTransporters.GetComponent<Animator>().Play("BigMode", 0, 0);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(ball.beat - offSet + spawnOffset, delegate { 
                            if (!intervalStarted && ballTriggerSetInterval) 
                            {
                                ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftOpened", 0, 0);
                                ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightOpened", 0, 0);
                                if (gandwHasEntered) gandwAnim.Play("GANDWLeverUp", 0, 0);
                                SetIntervalStart(ball.beat, beatInterval); 
                            }
                        }),
                        new BeatAction.Action(ball.beat - offSet + spawnOffset, delegate { InstantExitBall(ball.beat, ball.isBig, offSet); }),
                        new BeatAction.Action(ball.beat + beatInterval - 1, delegate { SpawnPlayerBall(ball.beat + beatInterval - 1, ball.isBig); }),
                    });

                }
                queuedBalls.Clear();
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

        void WrongInputBig(PlayerActionEvent caller, float state)
        {
            float beat = caller.startBeat + caller.timer;
            shouldMiss = false;
            if (currentBalls.Count > 0)
            {
                GameObject currentBall = currentBalls[0];
                currentBalls.Remove(currentBall);
                GameObject.Destroy(currentBall);
            }
            doughDudesPlayer.GetComponent<Animator>().Play("SmallDoughJump", 0, 0);
            Jukebox.PlayOneShotGame("workingDough/BigBallTooWeak");
            SpawnPlayerBallResult(beat, true, playerWrongInputTooWeakFirstCurve, playerWrongInputTooWeakSecondCurve, 0.5f, 1f);
            playerImpact.SetActive(true);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.1f, delegate { playerImpact.SetActive(false); }),
            });
        }

        void WrongInputSmall(PlayerActionEvent caller, float state)
        {
            float beat = caller.startBeat + caller.timer;
            shouldMiss = false;
            if (currentBalls.Count > 0)
            {
                GameObject currentBall = currentBalls[0];
                currentBalls.Remove(currentBall);
                GameObject.Destroy(currentBall);
            }
            GameObject.Instantiate(breakParticleEffect, breakParticleHolder);
            doughDudesPlayer.GetComponent<Animator>().Play("BigDoughJump", 0, 0);
            Jukebox.PlayOneShotGame("workingDough/BreakBall");
            playerImpact.SetActive(true);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.1f, delegate { playerImpact.SetActive(false); }),
            });
        }

        void MissBig(PlayerActionEvent caller)
        {
            if (!shouldMiss)
            {
                shouldMiss = true;
                return;
            }
            if (currentBalls.Count > 0)
            {
                GameObject currentBall = currentBalls[0];
                currentBalls.Remove(currentBall);
                GameObject.Destroy(currentBall);
            }

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
            if (!shouldMiss)
            {
                shouldMiss = true;
                return;
            }

            if (currentBalls.Count > 0)
            {
                GameObject currentBall = currentBalls[0];
                currentBalls.Remove(currentBall);
                GameObject.Destroy(currentBall);
            }
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
            float beat = caller.startBeat + caller.timer;
            if (currentBalls.Count > 0)
            {
                GameObject currentBall = currentBalls[0];
                currentBalls.Remove(currentBall);
                GameObject.Destroy(currentBall);
            }
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("workingDough/SmallBarely");
                doughDudesPlayer.GetComponent<Animator>().Play("SmallDoughJump", 0, 0);

                playerImpact.SetActive(true);
                SpawnPlayerBallResult(beat, false, playerBarelyCurveFirst, playerBarelyCurveSecond, 0.75f, 1f);
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.1f, delegate { playerImpact.SetActive(false); }),
                });
                return;
            }
            Success(false, beat);
        }

        void JustBig(PlayerActionEvent caller, float state)
        {
            float beat = caller.startBeat + caller.timer;
            if (currentBalls.Count > 0)
            {
                GameObject currentBall = currentBalls[0];
                currentBalls.Remove(currentBall);
                GameObject.Destroy(currentBall);
            }
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("workingDough/BigBarely");
                doughDudesPlayer.GetComponent<Animator>().Play("BigDoughJump", 0, 0);

                playerImpact.SetActive(true);
                SpawnPlayerBallResult(beat, true, playerBarelyCurveFirst, playerBarelyCurveSecond, 0.75f, 1f);
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.1f, delegate { playerImpact.SetActive(false); }),
                });
                return;
            }
            Success(true, beat);
        }

        void Success(bool isBig, float beat)
        {
            if (isBig)
            {
                Jukebox.PlayOneShotGame("workingDough/rightBig");
                doughDudesPlayer.GetComponent<Animator>().Play("BigDoughJump", 0, 0);
                backgroundAnimator.Play("BackgroundFlash", 0, 0);
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
                new BeatAction.Action(beat + 2f, delegate { SpawnBGBall(beat + 2f, isBig); }),
            });
        }

        void SpawnBGBall(float beat, bool isBig)
        {
            var objectToSpawn = isBig ? bigBGBall : smallBGBall;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<BGBall>();
            ballComponent.startBeat = beat;
            ballComponent.firstCurve = isBig ? firstBGCurveBig : firstBGCurveSmall;
            ballComponent.secondCurve = isBig ? secondBGCurveBig : secondBGCurveSmall;
            ballComponent.thirdCurve = isBig ? thirdBGCurveBig : thirdBGCurveSmall;

            spawnedBall.SetActive(true);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 9f, delegate { if (!spaceshipRisen) spaceshipAnimator.Play("AbsorbBall", 0, 0); }),
            });
        }

        public void InstantElevation(bool isUp)
        {
            doughDudesHolderAnim.Play(isUp ? "InAir" : "OnGround", 0, 0);
        }

        public void Elevate(float beat, float length, bool isUp)
        {
            liftingAnimName = isUp ? "LiftUp" : "LiftDown";
            liftingStartBeat = beat;
            liftingLength = length;
            liftingDoughDudes = true;
            doughDudesHolderAnim.DoScaledAnimation(liftingAnimName, liftingStartBeat, liftingLength);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length - 0.1f, delegate { liftingDoughDudes = false; }),
            });
        } 

        public void LaunchShip(float beat, float length)
        {
            spaceshipRisen = true;
            if (!spaceshipLights.activeSelf)
            {
                spaceshipLights.SetActive(true);
                spaceshipLights.GetComponent<Animator>().Play("SpaceshipLights", 0, 0);
            }
            spaceshipAnimator.Play("SpaceshipShake", 0, 0);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate { spaceshipAnimator.Play("SpaceshipLaunch", 0, 0); }),
                new BeatAction.Action(beat + length, delegate { Jukebox.PlayOneShotGame("workingDough/LaunchRobot"); }),
                new BeatAction.Action(beat + length, delegate { Jukebox.PlayOneShotGame("workingDough/Rocket"); }),
            });
        }

        public void RiseUpShip(float beat, float length)
        { 
            spaceshipRisen = true;
            spaceshipRising = true;
            risingLength = length;
            risingStartBeat = beat;
            if (!spaceshipLights.activeSelf)
            {
                spaceshipLights.SetActive(true);
                spaceshipLights.GetComponent<Animator>().Play("SpaceshipLights", 0, 0);
            }
            spaceshipAnimator.DoScaledAnimation("RiseSpaceship", risingStartBeat, risingLength);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length - 0.1f, delegate { spaceshipRising = false; }),
            });
        }

        public void GANDWEnterOrExit(float beat, float length, bool shouldExit)
        {
            gandwMoving = true;
            gandwHasEntered = false;
            gandMovingLength = length;
            gandMovingStartBeat = beat;
            gandwMovingAnimName = shouldExit ? "GANDWLeave" : "GANDWEnter";
            gandwAnim.DoScaledAnimation(gandwMovingAnimName, gandMovingStartBeat, gandMovingLength);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length - 0.1f, delegate { gandwMoving = false; }),
                new BeatAction.Action(beat + length, delegate { gandwHasEntered = shouldExit ? false : true; }),
            });
        }

        public void InstantGANDWEnterOrExit(bool shouldExit)
        {
            gandwAnim.Play(shouldExit ? "GANDWLeft" : "MrGameAndWatchLeverDown", 0, 0);
            gandwHasEntered = shouldExit ? false : true;
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
