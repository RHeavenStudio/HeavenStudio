using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlRingsideLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("ringside", "Ringside", "6bdfe7", false, false, new List<GameAction>()
            {
                new GameAction("toggleBop", "Bop")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.ToggleBop(e.beat, e.length, e["bop2"], e["bop"]); },
                    parameters = new List<Param>()
                    {
                        new Param("bop2", true, "Bop", "Toggle if the wrestler should bop for the duration of this event."),
                        new Param("bop", false, "Bop (Auto)", "Toggle if the wrestler should automatically bop until another Bop event is reached."),
                    },
                    resizable = true,
                },
                new GameAction("question", "Question")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.Question(e.beat, e["alt"], e["variant"]); },
                    parameters = new List<Param>()
                    {
                        new Param("alt", false, "Alternate Voice Line", "Toggle if the reporter should use an alternate voice line."),
                        new Param("variant", Ringside.QuestionVariant.Random, "Variant", "Choose the variant of the cue.")
                    },
                    defaultLength = 4f
                },
                new GameAction("woahYouGoBigGuy", "Woah You Go Big Guy!")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.BigGuy(e.beat, e["variant"]); },
                    parameters = new List<Param>()
                    {
                        new Param("variant", Ringside.QuestionVariant.Random, "Variant", "Choose the variant of the cue.")
                    },
                    defaultLength = 4f
                },
                new GameAction("poseForTheFans", "Pose For The Fans!")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; Ringside.PoseForTheFans(e.beat, e["and"], e["variant"], e["keepZoomedOut"], e["newspaperBeats"]); },
                    parameters = new List<Param>()
                    {
                        new Param("and", false, "And", "Toggle if the camera crew should say \"And\" before saying the rest of the cue."),
                        new Param("variant", Ringside.PoseForTheFansVariant.Random, "Variant", "Choose the variant of the cue."),
                        new Param("keepZoomedOut", false, "Stay Zoomed Out", "Toggle if the camera should stay zoomed out after the event."),
                        new Param("newspaperBeats", new EntityTypes.Float(0, 80, 0), "Newspaper Beats", "Set how many beats the newspaper should be visible for."),
                        new Param("ease", Util.EasingFunction.Ease.EaseOutQuad, "Ease", "Set the easing of the action."),
                    },
                    defaultLength = 4f
                },
                new GameAction("toggleSweat", "Toggle Sweat")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.ToggleSweat(e["sweat"]); },
                    parameters = new List<Param>()
                    {
                        new Param("sweat", false, "Sweat", "Toggle if the wrestler should sweat."),
                    },
                    defaultLength = 0.5f
                },
                new GameAction("questionScaled", "Question (Stretchable)")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.Question(e.beat, e["alt"], e["variant"], e.length); },
                    preFunction = delegate {if (Ringside.instance == null) return; var e = eventCaller.currentEntity; Ringside.instance.PreQuestion(e.beat, e["variant"], e.length); },
                    parameters = new List<Param>()
                    {
                        new Param("alt", false, "Alternate Voice Line", "Toggle if the reporter should use an alternate voice line."),
                        new Param("variant", Ringside.QuestionVariant.Random, "Variant", "Choose the variant of the cue.")
                    },
                    defaultLength = 4f,
                    resizable = true
                },
            },
            new List<string>() {"rvl", "normal"},
            "rvlinterview", "en",
            new List<string>() {"en"}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class Ringside : Minigame
    {
        private static Color _defaultBGColorLight;
        public static Color defaultBGColorLight
        {
            get
            {
                ColorUtility.TryParseHtmlString("#5a5a5a", out _defaultBGColorLight);
                return _defaultBGColorLight;
            }
        }

        [Header("Components")]
        [SerializeField] Animator wrestlerAnim;
        [SerializeField] Animator reporterAnim;
        [SerializeField] Animator reporterHeadAnim;
        [SerializeField] Animator audienceAnim;
        [SerializeField] Animator wrestlerNewspaperAnim;
        [SerializeField] Animator reporterNewspaperAnim;
        [SerializeField] SpriteRenderer flashWhite;
        [SerializeField] SpriteRenderer blackVoid;
        [SerializeField] GameObject flashObject;
        [SerializeField] GameObject poseFlash;
        [SerializeField] GameObject newspaper;
        [SerializeField] Transform wrestlerTransform;
        [SerializeField] SpriteRenderer bg;
        [SerializeField] ParticleSystem flashParticles;
        [SerializeField] ParticleSystem sweatParticles;

        [Header("Variables")]
        public static List<QueuedPose> queuedPoses = new List<QueuedPose>();
        public struct QueuedPose
        {
            public double beat;
            public bool keepZoomedOut;
            public float newspaperBeats;
        }
        Tween flashTween;
        Tween bgTween;
        public enum QuestionVariant
        {
            First = 1,
            Second = 2,
            Third = 3,
            Random = 4,
        }
        public enum PoseForTheFansVariant
        {
            First = 1,
            Second = 2,
            Random = 3
        }
        private float currentZoomCamBeat;
        private Vector3 lastCamPos = new Vector3(0, 0, -10);
        private Vector3 currentCamPos = new Vector3(0, 0, -10);
        private bool missedBigGuy;
        private bool reporterShouldHeart;
        private bool hitPose;
        private bool shouldNotInput;
        private bool keepZoomOut;
        private bool canBop = true;
        private Sound kidsLaugh;
        private int currentPose;
        private Util.EasingFunction.Ease lastEase;
        private GameObject currentNewspaper;

        private int currentZoomIndex;

        private List<RiqEntity> allCameraEvents = new List<RiqEntity>();

        public GameEvent bop = new GameEvent();

        public static Ringside instance;

        const int IAAltDownCat = IAMAXCAT;

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

        public static PlayerInput.InputAction InputAction_Alt =
            new("RvlInterviewAlt", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);

        void OnDestroy()
        {
            if (queuedPoses.Count > 0) queuedPoses.Clear();
            SoundByte.KillLoop(kidsLaugh, 2f);
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        public override void OnTimeChange()
        {
            UpdateCameraZoom();
        }

        void Awake()
        {
            instance = this;
            SetupBopRegion("ringside", "toggleBop", "bop");
            var camEvents = EventCaller.GetAllInGameManagerList("ringside", new string[] { "poseForTheFans" });
            List<RiqEntity> tempEvents = new List<RiqEntity>();
            for (int i = 0; i < camEvents.Count; i++)
            {
                if (camEvents[i].beat + camEvents[i].beat >= Conductor.instance.songPositionInBeatsAsDouble)
                {
                    tempEvents.Add(camEvents[i]);
                }
            }

            allCameraEvents = tempEvents;

            UpdateCameraZoom();
            wrestlerAnim.Play("Idle", 0, 1);
            ReporterBlink();
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat) && canBop)
            {
                if (UnityEngine.Random.Range(1, 18) == 1)
                {
                    wrestlerAnim.DoScaledAnimationAsync("BopPec");
                }
                else
                {
                    wrestlerAnim.DoScaledAnimationAsync("Bop");
                }
            }
        }

        private bool canDoMissExpression()
        {
            return (reporterHeadAnim.IsPlayingAnimationNames(new string[] { "BlinkHead", "ExtendBlink", "ExcitedBlink", "Idle", "ExtendIdle", "Excited", "Miss", "Late" }) || reporterHeadAnim.IsAnimationNotPlaying()) && !reporterHeadAnim.GetCurrentAnimatorStateInfo(0).IsName("Flinch");
        }

        void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress) && !shouldNotInput)
                {
                    if ((PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
                        || (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && !IsExpectingInputNow(InputAction_Alt)))
                    {
                        Ringside.instance.ScoreMiss(0.5f);

                        wrestlerAnim.DoScaledAnimationAsync("YeMiss", 0.25f);
                        SoundByte.PlayOneShotGame($"ringside/confusedanswer");
                        if (canDoMissExpression()) reporterHeadAnim.DoScaledAnimationAsync("Miss", 0.5f);
                    }
                }
                if ( PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch
                    && PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress) && !shouldNotInput)
                {
                    Ringside.instance.ScoreMiss(0.5f);
                    
                    SoundByte.PlayOneShotGame($"ringside/muscles2");
                    wrestlerAnim.DoScaledAnimationAsync("BigGuyTwo", 0.5f);
                    if (canDoMissExpression()) reporterAnim.Play("FlinchReporter", 0, 0);
                    if (canDoMissExpression()) reporterHeadAnim.Play("Flinch", 0, 0);
                    SoundByte.PlayOneShotGame("ringside/barely");
                }
                if (PlayerInput.GetIsAction(InputAction_Alt) && !IsExpectingInputNow(InputAction_Alt) && !shouldNotInput)
                {
                    Ringside.instance.ScoreMiss(0.5f);
                    
                    int randomPose = UnityEngine.Random.Range(1, 7);
                    wrestlerAnim.Play($"Pose{randomPose}", 0, 0);
                    if (canDoMissExpression()) reporterAnim.Play("FlinchReporter", 0, 0);
                    if (canDoMissExpression()) reporterHeadAnim.Play("Flinch", 0, 0);
                    SoundByte.PlayOneShotGame($"ringside/yell{UnityEngine.Random.Range(1, 7)}Raw");
                    wrestlerTransform.localScale = new Vector3(1.1f, 1.1f, 1f);
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(cond.songPositionInBeatsAsDouble + 0.1f, delegate { wrestlerTransform.localScale = new Vector3(1f, 1f, 1f); }),
                    });
                }
            }
            if (allCameraEvents.Count > 0)
            {
                if (currentZoomIndex < allCameraEvents.Count && currentZoomIndex >= 0)
                {
                    if (Conductor.instance.songPositionInBeatsAsDouble >= allCameraEvents[currentZoomIndex].beat)
                    {
                        UpdateCameraZoom();
                        currentZoomIndex++;
                    }
                }

                float normalizedBeat = Conductor.instance.GetPositionFromBeat(currentZoomCamBeat, 2.5f);
                float normalizedShouldStopBeat = Conductor.instance.GetPositionFromBeat(currentZoomCamBeat, 3.99f);

                if (normalizedBeat >= 0)
                {
                    if (normalizedShouldStopBeat > 1 && !keepZoomOut)
                    {
                        GameCamera.AdditionalPosition = new Vector3(0, 0, 0);
                    }
                    else if (normalizedBeat > 1)
                    {
                        GameCamera.AdditionalPosition = new Vector3(currentCamPos.x, currentCamPos.y, currentCamPos.z + 10);
                    }
                    else
                    {
                        Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);
                        float newPosX = func(lastCamPos.x, currentCamPos.x, normalizedBeat);
                        float newPosY = func(lastCamPos.y, currentCamPos.y, normalizedBeat);
                        float newPosZ = func(lastCamPos.z + 10, currentCamPos.z + 10, normalizedBeat);
                        GameCamera.AdditionalPosition = new Vector3(newPosX, newPosY, newPosZ);
                    }
                }
            }
        }
        
        void LateUpdate()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedPoses.Count > 0)
                {
                    foreach (var p in queuedPoses)
                    {
                        QueuePose(p.beat, p.keepZoomedOut, p.newspaperBeats);
                    }
                    queuedPoses.Clear();
                }
            }

        }

        public void ToggleBop(double beat, float length, bool startBopping, bool autoBop)
        {
            if (startBopping)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            if (wrestlerAnim.IsAnimationNotPlaying())
                            {
                                if (UnityEngine.Random.Range(1, 18) == 1)
                                {
                                    wrestlerAnim.DoScaledAnimationAsync("BopPec");
                                }
                                else
                                {
                                    wrestlerAnim.DoScaledAnimationAsync("Bop");
                                }
                            }
                        })
                    });
                }
            }
        }

        public void ToggleSweat(bool shouldSweat)
        {
            if (shouldSweat)
            {
                sweatParticles.Play();
            }
            else
            {
                sweatParticles.Stop();
            }
        }

        public void Question(double beat, bool alt, int questionVariant, float length = 4f)
        {
            if (length <= 2f) return;
            int currentQuestion = questionVariant;
            if (currentQuestion == (int)QuestionVariant.Random) currentQuestion = UnityEngine.Random.Range(1, 4);
            reporterAnim.DoScaledAnimationAsync("WubbaLubbaDubbaThatTrue", 0.4f);
            reporterHeadAnim.DoScaledAnimationAsync("Wubba", 0.4f);
            List<MultiSound.Sound> qSounds = new List<MultiSound.Sound>();
            if (alt)
            {
                qSounds.Add(new MultiSound.Sound($"ringside/wub{currentQuestion}", beat));
            }
            else
            {
                qSounds.Add(new MultiSound.Sound($"ringside/wubba{currentQuestion}-1", beat));
                qSounds.Add(new MultiSound.Sound($"ringside/wubba{currentQuestion}-2", beat + 0.25f));
            }
            float extend = length - 3f;
            int totalExtend = 0;
            if (extend > 0f)
            {
                for (int i = 0; i < extend; i++)
                {
                    qSounds.Add(new MultiSound.Sound($"ringside/dubba{currentQuestion}-1", beat + i + 0.5f ));
                    qSounds.Add(new MultiSound.Sound($"ringside/dubba{currentQuestion}-2", beat + i + 0.75f ));
                    qSounds.Add(new MultiSound.Sound($"ringside/dubba{currentQuestion}-3", beat + i + 1f ));
                    qSounds.Add(new MultiSound.Sound($"ringside/dubba{currentQuestion}-4", beat + i + 1.25f ));
                    totalExtend++;
                }
            }

            MultiSound.Play(qSounds.ToArray(), forcePlay: true);
            ThatTrue(beat + totalExtend, currentQuestion);
        }

        public void PreQuestion(double beat, int questionVariant, float length = 4f)
        {
            if (GameManager.instance.currentGame != "ringside") return;
            if (instance == null) return;
            if (length <= 2f)
            {
                int currentQuestion = questionVariant;
                if (currentQuestion == (int)QuestionVariant.Random) currentQuestion = UnityEngine.Random.Range(1, 4);
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 0.5f, delegate { reporterAnim.DoScaledAnimationAsync("WubbaLubbaDubbaThatTrue", 0.4f); reporterHeadAnim.DoScaledAnimationAsync("Wubba", 0.4f); }),
                });
                ThatTrue(beat - 1, currentQuestion);
            }
        }

        public void ThatTrue(double beat, int currentQuestion)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound($"ringside/that{currentQuestion}", beat + 0.5f),
                new MultiSound.Sound($"ringside/true{currentQuestion}", beat + 1f),
            }, forcePlay: true);
            ScheduleInput(beat, 2f, InputAction_BasicPress, JustQuestion, Miss, Nothing);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.5f, delegate { reporterAnim.DoScaledAnimationAsync("ThatTrue", 0.5f); reporterHeadAnim.DoScaledAnimationAsync("IsThat", 0.5f); }),
                new BeatAction.Action(beat + 1.5f, delegate { canBop = false; }),
                new BeatAction.Action(beat + 2.5f, delegate { canBop = true; })
            });
        }

        public void BigGuy(double beat, int questionVariant)
        {
            int currentQuestion = questionVariant;
            if (currentQuestion == (int)QuestionVariant.Random) currentQuestion = UnityEngine.Random.Range(1, 4);
            reporterAnim.DoScaledAnimationAsync("Woah", 0.5f);
            reporterHeadAnim.DoScaledAnimationAsync("Woah", 0.5f);
            float youBeat = 0.65f;
            if (currentQuestion == (int)QuestionVariant.Third) youBeat = 0.7f;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound($"ringside/woah{currentQuestion}", beat),
                new MultiSound.Sound($"ringside/you{currentQuestion}", beat + youBeat),
                new MultiSound.Sound($"ringside/go{currentQuestion}", beat + 1f),
                new MultiSound.Sound($"ringside/big{currentQuestion}", beat + 1.5f),
                new MultiSound.Sound($"ringside/guy{currentQuestion}", beat + 2f),
            }, forcePlay: true);

            ScheduleInput(beat, 2.5f, InputAction_BasicPress, JustBigGuyFirst, MissBigGuyOne, Nothing);
            ScheduleInput(beat, 3f, InputAction_FlickPress, JustBigGuySecond, MissBigGuyTwo, Nothing);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2f, delegate { reporterAnim.Play("True", 0, 0); reporterHeadAnim.Play("Extend", 0, 0); }),
                new BeatAction.Action(beat + 2.25f, delegate { canBop = false; }),
                new BeatAction.Action(beat + 3.5f, delegate { canBop = true; })
            });
        }

        public static void PoseForTheFans(double beat, bool and, int variant, bool keepZoomedOut, float newspaperBeats)
        {
            if (and)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("ringside/poseAnd", beat - 0.5f),
                }, forcePlay: true);
            }
            int poseLine = variant;
            if (poseLine == 3) poseLine = UnityEngine.Random.Range(1, 3);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound($"ringside/pose{poseLine}", beat),
                new MultiSound.Sound($"ringside/for{poseLine}", beat + 0.5f),
                new MultiSound.Sound($"ringside/the{poseLine}", beat + 0.75f),
                new MultiSound.Sound($"ringside/fans{poseLine}", beat + 1f),
            }, forcePlay: true);
            if (GameManager.instance.currentGame == "ringside")
            {
                Ringside.instance.QueuePose(beat, keepZoomedOut, newspaperBeats);
            }
            else
            {
                queuedPoses.Add(new QueuedPose { beat = beat, keepZoomedOut = keepZoomedOut, newspaperBeats = newspaperBeats});
            }
        }

        public void QueuePose(double beat, bool keepZoomedOut, float newspaperBeats)
        {
            if (newspaperBeats > 0)
            {
                reporterShouldHeart = true;
            }
            else
            {
                reporterShouldHeart = false;
            }
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate 
                {
                    audienceAnim.DoScaledAnimationAsync("PoseAudience", 0.25f);
                    wrestlerAnim.DoScaledAnimationAsync("PreparePose", 0.25f);
                    canBop = false;
                }),
                new BeatAction.Action(beat + 1, delegate  { PoseCheck(beat); }),
                new BeatAction.Action(beat + 4f, delegate
                {
                    if (BeatIsInBopRegion(beat + 4f))
                    {
                        if (UnityEngine.Random.Range(1, 18) == 1)
                        {
                            wrestlerAnim.DoScaledAnimationAsync("BopPec");
                        }
                        else
                        {
                            wrestlerAnim.DoScaledAnimationAsync("Bop");
                        }
                    }
                    else wrestlerAnim.Play("Idle", 0, 1);
                    shouldNotInput = false;
                    canBop = true;
                    reporterAnim.Play("IdleReporter", 0, 0);
                    if (reporterHeadAnim.IsAnimationNotPlaying()) reporterHeadAnim.Play("Idle", 0, 0);
                }),
            });
            if (!keepZoomedOut)
            {
                if (newspaperBeats > 0)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 3f, delegate
                        {
                            keepZoomOut = true;
                            blackVoid.color = Color.black;
                            GameObject spawnedNewsPaper = Instantiate(newspaper, transform);
                            spawnedNewsPaper.SetActive(true);
                            currentNewspaper = spawnedNewsPaper;
                            wrestlerNewspaperAnim = spawnedNewsPaper.transform.GetChild(1).GetComponent<Animator>();
                            reporterNewspaperAnim = spawnedNewsPaper.transform.GetChild(2).GetComponent<Animator>();
                            if (UnityEngine.Random.Range(1, 3) == 1)
                            {
                                spawnedNewsPaper.GetComponent<Animator>().Play("NewspaperEnter", 0, 0);
                            }
                            else
                            {
                                spawnedNewsPaper.GetComponent<Animator>().Play("NewspaperEnterRight", 0, 0);
                            }
                            if (!hitPose)
                            {
                                wrestlerNewspaperAnim.Play($"Miss{UnityEngine.Random.Range(1, 7)}Newspaper", 0, 0);
                                reporterNewspaperAnim.Play("IdleReporterNewspaper", 0, 0);
                                kidsLaugh = SoundByte.PlayOneShotGame("ringside/kidslaugh", -1, 1, 1, true);
                            }
                            else
                            {
                                wrestlerNewspaperAnim.Play($"Pose{currentPose}Newspaper", 0, 0);
                                reporterNewspaperAnim.Play("HeartReporterNewspaper", 0, 0);
                                hitPose = true;
                            }
                        }),
                        new BeatAction.Action(beat + 3f + newspaperBeats, delegate
                        {
                            blackVoid.color = new Color(1f, 1f, 1f, 0);
                            Destroy(currentNewspaper); currentNewspaper = null;
                            lastCamPos = new Vector3(0, 0, -10);
                            SoundByte.KillLoop(kidsLaugh, 0.25f);
                            keepZoomOut = false;
                        })
                    });
                }
                else
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 3.99f, delegate
                        {
                            lastCamPos = new Vector3(0, 0, -10);
                            keepZoomOut = false;
                        }),
                    });

                }
            }
            else
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2.5f, delegate
                    {
                        lastCamPos = currentCamPos;
                        keepZoomOut = true;
                    })
                });
                if (newspaperBeats > 0)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 3f, delegate
                        {
                            blackVoid.color = Color.black;
                            GameObject spawnedNewsPaper = Instantiate(newspaper, transform);
                            spawnedNewsPaper.SetActive(true);
                            currentNewspaper = spawnedNewsPaper;
                            wrestlerNewspaperAnim = spawnedNewsPaper.transform.GetChild(1).GetComponent<Animator>();
                            reporterNewspaperAnim = spawnedNewsPaper.transform.GetChild(2).GetComponent<Animator>();
                            if (UnityEngine.Random.Range(1, 3) == 1)
                            {
                                spawnedNewsPaper.GetComponent<Animator>().Play("NewspaperEnter", 0, 0);
                            }
                            else
                            {
                                spawnedNewsPaper.GetComponent<Animator>().Play("NewspaperEnterRight", 0, 0);
                            }
                            if (!hitPose)
                            {
                                wrestlerNewspaperAnim.Play($"Miss{UnityEngine.Random.Range(1, 7)}Newspaper", 0, 0);
                                reporterNewspaperAnim.Play("IdleReporterNewspaper", 0, 0);
                                kidsLaugh = SoundByte.PlayOneShotGame("ringside/kidslaugh", -1, 1, 1, true);
                            }
                            else
                            {
                                wrestlerNewspaperAnim.Play($"Pose{currentPose}Newspaper", 0, 0);
                                reporterNewspaperAnim.Play("HeartReporterNewspaper", 0, 0);
                                hitPose = true;
                            }
                        }),
                        new BeatAction.Action(beat + 3f + newspaperBeats, delegate
                        {
                            blackVoid.color = new Color(1f, 1f, 1f, 0);
                            Destroy(currentNewspaper); currentNewspaper = null;
                            SoundByte.KillLoop(kidsLaugh, 0.25f);
                        })
                    });
                }
            }
        }

        private void UpdateCameraZoom()
        {
            if (currentZoomIndex < allCameraEvents.Count && currentZoomIndex >= 0)
            {
                currentZoomCamBeat = (float)allCameraEvents[currentZoomIndex].beat;
                currentCamPos = new Vector3(poseFlash.transform.position.x, poseFlash.transform.position.y, -21.5f);
                lastEase = (Util.EasingFunction.Ease)allCameraEvents[currentZoomIndex]["ease"];
            }
        }

        public void PoseCheck(double beat)
        {
            ScheduleInput(beat, 2f, InputAction_Alt, JustPoseForTheFans, MissPose, Nothing);
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

        public void ChangeBGColor(Color color, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (bgTween != null)
                bgTween.Kill(true);

            if (seconds == 0)
            {
                bg.color = color;
            }
            else
            {
                flashTween = bg.DOColor(color, seconds);
            }
        }

        public void FadeBGColor(Color start, Color end, float beats)
        {
            ChangeBGColor(start, 0f);
            ChangeBGColor(end, beats);
        }

        public void ReporterBlink()
        {
            if (reporterHeadAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                reporterHeadAnim.DoScaledAnimationAsync("BlinkHead", 0.5f);
            }
            else if (reporterHeadAnim.GetCurrentAnimatorStateInfo(0).IsName("ExtendIdle"))
            {
                reporterHeadAnim.DoScaledAnimationAsync("ExtendBlink", 0.5f);
            }
            else if (reporterHeadAnim.GetCurrentAnimatorStateInfo(0).IsName("Excited"))
            {
                reporterHeadAnim.DoScaledAnimationAsync("ExcitedBlink", 0.5f);
            }
            float randomTime = UnityEngine.Random.Range(0.3f, 1.8f);
            Invoke("ReporterBlink", randomTime);
        }

        public void JustQuestion(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                wrestlerAnim.DoScaledAnimationAsync("Cough", 0.5f);
                SoundByte.PlayOneShotGame($"ringside/cough");
                reporterHeadAnim.DoScaledAnimationAsync("Late", 0.5f);
                SoundByte.PlayOneShotGame($"ringside/huhaudience{UnityEngine.Random.Range(0, 2)}");
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { reporterAnim.Play("IdleReporter", 0, 0); if (reporterHeadAnim.IsAnimationNotPlaying()) reporterHeadAnim.Play("Idle", 0, 0); }),
                });
                return;
            }
            SuccessQuestion(caller);
        }

        public void SuccessQuestion(PlayerActionEvent caller)
        {
            wrestlerAnim.DoScaledAnimationAsync("Ye", 0.5f);
            reporterHeadAnim.Play("ExtendSmile", 0, 0);
            SoundByte.PlayOneShotGame($"ringside/ye{UnityEngine.Random.Range(1, 4)}");
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate 
                { 
                    SoundByte.PlayOneShotGame("ringside/yeCamera");
                    FadeFlashColor(Color.white, new Color(1, 1, 1, 0), 0.5f);
                    flashObject.SetActive(true);
                    reporterAnim.Play("IdleReporter", 0, 0);
                    reporterHeadAnim.Play("Smile", 0, 0);
                }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.6f, delegate { flashObject.SetActive(false); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { if (reporterHeadAnim.IsAnimationNotPlaying()) reporterHeadAnim.Play("Idle", 0, 0); }),
            });
        }

        public void JustBigGuyFirst(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                missedBigGuy = true;
                SoundByte.PlayOneShotGame($"ringside/muscles1");
                wrestlerAnim.DoScaledAnimationAsync("BigGuyOne", 0.5f);
                return;
            }
            SuccessBigGuyFirst();
        }

        public void SuccessBigGuyFirst()
        {
            missedBigGuy = false;
            SoundByte.PlayOneShotGame($"ringside/muscles1");
            wrestlerAnim.DoScaledAnimationAsync("BigGuyOne", 0.5f);
        }

        public void JustBigGuySecond(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame($"ringside/muscles2");
                wrestlerAnim.DoScaledAnimationAsync("BigGuyTwo", 0.5f);
                if (!missedBigGuy)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
                    });
                }
                else
                {
                    reporterHeadAnim.Play("Late", 0, 0);
                    SoundByte.PlayOneShotGame($"ringside/huhaudience{UnityEngine.Random.Range(0, 2)}");
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
                        new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { if (reporterHeadAnim.IsAnimationNotPlaying()) reporterHeadAnim.Play("Idle", 0, 0); }),
                    });
                }
                return;
            }
            SuccessBigGuySecond(caller);
        }

        public void SuccessBigGuySecond(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame($"ringside/muscles2");
            wrestlerAnim.DoScaledAnimationAsync("BigGuyTwo", 0.5f);
            if (!missedBigGuy)
            {
                reporterHeadAnim.Play("ExtendSmile", 0, 0);
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate 
                    { 
                        SoundByte.PlayOneShotGame("ringside/musclesCamera");
                        reporterAnim.Play("IdleReporter", 0, 0);
                        reporterHeadAnim.Play("Smile", 0, 0);
                        FadeFlashColor(Color.white, new Color(1, 1, 1, 0), 0.5f);
                        flashObject.SetActive(true);
                    }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.6f, delegate { flashObject.SetActive(false); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { if (reporterHeadAnim.IsAnimationNotPlaying()) reporterHeadAnim.Play("Idle", 0, 0); }),
                });
            }
            else
            {
                reporterHeadAnim.Play("Miss", 0, 0);
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { if (reporterHeadAnim.IsAnimationNotPlaying()) reporterHeadAnim.Play("Idle", 0, 0); }),
                });
            }
        }

        public void JustPoseForTheFans(PlayerActionEvent caller, float state)
        {
            shouldNotInput = true;
            if (state >= 1f || state <= -1f)
            {
                wrestlerTransform.localScale = new Vector3(1.2f, 1.2f, 1f);
                int randomPose = UnityEngine.Random.Range(1, 7);
                wrestlerAnim.Play($"Pose{randomPose}", 0, 0);
                SoundByte.PlayOneShotGame($"ringside/yell{UnityEngine.Random.Range(1, 7)}Raw");
                reporterHeadAnim.Play("Late", 0, 0);
                SoundByte.PlayOneShotGame($"ringside/huhaudience{UnityEngine.Random.Range(0, 2)}");
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.1f, delegate { wrestlerTransform.localScale = new Vector3(1f, 1f, 1f); }),
                });
                return;
            }
            SuccessPoseForTheFans(caller);
        }

        public void SuccessPoseForTheFans(PlayerActionEvent caller)
        {
            wrestlerTransform.localScale = new Vector3(1.2f, 1.2f, 1f);
            int randomPose = UnityEngine.Random.Range(1, 7);
            currentPose = randomPose;
            wrestlerAnim.Play($"Pose{randomPose}", 0, 0);
            if (reporterShouldHeart)
            {
                reporterAnim.Play("HeartReporter", 0, 0);
                reporterHeadAnim.Play("Heart", 0, 0);
            }
            else
            {
                reporterAnim.Play("ExcitedReporter", 0, 0);
                reporterHeadAnim.Play("Excited", 0, 0);
            }
            hitPose = true;
            SoundByte.PlayOneShotGame($"ringside/yell{UnityEngine.Random.Range(1, 7)}");
            FadeFlashColor(Color.white, new Color(1, 1, 1, 0), 1f);
            FadeBGColor(Color.black, defaultBGColorLight, 1f);
            flashParticles.Play();
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 0.1f, delegate { wrestlerTransform.localScale = new Vector3(1f, 1f, 1f); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { SoundByte.PlayOneShotGame("ringside/poseCamera"); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { flashParticles.Stop(); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { poseFlash.SetActive(true); poseFlash.GetComponent<Animator>().Play("PoseFlashing", 0, 0); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1.99f, delegate { poseFlash.SetActive(false); }),
            });
        }

        public void Miss(PlayerActionEvent caller)
        {
            reporterHeadAnim.Play("Late", 0, 0);
            SoundByte.PlayOneShotGame($"ringside/huhaudience{UnityEngine.Random.Range(0, 2)}");
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { reporterHeadAnim.Play("Idle", 0, 0); }),
            });
        }

        public void MissBigGuyOne(PlayerActionEvent caller)
        {
            missedBigGuy = true;
        }

        public void MissBigGuyTwo(PlayerActionEvent caller)
        {
            reporterHeadAnim.Play("Late", 0, 0);
            SoundByte.PlayOneShotGame($"ringside/huhaudience{UnityEngine.Random.Range(0, 2)}");
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { reporterHeadAnim.Play("Idle", 0, 0); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { wrestlerAnim.Play("Idle", 0, 0); }),
            });
        }
        
        public void MissPose(PlayerActionEvent caller)
        {
            shouldNotInput = true;
            reporterHeadAnim.Play("Late", 0, 0);
            SoundByte.PlayOneShotGame($"ringside/huhaudience{UnityEngine.Random.Range(0, 2)}");
        }

        public void Nothing(PlayerActionEvent caller){}
    }
}
