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
            return new Minigame("workingDough", "Working Dough", "000000", false, false, new List<GameAction>()
            {
                new GameAction("beat intervals", "Start Interval")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.PreSetIntervalStart(e.beat, e.length);  },
                    defaultLength = 8f,
                    resizable = true,
                    priority = 2,
                },
                new GameAction("small ball", "Small Ball")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.PreSpawnBall(e.beat, false, false);  },
                    defaultLength = 0.5f,
                    priority = 1,
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.OnSpawnBallInactive(e.beat, false, false); },
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.OnSpawnBall(e.beat, false, false); }
                },
                new GameAction("big ball", "Big Ball")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.PreSpawnBall(e.beat, true, e["hasGandw"]);  },
                    defaultLength = 0.5f,
                    priority = 1,
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.OnSpawnBallInactive(e.beat, true, e["hasGandw"]); },
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.OnSpawnBall(e.beat, true, e["hasGandw"]); },
                    parameters = new List<Param>()
                    {
                        new Param("hasGandw", false, "Has Mr. Game & Watch")
                    }
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    preFunction = delegate { WorkingDough.PrePassTurn(eventCaller.currentEntity.beat); },
                    preFunctionLength = 1
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
                new GameAction("disableBG", "Toggle Background")
                {
                    function = delegate { WorkingDough.instance.DisableBG(); },
                    defaultLength = 0.5f
                }
            },
            new List<string>() {"rvl", "repeat"},
            "rvldough", "en",
            new List<string>() {}
            );
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
        public GameObject playerImpact;
        [SerializeField] GameObject smallBallNPC;
        [SerializeField] GameObject bigBallNPC;
        [SerializeField] Transform ballHolder;
        [SerializeField] SpriteRenderer arrowSRLeftNPC;
        [SerializeField] SpriteRenderer arrowSRRightNPC;
        [SerializeField] SpriteRenderer arrowSRLeftPlayer;
        public SpriteRenderer arrowSRRightPlayer;
        [SerializeField] GameObject NPCBallTransporters;
        [SerializeField] GameObject PlayerBallTransporters;
        [SerializeField] GameObject playerEnterSmallBall;
        [SerializeField] GameObject playerEnterBigBall;
        public GameObject missImpact;
        public Transform breakParticleHolder;
        public GameObject breakParticleEffect;
        public Animator backgroundAnimator;
        [SerializeField] Animator conveyerAnimator;
        [SerializeField] GameObject smallBGBall;
        [SerializeField] GameObject bigBGBall;
        [SerializeField] Animator spaceshipAnimator;
        [SerializeField] GameObject spaceshipLights;
        [SerializeField] Animator doughDudesHolderAnim;
        [SerializeField] Animator gandwAnim;

        [SerializeField] private GameObject[] bgObjects;
        private bool bgDisabled;

        [Header("Variables")]
        float risingLength = 4f;
        double risingStartBeat;
        float liftingLength = 4f;
        double liftingStartBeat;
        float gandMovingLength = 4f;
        double gandMovingStartBeat;
        public bool bigMode;
        public bool bigModePlayer;
        static List<QueuedBall> queuedBalls = new List<QueuedBall>();
        static List<double> passedTurns = new List<double>();
        struct QueuedBall
        {
            public double beat;
            public bool isBig;
            public bool hasGandw;
        }
        public bool spaceshipRisen = false;
        public bool spaceshipRising = false;
        bool liftingDoughDudes;
        string liftingAnimName;
        bool gandwHasEntered = true;
        bool gandwMoving;
        string gandwMovingAnimName;

        [Header("Curves")]
        [SerializeField] SuperCurveObject.Path[] ballBouncePaths;
        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in ballBouncePaths)
            {
                if (path.preview)
                {
                    smallBallNPC.GetComponent<NPCDoughBall>().DrawEditorGizmo(path);
                }
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

        [Header("Resources")]
        public Sprite whiteArrowSprite;
        public Sprite redArrowSprite;

        public static WorkingDough instance;
        private static CallAndResponseHandler crHandlerInstance;

        void Awake()
        {
            if (crHandlerInstance == null)
            {
                crHandlerInstance = new CallAndResponseHandler(8);
            }
            instance = this;
        }

        void Start()
        {
            conveyerAnimator.Play("ConveyerBelt", 0, 0);
            doughDudesHolderAnim.Play("OnGround", 0, 0);
        }

        public void DisableBG()
        {
            bgDisabled = !bgDisabled;
            foreach (var bgObject in bgObjects)
            {
                bgObject.SetActive(!bgDisabled);
            }
        }

        public void SetIntervalStart(double beat, float interval)
        {
            if (!crHandlerInstance.IntervalIsActive())
            {
                bigMode = false;
                BeatAction.New(ballTransporterLeftNPC, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 1, delegate
                    {
                        if (!instance.ballTransporterLeftNPC.GetComponent<Animator>().IsPlayingAnimationName("BallTransporterLeftOpened"))
                        {
                            instance.ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0);
                            instance.ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                            if (instance.gandwHasEntered && !bgDisabled) instance.gandwAnim.Play("GANDWLeverUp", 0, 0);
                        }
                    }),
                //Open player transporters
                    /*
                    new BeatAction.Action(beat + interval - 1f, delegate {
                         ballTransporterRightPlayer.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                    }),
                    new BeatAction.Action(beat + interval - 1f, delegate {
                        ballTransporterRightPlayer.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                    }),*/

                    //Close npc transporters
                    new BeatAction.Action(beat + interval, delegate {
                        if (bigMode)
                        {
                            NPCBallTransporters.GetComponent<Animator>().Play("NPCExitBigMode", 0, 0);
                            bigMode = false;
                        }
                    }),
                    /*
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
                    */
                });
            }
            crHandlerInstance.StartInterval(beat, interval);
        }

        public static void PrePassTurn(double beat)
        {
            if (GameManager.instance.currentGame == "workingDough")
            {
                instance.PassTurn(beat);
            }
            else
            {
                passedTurns.Add(beat);
            }
        }

        private void PassTurn(double beat)
        {
            if (crHandlerInstance.queuedEvents.Count > 0)
            {
                ballTransporterRightPlayer.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                ballTransporterLeftPlayer.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0);
                foreach (var ball in crHandlerInstance.queuedEvents)
                {
                    SpawnPlayerBall(beat + ball.relativeBeat - 1, ball.tag == "big", ball["hasGandw"]);
                }
                crHandlerInstance.queuedEvents.Clear();
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        if (crHandlerInstance.queuedEvents.Count > 0)
                        {
                            foreach (var ball in crHandlerInstance.queuedEvents)
                            {
                                SpawnPlayerBall(beat + ball.relativeBeat - 1, ball.tag == "big", ball["hasGandw"]);
                            }
                            crHandlerInstance.queuedEvents.Clear();
                        }
                    }),
                    new BeatAction.Action(beat + 1, delegate { if (!crHandlerInstance.IntervalIsActive()) ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftClose", 0, 0); }),
                    new BeatAction.Action(beat + 1, delegate { if (!crHandlerInstance.IntervalIsActive()) ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightClose", 0, 0); }),
                    new BeatAction.Action(beat + 1, delegate { if (gandwHasEntered && !bgDisabled) gandwAnim.Play("MrGameAndWatchLeverDown", 0, 0); }),
                    //Close player transporters
                    new BeatAction.Action(beat + crHandlerInstance.intervalLength + 1, delegate { ballTransporterLeftPlayer.GetComponent<Animator>().Play("BallTransporterLeftClose", 0, 0); }),
                    new BeatAction.Action(beat + crHandlerInstance.intervalLength + 1, delegate { ballTransporterRightPlayer.GetComponent<Animator>().Play("BallTransporterRightClose", 0, 0); }),
                    new BeatAction.Action(beat + crHandlerInstance.intervalLength + 1, delegate {
                        if (bigModePlayer)
                        {
                            PlayerBallTransporters.GetComponent<Animator>().Play("PlayerExitBigMode", 0, 0);
                            bigModePlayer = false;
                        }
                    }),
                });
            }
        }

        public void SpawnBall(double beat, bool isBig, bool hasGandw)
        {
            var objectToSpawn = isBig ? bigBallNPC : smallBallNPC;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<NPCDoughBall>();
            spawnedBall.SetActive(true);
            ballComponent.Init(beat, hasGandw);


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
                new BeatAction.Action(beat + 1f, delegate { npcImpact.SetActive(true); }),
                new BeatAction.Action(beat + 1.1f, delegate { npcImpact.SetActive(false); }),
                new BeatAction.Action(beat + 1.9f, delegate { arrowSRRightNPC.sprite = redArrowSprite; }),
                new BeatAction.Action(beat + 2f, delegate { arrowSRRightNPC.sprite = whiteArrowSprite; }),
            });
        }

        public static void PreSpawnBall(double beat, bool isBig, bool hasGandw)
        {
            double spawnBeat = beat - 1f;
            beat -= 1f;
            if (GameManager.instance.currentGame == "workingDough")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(spawnBeat, delegate
                    {
                        if (!instance.ballTransporterLeftNPC.GetComponent<Animator>().IsPlayingAnimationName("BallTransporterLeftOpened") && !crHandlerInstance.IntervalIsActive() && !instance.bgDisabled)
                        {
                            instance.ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0);
                            instance.ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                            if (instance.gandwHasEntered) instance.gandwAnim.Play("GANDWLeverUp", 0, 0);
                        }
                    }),
                    new BeatAction.Action(spawnBeat, delegate { if (instance != null) instance.SpawnBall(beat, isBig, hasGandw); }),
                    // new BeatAction.Action(spawnBeat + instance.beatInterval, delegate { instance.SpawnPlayerBall(beat + instance.beatInterval, isBig); }),
                });
            }
            else
            {
                queuedBalls.Add(new QueuedBall()
                {
                    beat = beat + 1f,
                    isBig = isBig,
                    hasGandw = hasGandw
                });
            }
        }

        public static void OnSpawnBallInactive(double beat, bool isBig, bool hasGandw)
        {
            if (crHandlerInstance == null)
            {
                crHandlerInstance = new CallAndResponseHandler(8);
            }
            crHandlerInstance.AddEvent(beat, 0, isBig ? "big" : "small", new List<CallAndResponseHandler.CallAndResponseEventParam>()
            {
                new CallAndResponseHandler.CallAndResponseEventParam("hasGandw", hasGandw)
            });
        }

        public void OnSpawnBall(double beat, bool isBig, bool hasGandw)
        {
            crHandlerInstance.AddEvent(beat, 0, isBig ? "big" : "small", new List<CallAndResponseHandler.CallAndResponseEventParam>()
            {
                new CallAndResponseHandler.CallAndResponseEventParam("hasGandw", hasGandw)
            });
            SoundByte.PlayOneShotGame(isBig ? "workingDough/hitBigOther" : "workingDough/hitSmallOther");
            SoundByte.PlayOneShotGame(isBig ? "workingDough/bigOther" : "workingDough/smallOther");
        }

        public static void InactiveInterval(double beat, float interval)
        {
            if (crHandlerInstance == null)
            {
                crHandlerInstance = new CallAndResponseHandler(8);
            }
            crHandlerInstance.StartInterval(beat, interval);
        }

        public void SpawnPlayerBall(double beat, bool isBig, bool hasGandw)
        {
            var objectToSpawn = isBig ? playerEnterBigBall : playerEnterSmallBall;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<PlayerEnterDoughBall>();
            spawnedBall.SetActive(true);
            ballComponent.Init(beat, isBig, hasGandw);

            if (isBig && !bigModePlayer)
            {
                PlayerBallTransporters.GetComponent<Animator>().Play("PlayerGoBigMode", 0, 0);
                bigModePlayer = true;
            }

            BeatAction.New(doughDudesPlayer, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { arrowSRLeftPlayer.sprite = redArrowSprite; }),
                new BeatAction.Action(beat + 0.1f, delegate { arrowSRLeftPlayer.sprite = whiteArrowSprite; }),
            });
        }

        public static void PreSetIntervalStart(double beat, float interval)
        {
            if (GameManager.instance.currentGame == "workingDough")
            {
                // instance.ballTriggerSetInterval = false;
                // beatInterval = interval;
                instance.SetIntervalStart(beat, interval);
            }
            else
            {
                InactiveInterval(beat, interval);
            }
        }

        void OnDestroy()
        {
            if (crHandlerInstance != null && !Conductor.instance.isPlaying)
            {
                crHandlerInstance = null;
            }
            if (queuedBalls.Count > 0) queuedBalls.Clear();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        public override void OnGameSwitch(double beat)
        {
            if (queuedBalls.Count > 0)
            {
                foreach (var ball in queuedBalls)
                {
                    if (ball.isBig) NPCBallTransporters.GetComponent<Animator>().Play("BigMode", 0, 0);
                    if (!crHandlerInstance.IntervalIsActive())
                    {
                        ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftOpened", 0, 0);
                        ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightOpened", 0, 0);
                        if (gandwHasEntered && !bgDisabled) gandwAnim.Play("GANDWLeverUp", 0, 0);
                    }
                    if (ball.beat > beat - 1)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(ball.beat - 1, delegate { SpawnBall(ball.beat - 1, ball.isBig, ball.hasGandw); })
                        });
                    }

                }
                queuedBalls.Clear();
            }
        }

        void Update()
        {
            Conductor cond = Conductor.instance;
            if (!cond.isPlaying || cond.isPaused)
            {
                if (queuedBalls.Count > 0) queuedBalls.Clear();
            }

            if (spaceshipRising && !bgDisabled) spaceshipAnimator.DoScaledAnimation("RiseSpaceship", risingStartBeat, risingLength);
            if (liftingDoughDudes && !bgDisabled) doughDudesHolderAnim.DoScaledAnimation(liftingAnimName, liftingStartBeat, liftingLength);
            if (gandwMoving && !bgDisabled) gandwAnim.DoScaledAnimation(gandwMovingAnimName, gandMovingStartBeat, gandMovingLength);
            if (passedTurns.Count > 0)
            {
                foreach (var passTurn in passedTurns)
                {
                    PassTurn(passTurn);
                }
                passedTurns.Clear();
            }
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                doughDudesPlayer.GetComponent<Animator>().Play("SmallDoughJump", 0, 0);
                SoundByte.PlayOneShotGame("workingDough/smallPlayer");
            }
            else if (PlayerInput.AltPressed() && !IsExpectingInputNow(InputType.STANDARD_ALT_DOWN))
            {
                doughDudesPlayer.GetComponent<Animator>().Play("BigDoughJump", 0, 0);
                SoundByte.PlayOneShotGame("workingDough/bigPlayer");
            }
        }

        public void SpawnBGBall(double beat, bool isBig, bool hasGandw)
        {
            var objectToSpawn = isBig ? bigBGBall : smallBGBall;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<BGBall>();
            spawnedBall.SetActive(true);
            ballComponent.Init(beat, hasGandw);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 9f, delegate { if (!spaceshipRisen && !bgDisabled) spaceshipAnimator.Play("AbsorbBall", 0, 0); }),
            });
        }

        public void InstantElevation(bool isUp)
        {
            doughDudesHolderAnim.Play(isUp ? "InAir" : "OnGround", 0, 0);
        }

        public void Elevate(double beat, float length, bool isUp)
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

        public void LaunchShip(double beat, float length)
        {
            if (bgDisabled) return;
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
                new BeatAction.Action(beat + length, delegate { SoundByte.PlayOneShotGame("workingDough/LaunchRobot"); }),
            });
        }

        public void RiseUpShip(double beat, float length)
        {
            if (bgDisabled) return;
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

        public void GANDWEnterOrExit(double beat, float length, bool shouldExit)
        {
            if (bgDisabled) return;
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
            if (bgDisabled) return;
            gandwAnim.Play(shouldExit ? "GANDWLeft" : "MrGameAndWatchLeverDown", 0, 0);
            gandwHasEntered = shouldExit ? false : true;
        }
    }
}
