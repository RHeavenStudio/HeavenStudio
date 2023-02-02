using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlRingsideLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("ringside", "Ringside \n<color=#eb5454>[WIP]</color>", "WUTRU3", false, false, new List<GameAction>()
            {
                new GameAction("question", "Question")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.Question(e.beat, e["alt"], e["variant"]); },
                    parameters = new List<Param>()
                    {
                        new Param("alt", false, "Alt", "Whether the alt voice line should be used or not."),
                        new Param("variant", Ringside.QuestionVariant.Random, "Variant", "Which variant of the cue do you wish to play.")
                    },
                    defaultLength = 4f
                },
                new GameAction("woahYouGoBigGuy", "Woah You Go Big Guy!")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.BigGuy(e.beat, e["variant"]); },
                    parameters = new List<Param>()
                    {
                        new Param("variant", Ringside.QuestionVariant.Random, "Variant", "Which variant of the cue do you wish to play.")
                    },
                    defaultLength = 4f
                },
                new GameAction("poseForTheFans", "Pose For The Fans!")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; Ringside.PoseForTheFans(e.beat, e["and"], e["variant"], e["keepZoomedOut"], e["newspaperBeats"]); },
                    parameters = new List<Param>()
                    {
                        new Param("and", false, "And", "Whether the And voice line should be said or not."),
                        new Param("variant", Ringside.PoseForTheFansVariant.Random, "Variant", "Which variant of the cue do you wish to play."),
                        new Param("keepZoomedOut", false, "Keep Zoomed Out", "Whether the camera should keep being zoomed out after the event has completed."),
                        new Param("newspaperBeats", new EntityTypes.Float(0, 80, 0), "Newspaper Beats", "How many beats should the newspaper stay for?")
                    },
                    defaultLength = 4f
                },
                new GameAction("toggleBop", "Toggle Bop")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.ToggleBop(e["bop"]); },
                    parameters = new List<Param>()
                    {
                        new Param("bop", false, "Bop?", "Whether the wrestler should bop or not."),
                    },
                    defaultLength = 0.5f
                },
                new GameAction("toggleSweat", "Toggle Sweat")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.ToggleSweat(e["sweat"]); },
                    parameters = new List<Param>()
                    {
                        new Param("sweat", false, "Sweat?", "Whether the wrestler should sweat or not."),
                    },
                    defaultLength = 0.5f
                },
            });
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
            public float beat;
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
        private bool shouldBop = true;
        private bool missedBigGuy;
        private bool reporterShouldHeart;
        private bool missedPose;
        private Sound kidsLaugh;
        private int currentPose;

        private int currentZoomIndex;

        private List<DynamicBeatmap.DynamicEntity> allCameraEvents = new List<DynamicBeatmap.DynamicEntity>();

        public GameEvent bop = new GameEvent();

        public static Ringside instance;

        void OnDestroy()
        {
            if (queuedPoses.Count > 0) queuedPoses.Clear();
        }

        public override void OnTimeChange()
        {
            UpdateCameraZoom();
        }

        void Awake()
        {
            instance = this;
            var camEvents = EventCaller.GetAllInGameManagerList("ringside", new string[] { "poseForTheFans" });
            List<DynamicBeatmap.DynamicEntity> tempEvents = new List<DynamicBeatmap.DynamicEntity>();
            for (int i = 0; i < camEvents.Count; i++)
            {
                if (camEvents[i].beat + camEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    tempEvents.Add(camEvents[i]);
                }
            }

            allCameraEvents = tempEvents;

            UpdateCameraZoom();
        }

        void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1) && isPlaying(wrestlerAnim, "Idle") && shouldBop)
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
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    wrestlerAnim.DoScaledAnimationAsync("YeMiss", 0.25f);
                    Jukebox.PlayOneShotGame($"ringside/confusedanswer");
                    if (isPlaying(reporterAnim, "IdleReporter")) reporterAnim.Play("IdleLate", 0, 0);
                }
                if (PlayerInput.AltPressed() && !IsExpectingInputNow(InputType.STANDARD_ALT_DOWN))
                {
                    int randomPose = UnityEngine.Random.Range(1, 4);
                    currentPose = randomPose;
                    wrestlerAnim.Play($"Pose{randomPose}", 0, 0);
                    reporterAnim.Play("FlinchReporter", 0, 0);
                    Jukebox.PlayOneShotGame($"ringside/yell{UnityEngine.Random.Range(1, 7)}Raw");
                    Jukebox.PlayOneShotGame("ringside/barely");
                    missedPose = true;
                    wrestlerTransform.localScale = new Vector3(1.1f, 1.1f, 1f);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(cond.songPositionInBeats + 0.1f, delegate { wrestlerTransform.localScale = new Vector3(1f, 1f, 1f); }),
                    });
                }
                ReporterBlink();
            }
            if (allCameraEvents.Count > 0)
            {
                if (currentZoomIndex < allCameraEvents.Count && currentZoomIndex >= 0)
                {
                    if (Conductor.instance.songPositionInBeats >= allCameraEvents[currentZoomIndex].beat)
                    {
                        UpdateCameraZoom();
                        currentZoomIndex++;
                    }
                }

                float normalizedBeat = Conductor.instance.GetPositionFromBeat(currentZoomCamBeat, 2.5f);

                if (normalizedBeat >= 0)
                {
                    if (normalizedBeat > 1)
                    {
                        GameCamera.additionalPosition = new Vector3(currentCamPos.x, currentCamPos.y, currentCamPos.z + 10);
                    }
                    else
                    {
                        EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutQuad);
                        float newPosX = func(lastCamPos.x, currentCamPos.x, normalizedBeat);
                        float newPosY = func(lastCamPos.y, currentCamPos.y, normalizedBeat);
                        float newPosZ = func(lastCamPos.z + 10, currentCamPos.z + 10, normalizedBeat);
                        GameCamera.additionalPosition = new Vector3(newPosX, newPosY, newPosZ);
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
                        if (p.newspaperBeats > 0)
                        {
                            reporterShouldHeart = true;
                        }
                        else
                        {
                            reporterShouldHeart = false;
                        }
                        if (cond.songPositionInBeats - 0.05f > p.beat)
                        {
                            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(p.beat, delegate  { wrestlerAnim.Play("PreparePoseIdle", 0, 0); }),
                            });
                        }
                        else
                        {
                            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(p.beat, delegate  {wrestlerAnim.DoScaledAnimationAsync("PreparePose", 0.25f); }),
                            });
                        }
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(p.beat, delegate {audienceAnim.DoScaledAnimationAsync("PoseAudience", 0.25f); }),
                            new BeatAction.Action(p.beat + 1, delegate  { PoseCheck(p.beat); }),
                            new BeatAction.Action(p.beat + 3.99f, delegate { wrestlerAnim.Play("Idle", 0, 0); }),
                            new BeatAction.Action(p.beat + 3.99f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
                        });
                        if (!p.keepZoomedOut)
                        {
                            if (p.newspaperBeats > 0)
                            {
                                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                                {
                                    new BeatAction.Action(p.beat + 3f, delegate
                                    {
                                        blackVoid.color = Color.black;
                                        newspaper.SetActive(true);
                                        if (UnityEngine.Random.Range(1, 3) == 1)
                                        {
                                            newspaper.GetComponent<Animator>().Play("NewspaperEnter", 0, 0);
                                        }
                                        else
                                        {
                                            newspaper.GetComponent<Animator>().Play("NewspaperEnterRight", 0, 0);
                                        }
                                        if (missedPose)
                                        {
                                            wrestlerNewspaperAnim.Play($"Miss{UnityEngine.Random.Range(1, 3)}Newspaper", 0, 0);
                                            reporterNewspaperAnim.Play("IdleReporterNewspaper", 0, 0);
                                            kidsLaugh = Jukebox.PlayOneShotGame("ringside/kidslaugh", -1, 1, 1, true);
                                        }
                                        else
                                        {
                                            wrestlerNewspaperAnim.Play($"Pose{currentPose}Newspaper", 0, 0);
                                            reporterNewspaperAnim.Play("HeartReporterNewspaper", 0, 0);
                                        }
                                    }),
                                    new BeatAction.Action(p.beat + 3f + p.newspaperBeats, delegate
                                    {
                                        blackVoid.color = new Color(1f, 1f, 1f, 0);
                                        newspaper.SetActive(false);
                                        lastCamPos = new Vector3(0, 0, -10);
                                        currentCamPos = new Vector3(0, 0, -10);
                                        Jukebox.KillLoop(kidsLaugh, 0.25f);
                                    })
                                });
                            }
                            else
                            {
                                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                                {
                                    new BeatAction.Action(p.beat + 3.99, delegate
                                    {
                                        lastCamPos = new Vector3(0, 0, -10);
                                        currentCamPos = new Vector3(0, 0, -10);
                                    })
                                });
                            }
                        }
                        else
                        {
                            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(p.beat + 2.5f, delegate
                                {
                                    lastCamPos = currentCamPos;
                                })
                            });
                            if (p.newspaperBeats > 0)
                            {
                                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                                {
                                    new BeatAction.Action(p.beat + 3f, delegate
                                    {
                                        blackVoid.color = Color.black;
                                        newspaper.SetActive(true);
                                        if (UnityEngine.Random.Range(1, 3) == 1)
                                        {
                                            newspaper.GetComponent<Animator>().Play("NewspaperEnter", 0, 0);
                                        }
                                        else
                                        {
                                            newspaper.GetComponent<Animator>().Play("NewspaperEnterRight", 0, 0);
                                        }
                                        if (missedPose)
                                        {
                                            wrestlerNewspaperAnim.Play($"Miss{UnityEngine.Random.Range(1, 3)}Newspaper", 0, 0);
                                            reporterNewspaperAnim.Play("IdleReporterNewspaper", 0, 0);
                                            kidsLaugh = Jukebox.PlayOneShotGame("ringside/kidslaugh", -1, 1, 1, true);
                                        }
                                        else
                                        {
                                            wrestlerNewspaperAnim.Play($"Pose{currentPose}Newspaper", 0, 0);
                                            reporterNewspaperAnim.Play("HeartReporterNewspaper", 0, 0);
                                        }
                                    }),
                                    new BeatAction.Action(p.beat + 3f + p.newspaperBeats, delegate
                                    {
                                        blackVoid.color = new Color(1f, 1f, 1f, 0);
                                        newspaper.SetActive(false);
                                        Jukebox.KillLoop(kidsLaugh, 0.25f);
                                    })
                                });
                            }
                        }
                    }
                    queuedPoses.Clear();
                }
            }

        }

        public void ToggleBop(bool startBopping)
        {
            shouldBop = startBopping;
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

        public void Question(float beat, bool alt, int questionVariant)
        {
            int currentQuestion = questionVariant;
            if (currentQuestion == 4) currentQuestion = UnityEngine.Random.Range(1, 4);
            reporterAnim.DoScaledAnimationAsync("WubbaLubbaDubbaThatTrue", 0.4f);
            if (alt)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"ringside/wub{currentQuestion}", beat),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-1", beat + 0.5f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-2", beat + 0.75f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-3", beat + 1f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-4", beat + 1.25f),
                }, forcePlay: true);
            }
            else
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"ringside/wubba{currentQuestion}-1", beat),
                    new MultiSound.Sound($"ringside/wubba{currentQuestion}-2", beat + 0.25f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-1", beat + 0.5f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-2", beat + 0.75f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-3", beat + 1f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-4", beat + 1.25f),
                }, forcePlay: true);
            }
            ThatTrue(beat + 1.25f, currentQuestion);
        }

        public void ThatTrue(float beat, int currentQuestion)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound($"ringside/that{currentQuestion}", beat + 0.25f),
                new MultiSound.Sound($"ringside/true{currentQuestion}", beat + 0.75f),
            }, forcePlay: true);
            ScheduleInput(beat, 1.75f, InputType.STANDARD_DOWN, JustQuestion, Miss, Nothing);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.25f, delegate { reporterAnim.DoScaledAnimationAsync("ThatTrue", 0.5f); }),
            });
        }

        public void BigGuy(float beat, int questionVariant)
        {
            int currentQuestion = questionVariant;
            if (currentQuestion == 4) currentQuestion = UnityEngine.Random.Range(1, 4);
            reporterAnim.DoScaledAnimationAsync("Woah", 0.4f);
            float youBeat = 0.65f;
            if (currentQuestion == 3) youBeat = 0.7f;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound($"ringside/woah{currentQuestion}", beat),
                new MultiSound.Sound($"ringside/you{currentQuestion}", beat + youBeat),
                new MultiSound.Sound($"ringside/go{currentQuestion}", beat + 1f),
                new MultiSound.Sound($"ringside/big{currentQuestion}", beat + 1.5f),
                new MultiSound.Sound($"ringside/guy{currentQuestion}", beat + 2f),
            }, forcePlay: true);

            ScheduleInput(beat, 2.5f, InputType.STANDARD_DOWN, JustBigGuyFirst, MissBigGuyOne, Nothing);
            ScheduleInput(beat, 3f, InputType.STANDARD_DOWN, JustBigGuySecond, MissBigGuyTwo, Nothing);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2f, delegate { reporterAnim.Play("True", 0, 0); }),
            });
        }

        public static void PoseForTheFans(float beat, bool and, int variant, bool keepZoomedOut, float newspaperBeats)
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
                if (newspaperBeats > 0)
                {
                    Ringside.instance.reporterShouldHeart = true;
                }
                else
                {
                    Ringside.instance.reporterShouldHeart = false;
                }
                Ringside.instance.PoseCheck(beat);
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { Ringside.instance.audienceAnim.DoScaledAnimationAsync("PoseAudience", 0.25f); }),
                    new BeatAction.Action(beat, delegate { Ringside.instance.wrestlerAnim.DoScaledAnimationAsync("PreparePose", 0.25f); }),
                    new BeatAction.Action(beat + 3.99f, delegate { Ringside.instance.wrestlerAnim.Play("Idle", 0, 0); }),
                    new BeatAction.Action(beat + 3.99f, delegate { Ringside.instance.reporterAnim.Play("IdleReporter", 0, 0); }),
                });
                if (!keepZoomedOut)
                {
                    if (newspaperBeats > 0)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + 3f, delegate
                            {
                                Ringside.instance.blackVoid.color = Color.black;
                                Ringside.instance.newspaper.SetActive(true);
                                if (UnityEngine.Random.Range(1, 3) == 1)
                                {
                                    Ringside.instance.newspaper.GetComponent<Animator>().Play("NewspaperEnter", 0, 0);
                                }
                                else
                                {
                                    Ringside.instance.newspaper.GetComponent<Animator>().Play("NewspaperEnterRight", 0, 0);
                                }
                                if (Ringside.instance.missedPose)
                                {
                                    Ringside.instance.wrestlerNewspaperAnim.Play($"Miss{UnityEngine.Random.Range(1, 3)}Newspaper", 0, 0);
                                    Ringside.instance.reporterNewspaperAnim.Play("IdleReporterNewspaper", 0, 0);
                                    Ringside.instance.kidsLaugh = Jukebox.PlayOneShotGame("ringside/kidslaugh", -1, 1, 1, true);
                                }
                                else
                                {
                                    Ringside.instance.wrestlerNewspaperAnim.Play($"Pose{Ringside.instance.currentPose}Newspaper", 0, 0);
                                    Ringside.instance.reporterNewspaperAnim.Play("HeartReporterNewspaper", 0, 0);
                                }
                            }),
                            new BeatAction.Action(beat + 3f + newspaperBeats, delegate
                            {
                                Jukebox.KillLoop(Ringside.instance.kidsLaugh, 0.25f);
                                Ringside.instance.blackVoid.color = new Color(1f, 1f, 1f, 0);
                                Ringside.instance.newspaper.SetActive(false);
                                Ringside.instance.lastCamPos = new Vector3(0, 0, -10);
                                Ringside.instance.currentCamPos = new Vector3(0, 0, -10);
                            })
                        });
                    }
                    else
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + 3.99, delegate
                            {
                                Ringside.instance.lastCamPos = new Vector3(0, 0, -10);
                                Ringside.instance.currentCamPos = new Vector3(0, 0, -10);
                            })
                        });
                    }
                }
                else
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 2.5f, delegate
                        {
                            Ringside.instance.lastCamPos = Ringside.instance.currentCamPos;
                        })
                    });
                    if (newspaperBeats > 0)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + 3f, delegate
                            {
                                Ringside.instance.blackVoid.color = Color.black;
                                Ringside.instance.newspaper.SetActive(true);
                                if (UnityEngine.Random.Range(1, 3) == 1)
                                {
                                    Ringside.instance.newspaper.GetComponent<Animator>().Play("NewspaperEnter", 0, 0);
                                }
                                else
                                {
                                    Ringside.instance.newspaper.GetComponent<Animator>().Play("NewspaperEnterRight", 0, 0);
                                }
                                if (Ringside.instance.missedPose)
                                {
                                    Ringside.instance.wrestlerNewspaperAnim.Play($"Miss{UnityEngine.Random.Range(1, 3)}Newspaper", 0, 0);
                                    Ringside.instance.reporterNewspaperAnim.Play("IdleReporterNewspaper", 0, 0);
                                    Ringside.instance.kidsLaugh = Jukebox.PlayOneShotGame("ringside/kidslaugh", -1, 1, 1, true);
                                }
                                else
                                {
                                    Ringside.instance.wrestlerNewspaperAnim.Play($"Pose{Ringside.instance.currentPose}Newspaper", 0, 0);
                                    Ringside.instance.reporterNewspaperAnim.Play("HeartReporterNewspaper", 0, 0);
                                }
                            }),
                            new BeatAction.Action(beat + 3f + newspaperBeats, delegate
                            {
                                Ringside.instance.blackVoid.color = new Color(1f, 1f, 1f, 0);
                                Ringside.instance.newspaper.SetActive(false);
                                Jukebox.KillLoop(Ringside.instance.kidsLaugh, 0.25f);
                            })
                        });
                    }
                }
            }
            else
            {
                queuedPoses.Add(new QueuedPose { beat = beat, keepZoomedOut = keepZoomedOut, newspaperBeats = newspaperBeats});
            }
        }

        private void UpdateCameraZoom()
        {

            if (currentZoomIndex < allCameraEvents.Count && currentZoomIndex >= 0)
            {
                currentZoomCamBeat = allCameraEvents[currentZoomIndex].beat;
                currentCamPos = new Vector3(poseFlash.transform.position.x, poseFlash.transform.position.y, -21.5f);
            }
        }

        public void PoseCheck(float beat)
        {
            ScheduleInput(beat, 2f, InputType.STANDARD_ALT_DOWN, JustPoseForTheFans, MissPose, Nothing);
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
            int randomNumber = UnityEngine.Random.Range(1, 200);
            if (randomNumber == 1)
            {
                if (isPlaying(reporterAnim, "IdleReporter"))
                {
                    reporterAnim.DoScaledAnimationAsync("BlinkReporter", 0.5f);
                }
            }
        }

        public void JustQuestion(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                wrestlerAnim.DoScaledAnimationAsync("Cough", 0.5f);
                Jukebox.PlayOneShotGame($"ringside/cough");
                reporterAnim.Play("ExtendMiss", 0, 0);
                Jukebox.PlayOneShotGame($"ringside/huhaudience{UnityEngine.Random.Range(0, 2)}");
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("IdleMiss", 0, 0); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
                });
                return;
            }
            SuccessQuestion(caller);
        }

        public void SuccessQuestion(PlayerActionEvent caller)
        {
            wrestlerAnim.DoScaledAnimationAsync("Ye", 0.5f);
            reporterAnim.Play("ExtendSmile", 0, 0);
            Jukebox.PlayOneShotGame($"ringside/ye{UnityEngine.Random.Range(1, 4)}");
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { Jukebox.PlayOneShotGame("ringside/yeCamera"); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { FadeFlashColor(Color.white, new Color(1, 1, 1, 0), 0.5f); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { flashObject.SetActive(true); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("SmileReporter", 0, 0); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.6f, delegate { flashObject.SetActive(false); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
            });
        }

        public void JustBigGuyFirst(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                missedBigGuy = true;
                Jukebox.PlayOneShotGame($"ringside/muscles1");
                wrestlerAnim.DoScaledAnimationAsync("BigGuyOne", 0.5f);
                return;
            }
            SuccessBigGuyFirst();
        }

        public void SuccessBigGuyFirst()
        {
            missedBigGuy = false;
            Jukebox.PlayOneShotGame($"ringside/muscles1");
            wrestlerAnim.DoScaledAnimationAsync("BigGuyOne", 0.5f);
        }

        public void JustBigGuySecond(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame($"ringside/muscles2");
                wrestlerAnim.DoScaledAnimationAsync("BigGuyTwo", 0.5f);
                if (!missedBigGuy)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
                    });
                }
                else
                {
                    reporterAnim.Play("ExtendMiss", 0, 0);
                    Jukebox.PlayOneShotGame($"ringside/huhaudience{UnityEngine.Random.Range(0, 2)}");
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("IdleMiss", 0, 0); }),
                        new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
                    });
                }
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { wrestlerAnim.Play("Idle", 0, 0); }),
                });
                return;
            }
            SuccessBigGuySecond(caller);
        }

        public void SuccessBigGuySecond(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame($"ringside/muscles2");
            wrestlerAnim.DoScaledAnimationAsync("BigGuyTwo", 0.5f);
            if (!missedBigGuy)
            {
                reporterAnim.Play("ExtendSmile", 0, 0);
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { Jukebox.PlayOneShotGame("ringside/musclesCamera"); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("SmileReporter", 0, 0); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { FadeFlashColor(Color.white, new Color(1, 1, 1, 0), 0.5f); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { flashObject.SetActive(true); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.6f, delegate { flashObject.SetActive(false); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
                });
            }
            else
            {
                reporterAnim.Play("ExtendMiss", 0, 0);
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("IdleMiss", 0, 0); }),
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
                });
            }
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { wrestlerAnim.Play("Idle", 0, 0); }),
            });

        }

        public void JustPoseForTheFans(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                wrestlerTransform.localScale = new Vector3(1.2f, 1.2f, 1f);
                int randomPose = UnityEngine.Random.Range(1, 4);
                currentPose = randomPose;
                wrestlerAnim.Play($"Pose{randomPose}", 0, 0);
                Jukebox.PlayOneShotGame($"ringside/yell{UnityEngine.Random.Range(1, 7)}Raw");
                missedPose = true;
                reporterAnim.Play("IdleMiss", 0, 0);
                Jukebox.PlayOneShotGame($"ringside/huhaudience{UnityEngine.Random.Range(0, 2)}");
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 0.1f, delegate { wrestlerTransform.localScale = new Vector3(1f, 1f, 1f); }),
                });
                return;
            }
            SuccessPoseForTheFans(caller);
        }

        public void SuccessPoseForTheFans(PlayerActionEvent caller)
        {
            missedPose = false;
            wrestlerTransform.localScale = new Vector3(1.2f, 1.2f, 1f);
            int randomPose = UnityEngine.Random.Range(1, 4);
            currentPose = randomPose;
            wrestlerAnim.Play($"Pose{randomPose}", 0, 0);
            if (reporterShouldHeart)
            {
                reporterAnim.Play("HeartReporter", 0, 0);
            }
            else
            {
                reporterAnim.Play("ExcitedReporter", 0, 0);
            }
            Jukebox.PlayOneShotGame($"ringside/yell{UnityEngine.Random.Range(1, 7)}");
            FadeFlashColor(Color.white, new Color(1, 1, 1, 0), 1f);
            FadeBGColor(Color.black, defaultBGColorLight, 1f);
            flashParticles.Play();
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 0.1f, delegate { wrestlerTransform.localScale = new Vector3(1f, 1f, 1f); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { Jukebox.PlayOneShotGame("ringside/poseCamera"); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { flashParticles.Stop(); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate { poseFlash.SetActive(true); poseFlash.GetComponent<Animator>().Play("PoseFlashing", 0, 0); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 1.99f, delegate { poseFlash.SetActive(false); }),
            });
        }

        public void Miss(PlayerActionEvent caller)
        {
            reporterAnim.Play("ExtendMiss", 0, 0);
            Jukebox.PlayOneShotGame($"ringside/huhaudience{UnityEngine.Random.Range(0, 2)}");
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("IdleMiss", 0, 0); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
            });
        }

        public void MissBigGuyOne(PlayerActionEvent caller)
        {
            missedBigGuy = true;
        }

        public void MissBigGuyTwo(PlayerActionEvent caller)
        {
            reporterAnim.Play("ExtendMiss", 0, 0);
            Jukebox.PlayOneShotGame($"ringside/huhaudience{UnityEngine.Random.Range(0, 2)}");
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.startBeat + caller.timer + 0.5f, delegate { reporterAnim.Play("IdleMiss", 0, 0); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { reporterAnim.Play("IdleReporter", 0, 0); }),
                new BeatAction.Action(caller.startBeat + caller.timer + 0.9f, delegate { wrestlerAnim.Play("Idle", 0, 0); }),
            });
        }
        
        public void MissPose(PlayerActionEvent caller)
        {
            missedPose = true;
            reporterAnim.Play("IdleMiss", 0, 0);
            Jukebox.PlayOneShotGame($"ringside/huhaudience{UnityEngine.Random.Range(0, 2)}");
        }

        public void Nothing(PlayerActionEvent caller){}

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
