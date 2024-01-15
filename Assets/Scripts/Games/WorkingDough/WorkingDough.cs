using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

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
                    preFunction = delegate { var e = eventCaller.currentEntity; WorkingDough.PreSetIntervalStart(e.beat, e.length, e["auto"]);  },
                    defaultLength = 8f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval.")
                    }
                },
                new GameAction("small ball", "Small Ball")
                {
                    defaultLength = 0.5f,
                    priority = 1,
                },
                new GameAction("big ball", "Big Ball")
                {
                    defaultLength = 0.5f,
                    priority = 1,
                    parameters = new List<Param>()
                    {
                        new Param("hasGandw", false, "Mr. Game & Watch", "Toggle if Mr. Game & Watch should be riding on the ball.")
                    }
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    preFunction = delegate { WorkingDough.PrePassTurn(eventCaller.currentEntity.beat); },
                    preFunctionLength = 1
                },
                new GameAction("rise spaceship", "Rise Up Spaceship")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.RiseUpShip(e.beat, e.length);  },
                    defaultLength = 4f,
                    resizable = true,
                    priority = 0
                },
                new GameAction("launch spaceship", "Launch Spaceship")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.LaunchShip(e.beat, e.length);  },
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
                    new Param("toggle", false, "Up", "Toggle if the dough dudes should go up or down.")
                    },
                    resizable = true,
                    priority = 0
                },
                new GameAction("instant lift", "Instant Lift")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.InstantElevation(e["toggle"]);  },
                    parameters = new List<Param>()
                    {
                    new Param("toggle", true, "Up", "Toggle if the dough dudes should go up or down.")
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
                    new Param("toggle", false, "Exit", "Toggle if Mr. Game & Watch should exit or enter the scene.")
                    },
                    resizable = true,
                    priority = 0
                },
                new GameAction("instant game and watch", "Instant Mr. G&W Enter or Exit")
                {
                    function = delegate { var e = eventCaller.currentEntity; WorkingDough.instance.InstantGANDWEnterOrExit(e["toggle"]);  },
                    parameters = new List<Param>()
                    {
                    new Param("toggle", false, "Exit", "Toggle if Mr. Game & Watch should exit or enter the scene.")
                    },
                    defaultLength = 0.5f,
                    priority = 0
                },
                new GameAction("disableBG", "Toggle Background")
                {
                    function = delegate { WorkingDough.instance.DisableBG(eventCaller.currentEntity["ship"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("ship", false, "Spaceship Only", "Toggle if the only the spaceship should be toggled.")
                    }
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
    using Jukebox;
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
        [SerializeField] private GameObject shipObject;
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
        static List<double> passedTurns = new List<double>();
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

        const int IA_AltPress = IAMAXCAT;
        protected static bool IA_TouchNrmPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && !instance.IsExpectingInputNow(InputAction_Alt);
        }

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_Alt);
        }

        public static PlayerInput.InputAction InputAction_Nrm =
            new("RvlDoughAlt", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadBasicPress, IA_TouchNrmPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_Alt =
            new("RvlDoughAlt", new int[] { IA_AltPress, IA_AltPress, IA_AltPress },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            conveyerAnimator.Play("ConveyerBelt", 0, 0);
            doughDudesHolderAnim.Play("OnGround", 0, 0);
        }

        private bool shipOnly;

        public void DisableBG(bool ship)
        {
            shipOnly = ship;
            bgDisabled = !bgDisabled;
            foreach (var bgObject in bgObjects)
            {
                bgObject.SetActive(!bgDisabled || shipOnly);
            }
            shipObject.SetActive(!bgDisabled && !shipOnly);
        }

        private List<RiqEntity> GetAllBallsInBetweenBeat(double beat, double endBeat)
        {
            List<RiqEntity> ballEvents = EventCaller.GetAllInGameManagerList("workingDough", new string[] { "small ball", "big ball" });
            List<RiqEntity> tempEvents = new();

            foreach (var entity in ballEvents)
            {
                if (entity.beat >= beat && entity.beat < endBeat)
                {
                    tempEvents.Add(entity);
                }
            }
            return tempEvents;
        }

        private RiqEntity GetLastIntervalBeforeBeat(double beat)
        {
            List<RiqEntity> intervalEvents = EventCaller.GetAllInGameManagerList("workingDough", new string[] { "beat intervals" });
            if (intervalEvents.Count == 0) return null;
            var tempEvents = intervalEvents.FindAll(x => x.beat <= beat);
            tempEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            return tempEvents[^1];
        }

        public void SetIntervalStart(double beat, double gameSwitchBeat, float interval = 8f, bool autoPassTurn = true)
        {
            List<RiqEntity> relevantBalls = GetAllBallsInBetweenBeat(beat, beat + interval);
            bool hasBigBall = false;
            foreach (var ball in relevantBalls)
            {
                bool isBig = ball.datamodel == "workingDough/big ball";
                if (ball.beat >= gameSwitchBeat)
                {
                    SpawnBall(ball.beat - 1, isBig, isBig && ball["hasGandw"]);
                    OnSpawnBall(ball.beat, isBig);
                }
                if (isBig) hasBigBall = true;
            }
            if (autoPassTurn)
            {
                PassTurn(beat + interval, interval, beat);
            }
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1, delegate
                {
                    bigMode = hasBigBall;
                    if (bigMode)
                    {
                        NPCBallTransporters.GetComponent<Animator>().Play("NPCGoBigMode", 0, 0);
                    }
                    if (!instance.ballTransporterLeftNPC.GetComponent<Animator>().IsPlayingAnimationNames("BallTransporterLeftOpened"))
                    {
                        instance.ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0);
                        instance.ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                        if (instance.gandwHasEntered && !bgDisabled) instance.gandwAnim.Play("GANDWLeverUp", 0, 0);
                    }
                }),
            });
        }

        public static void PrePassTurn(double beat)
        {
            if (GameManager.instance.currentGame == "workingDough")
            {
                instance.PassTurnStandalone(beat);
            }
            else
            {
                passedTurns.Add(beat);
            }
        }

        private void PassTurnStandalone(double beat)
        {
            RiqEntity lastInterval = GetLastIntervalBeforeBeat(beat);
            if (lastInterval == null) return;
            PassTurn(beat, lastInterval.length, lastInterval.beat);
        }

        private void PassTurn(double beat, double length, double startBeat)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1, delegate
                {
                    ballTransporterRightPlayer.GetComponent<Animator>().Play("BallTransporterRightOpen", 0, 0);
                    ballTransporterLeftPlayer.GetComponent<Animator>().Play("BallTransporterLeftOpen", 0, 0);
                    var relevantBallEvents = GetAllBallsInBetweenBeat(startBeat, startBeat + length);
                    if (relevantBallEvents.Count > 0)
                    {
                        bool hasBig = false;
                        foreach (var ball in relevantBallEvents)
                        {
                            double relativeBeat = ball.beat - startBeat;
                            bool isBig = ball.datamodel == "workingDough/big ball";
                            SpawnPlayerBall(beat + relativeBeat - 1, isBig, isBig ? ball["hasGandw"] : false);
                            if (isBig) hasBig = true;
                        }
                        bigModePlayer = hasBig;
                        if (bigModePlayer)
                        {
                            PlayerBallTransporters.GetComponent<Animator>().Play("PlayerGoBigMode", 0, 0);
                        }
                    }
                }),
                new BeatAction.Action(beat, delegate
                {
                    if (gandwHasEntered && !bgDisabled) gandwAnim.Play("MrGameAndWatchLeverDown", 0, 0);
                }),
                new BeatAction.Action(beat + 1, delegate 
                { 
                    if (beat + 1 > GetLastIntervalBeforeBeat(beat + 1).beat + GetLastIntervalBeforeBeat(beat + 1).length) 
                    {
                        ballTransporterLeftNPC.GetComponent<Animator>().Play("BallTransporterLeftClose", 0, 0);
                        ballTransporterRightNPC.GetComponent<Animator>().Play("BallTransporterRightClose", 0, 0);
                    }
                    if (bigMode)
                    {
                        NPCBallTransporters.GetComponent<Animator>().Play("NPCExitBigMode", 0, 0);
                        bigMode = false;
                    }
                }),
                //Close player transporters
                new BeatAction.Action(beat + length + 1, delegate 
                {
                    ballTransporterLeftPlayer.GetComponent<Animator>().Play("BallTransporterLeftClose", 0, 0);
                    ballTransporterRightPlayer.GetComponent<Animator>().Play("BallTransporterRightClose", 0, 0);
                    if (bigModePlayer)
                    {
                        PlayerBallTransporters.GetComponent<Animator>().Play("PlayerExitBigMode", 0, 0);
                        bigModePlayer = false;
                    }
                }),
            });
        }

        public void SpawnBall(double beat, bool isBig, bool hasGandw)
        {
            var objectToSpawn = isBig ? bigBallNPC : smallBallNPC;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<NPCDoughBall>();
            spawnedBall.SetActive(true);
            ballComponent.Init(beat, hasGandw);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                //Jump and play sound
                new BeatAction.Action(beat, delegate { arrowSRLeftNPC.sprite = redArrowSprite; }),
                new BeatAction.Action(beat + 0.1f, delegate { arrowSRLeftNPC.sprite = whiteArrowSprite; }),
                new BeatAction.Action(beat + 1f, delegate { doughDudesNPC.GetComponent<Animator>().DoScaledAnimationAsync(isBig ? "BigDoughJump" :"SmallDoughJump", 0.5f); }),
                new BeatAction.Action(beat + 1f, delegate { npcImpact.SetActive(true); }),
                new BeatAction.Action(beat + 1.1f, delegate { npcImpact.SetActive(false); }),
                new BeatAction.Action(beat + 1.9f, delegate { arrowSRRightNPC.sprite = redArrowSprite; }),
                new BeatAction.Action(beat + 2f, delegate { arrowSRRightNPC.sprite = whiteArrowSprite; }),
            });
        }

        public void OnSpawnBall(double beat, bool isBig)
        {
            SoundByte.PlayOneShotGame(isBig ? "workingDough/hitBigOther" : "workingDough/hitSmallOther", beat);
            SoundByte.PlayOneShotGame(isBig ? "workingDough/bigOther" : "workingDough/smallOther", beat);
        }

        public void SpawnPlayerBall(double beat, bool isBig, bool hasGandw)
        {
            var objectToSpawn = isBig ? playerEnterBigBall : playerEnterSmallBall;
            var spawnedBall = GameObject.Instantiate(objectToSpawn, ballHolder);

            var ballComponent = spawnedBall.GetComponent<PlayerEnterDoughBall>();
            spawnedBall.SetActive(true);
            ballComponent.Init(beat, isBig, hasGandw);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { arrowSRLeftPlayer.sprite = redArrowSprite; }),
                new BeatAction.Action(beat + 0.1f, delegate { arrowSRLeftPlayer.sprite = whiteArrowSprite; }),
            });
        }

        private struct QueuedInterval
        {
            public double beat;
            public float interval;
            public bool auto;
        }
        private static List<QueuedInterval> queuedIntervals = new();

        public static void PreSetIntervalStart(double beat, float interval, bool auto)
        {
            if (GameManager.instance.currentGame == "workingDough")
            {
                // instance.ballTriggerSetInterval = false;
                // beatInterval = interval;
                instance.SetIntervalStart(beat, beat, interval, auto);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval
                {
                    beat = beat,
                    interval = interval,
                    auto = auto
                });
            }
        }
        public override void OnGameSwitch(double beat)
        {
            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                if (queuedIntervals.Count > 0)
                {
                    foreach (var interval in queuedIntervals)
                    {
                        SetIntervalStart(interval.beat, beat, interval.interval, interval.auto);
                    }
                    queuedIntervals.Clear();
                }
            }
        }

        void Update()
        {
            if (spaceshipRising && !bgDisabled) spaceshipAnimator.DoScaledAnimation("RiseSpaceship", risingStartBeat, risingLength);
            if (liftingDoughDudes && !bgDisabled) doughDudesHolderAnim.DoScaledAnimation(liftingAnimName, liftingStartBeat, liftingLength);
            if (gandwMoving && !bgDisabled) gandwAnim.DoScaledAnimation(gandwMovingAnimName, gandMovingStartBeat, gandMovingLength);
            if (passedTurns.Count > 0)
            {
                foreach (var passTurn in passedTurns)
                {
                    PassTurnStandalone(passTurn);
                }
                passedTurns.Clear();
            }
            if (PlayerInput.GetIsAction(InputAction_Nrm) && !IsExpectingInputNow(InputAction_Nrm))
            {
                doughDudesPlayer.GetComponent<Animator>().DoScaledAnimationAsync("SmallDoughJump", 0.5f);
                SoundByte.PlayOneShotGame("workingDough/smallPlayer");
            }
            else if (PlayerInput.GetIsAction(InputAction_Alt) && !IsExpectingInputNow(InputAction_Alt))
            {
                doughDudesPlayer.GetComponent<Animator>().DoScaledAnimationAsync("BigDoughJump", 0.5f);
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
            BeatAction.New(instance, new List<BeatAction.Action>()
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
            BeatAction.New(instance, new List<BeatAction.Action>()
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
            BeatAction.New(instance, new List<BeatAction.Action>()
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
            BeatAction.New(instance, new List<BeatAction.Action>()
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
            BeatAction.New(instance, new List<BeatAction.Action>()
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
