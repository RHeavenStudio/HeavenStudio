using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrPingpongLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("rhythmRally", "Rhythm Rally", "FFFFFF", true, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { RhythmRally.instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 
                    defaultLength = 0.5f, 
                    resizable = true
                },
                new GameAction("whistle", "Whistle")
                {
                    function = delegate { RhythmRally.instance.PlayWhistle(); },
                    defaultLength = 0.5f
                },
                new GameAction("toss ball", "Toss Ball")
                {
                    function = delegate { RhythmRally.instance.Toss(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, 6f, true); }, 
                    defaultLength = 2f
                },
                new GameAction("rally", "Rally")
                {
                    function = delegate { RhythmRally.instance.Serve(eventCaller.currentEntity.beat, RhythmRally.RallySpeed.Normal); }, 
                    defaultLength = 4f, 
                    resizable = true
                },
                new GameAction("slow rally", "Slow Rally")
                {
                    function = delegate { RhythmRally.instance.Serve(eventCaller.currentEntity.beat, RhythmRally.RallySpeed.Slow); }, 
                    defaultLength = 8f, 
                    resizable = true
                },
                new GameAction("fast rally", "Fast Rally")
                {
                    function = delegate { RhythmRally.instance.PrepareFastRally(eventCaller.currentEntity.beat, RhythmRally.RallySpeed.Fast); }, 
                    defaultLength = 6f
                },
                new GameAction("superfast rally", "Superfast Rally")
                {
                    function = delegate { RhythmRally.instance.PrepareFastRally(eventCaller.currentEntity.beat, RhythmRally.RallySpeed.SuperFast); }, 
                    defaultLength = 12f
                },
                new GameAction("pose", "End Pose")
                {
                    function = delegate { RhythmRally.instance.Pose(); }, 
                    defaultLength = 0.5f
                },
                new GameAction("camera", "Camera Controls")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        var rotation = new Vector3(0, e["valA"], 0);
                        RhythmRally.instance.ChangeCameraAngle(rotation, e["valB"], e.length, (Ease)e["type"], (RotateMode)e["type2"]);
                    }, 
                    defaultLength = 4, 
                    resizable = true, 
                    parameters = new List<Param>() {
                        new Param("valA", new EntityTypes.Integer(-360, 360, 0), "Angle", "The rotation of the camera around the center of the table"),
                        new Param("valB", new EntityTypes.Float(0.5f, 4f, 1), "Zoom", "The camera's level of zoom (Lower value = Zoomed in)"),
                        new Param("type", Ease.Linear, "Ease", "The easing function to use"),
                        new Param("type2", RotateMode.Fast, "Rotation Mode", "The rotation mode to use")
                    } 
                },
            },
            new List<string>() { "ntr", "keep" },
            "ntrpingpong", "en", "ver0",
            new List<string>() {},
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_RhythmRally;

    public class RhythmRally : Minigame
    {
        public enum RallySpeed { Slow, Normal, Fast, SuperFast }

        [Header("Camera")]
        public Transform renderQuadTrans;
        public Transform cameraPivot;


        [Header("Ball and curve info")]
        public GameObject ball;
        public GameObject ballShadow;
        public TrailRenderer ballTrail;
        public BezierCurve3D serveCurve;
        public BezierCurve3D returnCurve;
        public BezierCurve3D tossCurve;
        public BezierCurve3D missCurve;
        public GameObject ballHitFX;


        [Header("Animators")]
        public Animator playerAnim;
        public Animator opponentAnim;

        [Header("Properties")]
        public RallySpeed rallySpeed = RallySpeed.Normal;
        public bool started;
        public bool missed;
        public bool served;
        public bool tossing;
        public float serveBeat;
        public float targetBeat;
        public float tossBeat;
        public float missBeat;
        public float tossLength;
        private bool inPose;

        public Paddlers paddlers;

        public GameEvent bop = new GameEvent();
        
        public static RhythmRally instance;

        private void Awake()
        {
            instance = this;
            paddlers.Init();
            renderQuadTrans.gameObject.SetActive(true);

            var cam = GameCamera.instance.camera;
            var camHeight = 2f * cam.orthographicSize;
            var camWidth = camHeight * cam.aspect;
            renderQuadTrans.localScale = new Vector3(camWidth, camHeight, 1f);

            playerAnim.Play("Idle", 0, 0);
            opponentAnim.Play("Idle", 0, 0);
        }

        const float tableHitTime = 0.58f;
        bool opponentServing = false; // Opponent serving this frame?
        void Update()
        {
            var cond = Conductor.instance;
            var currentBeat = cond.songPositionInBeats;
            
            var hitBeat = serveBeat; // Beat when the last paddler hit the ball
            var beatDur1 = 1f; // From paddle to table
            var beatDur2 = 1f; // From table to other paddle

            var playerState = playerAnim.GetCurrentAnimatorStateInfo(0);
            var opponentState = opponentAnim.GetCurrentAnimatorStateInfo(0);

            bool playerPrepping = false; // Player using prep animation?
            bool opponentPrepping = false; // Opponent using prep animation?

            if (started)
            {
                // Determine hitBeat and beatDurs.
                switch (rallySpeed)
                {
                    case RallySpeed.Normal:
                        if (!served)
                        {
                            hitBeat = serveBeat + 2f;
                        }
                        break;
                    case RallySpeed.Fast:
                        if (!served)
                        {
                            hitBeat = serveBeat + 1f;
                            beatDur1 = 1f;
                            beatDur2 = 2f;
                        }
                        else
                        {
                            beatDur1 = 0.5f;
                            beatDur2 = 0.5f;
                        }
                        break;
                    case RallySpeed.SuperFast:
                        if (!served)
                        {
                            hitBeat = serveBeat + 1f;
                        }

                        beatDur1 = 0.5f;
                        beatDur2 = 0.5f;
                        break;
                    case RallySpeed.Slow:
                        if (!served)
                        {
                            hitBeat = serveBeat + 4f;
                        }

                        beatDur1 = 2f;
                        beatDur2 = 2f;
                        break;
                }


                // Ball position.
                var curveToUse = served ? serveCurve : returnCurve;
                float curvePosition;

                var hitPosition1 = cond.GetPositionFromBeat(hitBeat, beatDur1);
                if (hitPosition1 >= 1f)
                {
                    var hitPosition2 = cond.GetPositionFromBeat(hitBeat + beatDur1, beatDur2);
                    curvePosition = tableHitTime + hitPosition2 * (1f - tableHitTime);
                }
                else
                {
                    curvePosition = hitPosition1 * tableHitTime;
                }

                if (!missed)
                {
                    float curveHeight = 1.25f;
                    if ((rallySpeed == RallySpeed.Fast && served) || rallySpeed == RallySpeed.SuperFast)
                        curveHeight = 0.75f;
                    else if (rallySpeed == RallySpeed.Fast && !served && hitPosition1 >= 1f)
                        curveHeight = 2f;
                    else if (rallySpeed == RallySpeed.Slow)
                        curveHeight = 3f;

                    curveToUse.transform.localScale = new Vector3(1f, curveHeight, 1f);
                    ball.transform.position = curveToUse.GetPoint(Mathf.Max(0, curvePosition));

                    // Make ball inactive before it passes through the floor.
                    if (curvePosition > 1.05f)
                        ball.SetActive(false);
                }
                else
                {
                    if (tossing)
                    {
                        TossUpdate(tossBeat, tossLength);
                    }
                    else
                    {
                        var missPosition = cond.GetPositionFromBeat(missBeat, 1f);
                        ball.transform.position = missCurve.GetPoint(Mathf.Max(0, missPosition));

                        if (missPosition > 1f)
                            ball.SetActive(false);
                    }
                }

                // TODO: Make conditional so ball shadow only appears when over table.
                ballShadow.transform.position = new Vector3(ball.transform.position.x, -0.399f, ball.transform.position.z);


                var timeBeforeNextHit = hitBeat + beatDur1 + beatDur2 - currentBeat;

                // Check if the opponent should swing.
                if (!served && timeBeforeNextHit <= 0f)
                {
                    var rallies = GameManager.instance.Beatmap.entities.FindAll(c => c.datamodel == "rhythmRally/rally" || c.datamodel == "rhythmRally/slow rally");
                    for (int i = 0; i < rallies.Count; i++)
                    {
                        var rally = rallies[i];
                        if (rally.beat - currentBeat <= 0f && rally.beat + rally.length - currentBeat > 0f)
                        {
                            Serve(hitBeat + beatDur1 + beatDur2, rallySpeed);
                            opponentServing = true;
                            break;
                        }
                    }
                }

                // Check if paddler should do ready animation.
                bool readyToPrep;
                switch (rallySpeed)
                {
                    case RallySpeed.Slow:
                    case RallySpeed.Fast:
                        readyToPrep = timeBeforeNextHit <= 2f;
                        break;
                    case RallySpeed.SuperFast:
                        readyToPrep = timeBeforeNextHit <= 0.5f;
                        break;
                    default:
                        readyToPrep = timeBeforeNextHit <= 1f;
                        break;
                }

                // Paddler ready animation.
                if (readyToPrep && !opponentServing && !inPose)
                {
                    if (served)
                    {
                        playerPrepping = true;
                        if ((playerState.IsName("Swing") && playerAnim.IsAnimationNotPlaying()) || (!playerState.IsName("Swing") && !playerState.IsName("Ready1")))
                            playerAnim.Play("Ready1");
                    }
                    else if (!opponentServing)
                    {
                        opponentPrepping = true;
                        if ((opponentState.IsName("Swing") && opponentAnim.IsAnimationNotPlaying()) || (!opponentState.IsName("Swing") && !opponentState.IsName("Ready1")))
                        {
                            opponentAnim.Play("Ready1");

                            // Toss ball if it fell off the table.
                            if (missed && !tossing)
                            {
                                float tossHeight = 3f;

                                if (rallySpeed == RallySpeed.Slow || rallySpeed == RallySpeed.Fast)
                                    tossHeight = 6f;

                                Toss(hitBeat + beatDur1, beatDur2, tossHeight);
                            }
                        }
                        
                        // If player never swung and is still in ready state, snap them out of it.
                        if (missed && playerState.IsName("Ready1"))
                            playerAnim.Play("Beat");
                    }
                }
            }
            else
            {
                if (tossing)
                {
                    TossUpdate(tossBeat, tossLength);
                }
            }


            // Paddler bop animation.
            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (currentBeat >= bop.startBeat && currentBeat < bop.startBeat + bop.length && !inPose)
                {
                    if (!playerPrepping && (playerAnim.IsAnimationNotPlaying() || playerState.IsName("Idle") || playerState.IsName("Beat")))
                        playerAnim.Play("Beat", 0, 0);

                    if (!opponentPrepping && !opponentServing && !tossing && (opponentAnim.IsAnimationNotPlaying() || opponentState.IsName("Idle") || opponentState.IsName("Beat")))
                        opponentAnim.Play("Beat", 0, 0);
                }
            }

            opponentServing = false;
        }

        public void Bop(float beat, float length)
        {
            bop.length = length;
            bop.startBeat = beat;
        }

        public void Serve(float beat, RallySpeed speed)
        {
            if (!ball.activeSelf)
                ball.SetActive(true);

            if (!ballTrail.gameObject.activeSelf)
                ballTrail.gameObject.SetActive(true);

            served = true;
            missed = false;
            started = true;
            opponentServing = true;
            tossing = false;
            
            serveBeat = beat;
            rallySpeed = speed;

            var bounceBeat = 0f;

            switch (rallySpeed)
            {
                case RallySpeed.Normal:
                    targetBeat = serveBeat + 2f;
                    bounceBeat = serveBeat + 1f;
                    break;
                case RallySpeed.Fast:
                case RallySpeed.SuperFast:
                    targetBeat = serveBeat + 1f;
                    bounceBeat = serveBeat + 0.5f;
                    break;
                case RallySpeed.Slow:
                    targetBeat = serveBeat + 4f;
                    bounceBeat = serveBeat + 2f;
                    break;
            }

            opponentAnim.Play("Swing", 0, 0);
            MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound("rhythmRally/Serve", serveBeat), new MultiSound.Sound("rhythmRally/ServeBounce", bounceBeat) });
            paddlers.BounceFX(bounceBeat);

            paddlers.ResetState();
        }

        public void Toss(float beat, float length, float height, bool firstToss = false)
        {
            // Hide trail while tossing to prevent weirdness while teleporting ball.
            ballTrail.gameObject.SetActive(false);

            tossCurve.transform.localScale = new Vector3(1f, height, 1f);
            tossBeat = beat;
            tossLength = length;
            tossing = true;

            if (firstToss)
            {
                opponentAnim.Play("Ready1");
            }

            if (!ball.activeSelf)
                ball.SetActive(true);
        }

        private void TossUpdate(float beat, float duration)
        {
            var tossPosition = Conductor.instance.GetPositionFromBeat(beat, duration);
            ball.transform.position = tossCurve.GetPoint(Mathf.Clamp(tossPosition, 0, 1));

            if (tossPosition > 1.05f)
                ball.SetActive(false);
        }

        public void PlayWhistle()
        {
            Jukebox.PlayOneShotGame("rhythmRally/Whistle");
        }

        public void Pose()
        {
            playerAnim.Play("Pose", 0, 0);
            opponentAnim.Play("Pose", 0, 0);
            ball.SetActive(false); // temporary solution, should realistically just fall down
            inPose = true;
        }

        public void ChangeCameraAngle(Vector3 rotation, float camZoom, float length, Ease ease, RotateMode rotateMode)
        {
            var len = length * Conductor.instance.secPerBeat;
            cameraPivot.DORotate(rotation, len, rotateMode).SetEase(ease);
            cameraPivot.DOScale(camZoom, len).SetEase(ease);
        }

        public void PrepareFastRally(float beat, RallySpeed speedChange)
        {
            if (speedChange == RallySpeed.Fast)
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2f, delegate { Serve(beat + 2f, RallySpeed.Fast); })
                });

                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("rhythmRally/Tonk", beat),
                    new MultiSound.Sound("rhythmRally/Tink", beat + 0.5f),
                    new MultiSound.Sound("rhythmRally/Tonk", beat + 1f)
                });
            }
            else if (speedChange == RallySpeed.SuperFast)
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 4f, delegate { Serve(beat + 4f, RallySpeed.SuperFast); }),
                    new BeatAction.Action(beat + 6f, delegate { Serve(beat + 6f, RallySpeed.SuperFast); }),
                    new BeatAction.Action(beat + 8f, delegate { Serve(beat + 8f, RallySpeed.SuperFast); }),
                    new BeatAction.Action(beat + 10f, delegate { Serve(beat + 10f, RallySpeed.SuperFast); })
                });

                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("rhythmRally/Tonk", beat),
                    new MultiSound.Sound("rhythmRally/Tink", beat + 0.5f),
                    new MultiSound.Sound("rhythmRally/Tonk", beat + 1f),
                    new MultiSound.Sound("rhythmRally/Tink", beat + 1.5f),
                    new MultiSound.Sound("rhythmRally/Tonk", beat + 2f),
                    new MultiSound.Sound("rhythmRally/Tink", beat + 2.5f),
                    new MultiSound.Sound("rhythmRally/Tonk", beat + 3f),
                    new MultiSound.Sound("rhythmRally/Tink", beat + 3.5f)
                });
            }
        }
    }
}