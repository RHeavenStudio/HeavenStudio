using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlFlipperFlopLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("flipperFlop", "Flipper-Flop", "ffc3fc", false, false, new List<GameAction>()
            {
                new GameAction("attentionCompany", "Attention Company!")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; FlipperFlop.AttentionCompany(e.beat, e["toggle"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Mute VoiceLine", "Mute Captain Tuck's voiceline?")
                    }
                },
                new GameAction("attentionCompanyAlt", "Attention Company! (Remix 5)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; FlipperFlop.AttentionCompany(e.beat, e["toggle"], true); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Mute VoiceLine", "Mute Captain Tuck's voiceline?")
                    }
                },
                new GameAction("flipping", "Flipping")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; FlipperFlop.Flipping(e.beat, e.length, false); },
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("tripleFlip", "Triple Flip")
                {
                    function = delegate {var e = eventCaller.currentEntity; FlipperFlop.instance.TripleFlip(e.beat); },
                    defaultLength = 4f
                },
                new GameAction("flipperRollVoiceLine", "Flipper Roll Voice Line")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; FlipperFlop.FlipperRollVoiceLine(e.beat, e["amount"], e["toggle"]); },
                    parameters = new List<Param>()
                    {
                        new Param("amount", new EntityTypes.Integer(1, 10, 1), "Amount", "1, 2, 3... etc. - flipper rolls"),
                        new Param("toggle", false, "Now", "Whether Captain Tuck should say -Now!- instead of numbers.")
                    },
                    defaultLength = 2f
                },
                new GameAction("flipperRolling", "Flipper Rolling")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; FlipperFlop.Flipping(e.beat, e.length, true, e["uh"], e["thatsIt"], e["appreciation"], e["heart"], e["barber"]); },
                    parameters = new List<Param>()
                    {
                        new Param("uh", new EntityTypes.Integer(0, 3, 0), "Uh Count", "How many Uhs should Captain Tuck say after the flippers roll?"),
                        new Param("appreciation", FlipperFlop.AppreciationType.None, "Appreciation", "Which appreciation line should Captain Tuck say?", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x != (int)FlipperFlop.AppreciationType.None, new string[] { "heart" })
                        }),
                        new Param("heart", false, "Hearts", "Should Captain Tuck blush and make hearts appear when he expresses his appreciation?"),
                        new Param("thatsIt", false, "That's it!", "Whether or not Captain Tuck should say -That's it!- on the final flipper roll.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "barber" })
                        }),
                        new Param("barber", false, "Barbershop that's it variant", "Should captain tuck use the Barbershop remix variant of that's it?")
                    },
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate {var e = eventCaller.currentEntity; FlipperFlop.instance.Bop(e.beat, e.length, e["whoBops"], e["whoBopsAuto"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("whoBops", FlipperFlop.WhoBops.Both, "Who Bops?", "Who will bop?"),
                        new Param("whoBopsAuto", FlipperFlop.WhoBops.None, "Who Bops? (Auto)", "Who will auto bop?"),
                    }
                },
                new GameAction("walk", "Captain Tuck Walk")
                {
                    function = delegate { var e = eventCaller.currentEntity; FlipperFlop.instance.CaptainWalk(e.beat, e.length, e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "Which ease should the animation be played at?")
                    }
                },
                new GameAction("toggleCaptain", "Disable/Enable Captain Tuck")
                {
                    function = delegate { FlipperFlop.instance.ToggleTuck(); },
                    defaultLength = 0.5f
                }
            },
            new List<string>() { "ntr", "keep" },
            "rvlseal", "en",
            new List<string>() { "en" }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_FlipperFlop;
    public class FlipperFlop : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator captainTuckAnim;
        [SerializeField] Animator captainTuckFaceAnim;
        [SerializeField] Transform flippersMovement;
        [SerializeField] ParticleSystem leftSnow;
        [SerializeField] ParticleSystem rightSnow;
        [Header("Properties")]
        [SerializeField] float rollDistance = 1.5f;
        private bool missed;
        bool isMoving;
        bool moveLeft;
        static List<QueuedFlip> queuedInputs = new List<QueuedFlip>();
        [SerializeField] FlipperFlopFlipper flipperPlayer;
        [SerializeField] List<FlipperFlopFlipper> flippers = new List<FlipperFlopFlipper>();
        List<double> queuedMovements = new();
        static List<QueuedAttention> queuedAttentions = new List<QueuedAttention>();
        static List<double> queuedFlipperRollVoiceLines = new();
        float lastXPos;
        float currentXPos;
        float lastCameraXPos;
        float currentCameraXPos;
        bool isWalking;
        bool readyRoll;
        public GameEvent bop = new GameEvent();
        bool goBopFlip;
        bool goBopTuck;
        EasingFunction.Ease lastEase;
        double walkStartBeat;
        float walkLength;
        CaptainTuckBopType currentCaptainBop;
        public struct QueuedFlip
        {
            public double beat;
            public float length;
            public bool roll;
            public int uh;
            public bool thatsIt;
            public int appreciation;
            public bool heart;
            public bool thatsItBarberShop;
        }
        public struct QueuedAttention
        {
            public double beat;
            public bool mute;
            public bool remix5;
        }
        public enum AppreciationType
        {
            None = 0,
            Good = 1,
            GoodJob = 2,
            Nice = 3,
            WellDone = 4,
            Yes = 5,
            Random = 6
        }
        public enum WhoBops
        {
            Flippers = 0,
            CaptainTuck = 1,
            Both = 2,
            None = 3
        }
        public enum CaptainTuckBopType
        {
            Normal = 0,
            Roll = 1,
            Miss = 2,
            Success = 3
        }
        public static FlipperFlop instance;

        const int IAAltDownCat = IAMAXCAT;

        protected static bool IA_TouchPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && (instance.IsExpectingInputNow(InputAction_Nrm) || !(instance.IsExpectingInputNow(InputAction_Alt) || instance.readyRoll));
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
            return PlayerInput.GetSlide(out dt);
        }

        public static PlayerInput.InputAction InputAction_Alt =
            new("RvlSealAlt", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);
        public static PlayerInput.InputAction InputAction_Nrm =
            new("RvlSealNrm", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadBasicPress, IA_TouchPress, IA_BatonBasicPress);

        void Awake()
        {
            instance = this;
            lastXPos = flippersMovement.position.x;
            currentXPos = lastXPos;
        }

        void OnDestroy()
        {
            if (queuedInputs.Count > 0) queuedInputs.Clear();
            if (queuedAttentions.Count > 0) queuedAttentions.Clear();
            if (queuedFlipperRollVoiceLines.Count > 0) queuedFlipperRollVoiceLines.Clear();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
                {
                    if (goBopFlip) SingleBop((int)WhoBops.Flippers);
                    if (goBopTuck) SingleBop((int)WhoBops.CaptainTuck);
                }
                if (isWalking)
                {
                    float normalizedBeat = cond.GetPositionFromBeat(walkStartBeat, walkLength);
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEase);
                    float animPos = func(0f, 1f, normalizedBeat);
                    captainTuckAnim.DoNormalizedAnimation("CaptainTuckWalkIntro", animPos);
                    if (normalizedBeat >= 1f)
                    {
                        isWalking = false;
                    }
                }
                if ((PlayerInput.GetIsAction(InputAction_Nrm) && !IsExpectingInputNow(InputAction_Nrm)) || (PlayerInput.GetIsAction(InputAction_Alt) && !IsExpectingInputNow(InputAction_Alt)))
                {
                    flipperPlayer.Flip(false, false, false, true);
                }
                if (queuedInputs.Count > 0)
                {
                    foreach (var input in queuedInputs)
                    {
                        QueueFlips(input.beat, input.length, input.roll, input.uh, input.thatsIt, input.appreciation, input.heart, input.thatsItBarberShop);
                    }
                    queuedInputs.Clear();
                }
                if (queuedFlipperRollVoiceLines.Count > 0)
                {
                    foreach (var voiceLine in queuedFlipperRollVoiceLines)
                    {
                        FlipperRollVoiceLineAnimation(voiceLine);
                    }
                    queuedFlipperRollVoiceLines.Clear();
                }
                if (queuedAttentions.Count > 0)
                {
                    foreach (var attention in queuedAttentions)
                    {
                        AttentionCompanyAnimation(attention.beat, attention.mute, attention.remix5);
                    }
                    queuedAttentions.Clear();
                }
                if (queuedMovements.Count > 0)
                {
                    if (cond.songPositionInBeatsAsDouble >= queuedMovements[0])
                    {
                        if (!isMoving)
                        {
                            currentXPos = flippersMovement.position.x + (moveLeft ? -rollDistance : rollDistance);
                            isMoving = true;
                            currentCameraXPos = GameCamera.AdditionalPosition.x + (moveLeft ? -rollDistance : rollDistance);
                            if (moveLeft)
                            {
                                rightSnow.Play();
                            }
                            else
                            {
                                leftSnow.Play();
                            }
                        }
                        float normalizedBeat = cond.GetPositionFromBeat(queuedMovements[0], 0.5f);
                        float normalizedCamBeat = cond.GetPositionFromBeat(queuedMovements[0], 1f);
                        if (normalizedCamBeat > 1f)
                        {
                            queuedMovements.RemoveAt(0);
                            isMoving = false;
                            lastXPos = currentXPos;
                            lastCameraXPos = currentCameraXPos;
                        }
                        else
                        {
                            EasingFunction.Function funcCam = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutQuad);
                            float newCameraPosX = funcCam(lastCameraXPos, currentCameraXPos, normalizedCamBeat);
                            GameCamera.AdditionalPosition = new Vector3(newCameraPosX, 0, 0);
                        }
                        if (1f >= normalizedBeat)
                        {
                            EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutQuad);
                            float newPosX = func(lastXPos, currentXPos, normalizedBeat);
                            flippersMovement.position = new Vector3(newPosX, flippersMovement.position.y, flippersMovement.position.z);
                        }
                    }
                }
            }
            else if (!cond.isPlaying)
            {
                queuedInputs.Clear();
                queuedAttentions.Clear();
                queuedFlipperRollVoiceLines.Clear();
            }
        }

        public void ToggleTuck()
        {
            captainTuckAnim.gameObject.SetActive(!captainTuckAnim.gameObject.activeSelf);
        }

        public void BumpIntoOtherSeal(bool toTheLeft)
        {
            if (toTheLeft)
            {
                flippers[1].Impact(true);
            }
            else
            {
                flippers[2].Impact(false);
            }
        }

        public void Bop(double beat, float length, int whoBops, int whoBopsAuto)
        {
            goBopFlip = whoBopsAuto == (int)WhoBops.Flippers || whoBopsAuto == (int)WhoBops.Both;
            goBopTuck = whoBopsAuto == (int)WhoBops.CaptainTuck || whoBopsAuto == (int)WhoBops.Both;
            for (int i = 0; i < length; i++)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + i, delegate
                    {
                        SingleBop(whoBops);
                    })
                });
            }
        }

        void SingleBop(int whoBops)
        {
            switch (whoBops)
            {
                case (int)WhoBops.Flippers:
                    foreach (var flipper in flippers)
                    {
                        flipper.Bop();
                    }
                    flipperPlayer.Bop();
                    break;
                case (int)WhoBops.CaptainTuck:
                    CaptainTuckBop();
                    break;
                case (int)WhoBops.Both:
                    foreach (var flipper in flippers)
                    {
                        flipper.Bop();
                    }
                    flipperPlayer.Bop();
                    CaptainTuckBop();
                    break;
                default:
                    break;
            }
        }

        void CaptainTuckBop()
        {
            switch (currentCaptainBop)
            {
                case CaptainTuckBopType.Normal:
                    captainTuckAnim.DoScaledAnimationAsync("CaptainBop", 0.5f);

                    break;
                case CaptainTuckBopType.Roll:
                    captainTuckAnim.DoScaledAnimationAsync("CaptainRoll", 0.5f);
                    break;
                case CaptainTuckBopType.Miss:
                    captainTuckAnim.DoScaledAnimationAsync("CaptainTuckMissBop", 0.5f);
                    break;
                case CaptainTuckBopType.Success:
                    captainTuckAnim.DoScaledAnimationAsync("CaptainSucessBop", 0.5f);
                    break;
            }
        }

        public void CaptainWalk(double beat, float length, int ease)
        {
            captainTuckAnim.gameObject.SetActive(true);
            isWalking = true;
            lastEase = (EasingFunction.Ease)ease;
            walkStartBeat = beat;
            walkLength = length;
        }

        public static void Flipping(double beat, float length, bool roll, int uh = 0, bool thatsIt = false, int appreciation = 0, bool heart = false, bool thatsItBarberShop = false)
        {
            if (GameManager.instance.currentGame == "flipperFlop")
            {
                FlipperFlop.instance.QueueFlips(beat, length, roll, uh, thatsIt, appreciation, heart, thatsItBarberShop);
            }
            else
            {
                queuedInputs.Add(new QueuedFlip { beat = beat, length = length, roll = roll, uh = uh, thatsIt = thatsIt, appreciation = appreciation, heart = heart, thatsItBarberShop = thatsItBarberShop });
            }
        }

        public void QueueFlips(double beat, float length, bool roll, int uh = 0, bool thatsIt = false, int appreciation = 0, bool heart = false, bool thatsItBarberShop = false)
        {
            int flopCount = 1;
            int recounts = 0;
            for (int i = 0; i < length; i++)
            {
                if (roll)
                {
                    ScheduleInput(beat - 1, 1 + i, InputAction_Alt, JustFlipperRoll, MissFlipperRoll, Nothing);
                    queuedMovements.Add(beat + i);
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i - NgEarlyTime() - 1.5, delegate { readyRoll = true; }),
                        new BeatAction.Action(beat + i - 0.5, delegate { moveLeft = flippers[0].left; readyRoll = true; }),
                    });
                    foreach (var flipper in flippers)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + i, delegate { flipper.Flip(roll, true);})
                        });
                    }

                    string soundToPlay = $"flipperFlop/count/flopCount{flopCount}";

                    if (recounts == 1)
                    {
                        soundToPlay = $"flipperFlop/count/flopCount{flopCount}B";
                    }
                    else if (recounts > 1)
                    {
                        if (flopCount < 3)
                        {
                            soundToPlay = $"flipperFlop/count/flopCount{flopCount}C";
                        }
                        else
                        {
                            soundToPlay = $"flipperFlop/count/flopCount{flopCount}B";
                        }
                    }



                    if (thatsIt && i + 1 == length)
                    {
                        int noiseToPlay = (flopCount == 4) ? 2 : flopCount;
                        soundToPlay = $"flipperFlop/count/flopNoise{noiseToPlay}";
                        if (thatsItBarberShop)
                        {
                            BeatAction.New(instance, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(beat + i, delegate
                                {
                                    SoundByte.PlayOneShotGame("flipperFlop/appreciation/thatsit1");
                                    SoundByte.PlayOneShotGame(soundToPlay);
                                    captainTuckAnim.DoScaledAnimationAsync("CaptainBop", 0.5f);
                                    captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f);
                                }),
                                new BeatAction.Action(beat + i + 0.5f, delegate { SoundByte.PlayOneShotGame("flipperFlop/appreciation/thatsit2"); captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                            });
                        }
                        else
                        {
                            BeatAction.New(instance, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(beat + i - 0.5f, delegate
                                {
                                    SoundByte.PlayOneShotGame("flipperFlop/appreciation/thatsit1");
                                    captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f);
                                }),
                                new BeatAction.Action(beat + i, delegate
                                {
                                    SoundByte.PlayOneShotGame("flipperFlop/appreciation/thatsit2");
                                    captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f);
                                    SoundByte.PlayOneShotGame(soundToPlay);
                                    captainTuckAnim.DoScaledAnimationAsync("CaptainBop", 0.5f);
                                }),
                            });
                        }
                    }
                    else
                    {
                        string failingSoundToPlay = $"flipperFlop/count/flopCountFail{flopCount}";
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + i, delegate {
                                string voiceLine = soundToPlay;
                                string failVoiceLine = failingSoundToPlay;
                                if (missed)
                                {
                                    voiceLine = failVoiceLine;
                                    currentCaptainBop = CaptainTuckBopType.Miss;
                                    captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckMissExpression", 0.5f);
                                }
                                else
                                {
                                    currentCaptainBop = CaptainTuckBopType.Roll;
                                    captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckRollExpression", 0.5f);
                                }
                                CaptainTuckBop();

                                SoundByte.PlayOneShotGame(voiceLine);
                            }),
                        });
                    }

                    if (appreciation != (int)AppreciationType.None && uh == 0 && i + 1 == length)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + i + 1f, delegate
                            {
                                AppreciationVoiceLine(beat + i, appreciation, heart);
                                if (!missed && appreciation != (int)AppreciationType.None)
                                {
                                    currentCaptainBop = CaptainTuckBopType.Success;
                                }
                                else
                                {
                                    currentCaptainBop = CaptainTuckBopType.Normal;
                                    captainTuckFaceAnim.Play("CaptainTuckNeutralExpression", 0, 0);
                                }
                                CaptainTuckBop();
                                missed = false;
                            }),
                            new BeatAction.Action(beat + i + 2f, delegate
                            {
                                missed = false;
                            }),
                            new BeatAction.Action(beat + i + 3.1f, delegate
                            {
                                currentCaptainBop = CaptainTuckBopType.Normal;
                                captainTuckFaceAnim.Play("CaptainTuckNeutralExpression", 0, 0);
                            }),
                        });
                    }
                    if (appreciation == (int)AppreciationType.None && uh == 0 && i + 1 == length)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + length - 0.1f, delegate { missed = false; currentCaptainBop = CaptainTuckBopType.Normal; captainTuckFaceAnim.Play("CaptainTuckNeutralExpression", 0, 0); })
                        });
                    }


                    if (i + 1 < length)
                    {
                        flopCount++;
                    }
                    if (flopCount > 4)
                    {
                        flopCount = 1;
                        recounts++;
                    }
                }
                else
                {
                    ScheduleInput(beat - 1, 1 + i, InputAction_Nrm, JustFlip, MissFlip, Nothing);
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i - NgEarlyTime() - 1, delegate { readyRoll = false; }),
                    });
                    foreach (var flipper in flippers)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + i, delegate { flipper.Flip(roll, true); CaptainTuckBop(); })
                        });
                    }
                }
            }
            if (roll)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length + NgLateTime() - 1, delegate { readyRoll = false; }),
                });
            }
            if (uh > 0 && flopCount != 4)
            {
                for (int i = 0; i < uh; i++)
                {
                    int voiceLineIndex = i + 1;
                    string voiceLine = $"flipperFlop/uh{voiceLineIndex}";
                    string failVoiceLine = $"flipperFlop/uhfail{voiceLineIndex}";

                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + length + i, delegate {
                            string voiceLineToPlay = voiceLine;
                            string failVoiceLineToPlay = failVoiceLine;
                            if (missed)
                            {
                                voiceLineToPlay = failVoiceLineToPlay;
                                currentCaptainBop = CaptainTuckBopType.Miss;
                                captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckMissSpeakExpression", 0.5f);
                            }
                            else
                            {
                                currentCaptainBop = CaptainTuckBopType.Roll;
                                captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckRollExpression", 0.5f);
                            }

                            CaptainTuckBop();
                            SoundByte.PlayOneShotGame(voiceLineToPlay);
                        }),
                    });
                }
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length + uh, delegate
                    {
                        AppreciationVoiceLine(beat + length + uh, appreciation, heart);
                        if (!missed && appreciation != (int)AppreciationType.None)
                        {
                            currentCaptainBop = CaptainTuckBopType.Success;
                        }
                        else
                        {
                            currentCaptainBop = CaptainTuckBopType.Normal;
                            captainTuckFaceAnim.Play("CaptainTuckNeutralExpression", 0, 0);
                        }
                        CaptainTuckBop();
                        missed = false;
                    }),
                    new BeatAction.Action(beat + length + uh + 1f, delegate
                    {
                        missed = false;
                    }),
                    new BeatAction.Action(beat + length + uh + 2.1f, delegate
                    {
                        currentCaptainBop = CaptainTuckBopType.Normal;
                        captainTuckFaceAnim.Play("CaptainTuckNeutralExpression", 0, 0);
                    }),
                });
            }
            else if (uh > 0 && flopCount == 4)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length, delegate
                    {
                        AppreciationVoiceLine(beat + length, appreciation, heart);
                        if (!missed && appreciation != (int)AppreciationType.None)
                        {
                            currentCaptainBop = CaptainTuckBopType.Success;
                        }
                        else
                        {
                            currentCaptainBop = CaptainTuckBopType.Normal;
                            captainTuckFaceAnim.Play("CaptainTuckNeutralExpression", 0, 0);
                        }
                        CaptainTuckBop();
                        missed = false;
                    }),
                    new BeatAction.Action(beat + length + 1f, delegate
                    {
                        missed = false;
                    }),
                    new BeatAction.Action(beat + length + 2.1f, delegate
                    {
                        currentCaptainBop = CaptainTuckBopType.Normal;
                        captainTuckFaceAnim.Play("CaptainTuckNeutralExpression", 0, 0);
                    }),
                });
            }
        }

        public static void AppreciationVoiceLine(double beat, int appreciation, bool heart)
        {
            if (FlipperFlop.instance.missed) return;
            if (appreciation == (int)AppreciationType.Random) appreciation = UnityEngine.Random.Range(1, 6);
            switch (appreciation)
            {
                case (int)AppreciationType.None:
                    break;
                case (int)AppreciationType.Good:
                    SoundByte.PlayOneShotGame("flipperFlop/appreciation/good");
                    if (heart)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckBlushExpression", 0.5f); })
                        });
                    }
                    else
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); })
                        });
                    }
                    break;
                case (int)AppreciationType.GoodJob:
                    SoundByte.PlayOneShotGame("flipperFlop/appreciation/goodjob");
                    if (heart)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckBlushExpression", 0.5f); })
                        });
                    }
                    else
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 1f); }),
                            new BeatAction.Action(beat + 0.5f, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); })
                        });
                    }
                    break;
                case (int)AppreciationType.Nice:
                    SoundByte.PlayOneShotGame("flipperFlop/appreciation/nice");
                    if (heart)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckBlushExpression", 0.5f); })
                        });
                    }
                    else
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); })
                        });
                    }
                    break;
                case (int)AppreciationType.WellDone:
                    SoundByte.PlayOneShotGame("flipperFlop/appreciation/welldone");
                    if (heart)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckBlushExpression", 0.5f); })
                        });
                    }
                    else
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 1f); }),
                            new BeatAction.Action(beat + 0.5f, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); })
                        });
                    }
                    break;
                case (int)AppreciationType.Yes:
                    SoundByte.PlayOneShotGame("flipperFlop/appreciation/yes");
                    if (heart)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckBlushExpression", 0.5f); })
                        });
                    }
                    else
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); })
                        });
                    }
                    break;
                case (int)AppreciationType.Random:
                    break;
            }
        }

        public void TripleFlip(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("flipperFlop/ding", beat),
                new MultiSound.Sound("flipperFlop/ding", beat + 0.5f),
                new MultiSound.Sound("flipperFlop/ding", beat + 1f),
            }, forcePlay: true);

            ScheduleInput(beat, 2, InputAction_Nrm, JustFlip, MissFlip, Nothing);
            ScheduleInput(beat, 2.5f, InputAction_Nrm, JustFlip, MissFlip, Nothing);
            ScheduleInput(beat, 3, InputAction_Nrm, JustFlip, MissFlip, Nothing);
            foreach (var flipper in flippers)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2, delegate { flipper.Flip(false, true);}),
                    new BeatAction.Action(beat + 2.5f, delegate { flipper.Flip(false, true);}),
                    new BeatAction.Action(beat + 3, delegate { flipper.Flip(false, true);})
                });
            }
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 3 - NgEarlyTime(), delegate {readyRoll = false; }),
                new BeatAction.Action(beat + 2, delegate {captainTuckAnim.DoScaledAnimationAsync("CaptainBop", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate {captainTuckAnim.DoScaledAnimationAsync("CaptainBop", 0.5f); }),
            });
        }

        public static void AttentionCompany(double beat, bool mute, bool remix5 = false)
        {
            if (mute)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("flipperFlop/attention/attention7", beat + (remix5 ? 2f : 3f)),
                }, forcePlay: true);
            }
            else
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("flipperFlop/attention/attention1", beat - 0.25f),
                    new MultiSound.Sound("flipperFlop/attention/attention2", beat, 1, 1, false, 0.025f),
                    new MultiSound.Sound("flipperFlop/attention/attention3", beat + 0.5f, 1, 1, false, 0.055f),
                    new MultiSound.Sound("flipperFlop/attention/attention4", beat + 2f, 1, 1, false, 0.06f),
                    new MultiSound.Sound("flipperFlop/attention/attention5", beat + 2.25f),
                    new MultiSound.Sound("flipperFlop/attention/attention6", beat + 2.5f),
                    new MultiSound.Sound("flipperFlop/attention/attention7", beat + (remix5 ? 2f : 3f), 1, 1, false, 0.025f),
                }, forcePlay: true);
            }
            if (GameManager.instance.currentGame == "flipperFlop")
            {
                instance.AttentionCompanyAnimation(beat, mute, remix5);
            }
            else
            {
                queuedAttentions.Add(new QueuedAttention { beat = beat, mute = mute, remix5 = remix5 });
            }
        }

        public void AttentionCompanyAnimation(double beat, bool mute, bool remix5)
        {
            List<BeatAction.Action> speaks = new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.25f, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                new BeatAction.Action(beat, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                new BeatAction.Action(beat + 0.5f, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.0625f); }),
                new BeatAction.Action(beat + 2f, delegate
                {
                    captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f);
                    if (remix5)
                    {
                        foreach (var flipper in flippers)
                        {
                            flipper.PrepareFlip();
                        }
                        flipperPlayer.PrepareFlip();
                    }
                }),
                new BeatAction.Action(beat + 2.25f, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                new BeatAction.Action(beat + 2.5f, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                new BeatAction.Action(beat + 3f, delegate
                {
                    if (remix5) return;
                    foreach (var flipper in flippers)
                    {
                        flipper.PrepareFlip();
                    }
                    flipperPlayer.PrepareFlip();
                }),
            };
            if (mute)
            {
                speaks = new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2f, delegate
                    {
                        if (remix5)
                        {
                            foreach (var flipper in flippers)
                            {
                                flipper.PrepareFlip();
                            }
                            flipperPlayer.PrepareFlip();
                        }
                    }),
                    new BeatAction.Action(beat + 3f, delegate
                    {
                        if (remix5) return;
                        foreach (var flipper in flippers)
                        {
                            flipper.PrepareFlip();
                        }
                        flipperPlayer.PrepareFlip();
                    }),
                };
            }

            List<BeatAction.Action> speaksToRemove = new List<BeatAction.Action>();

            foreach (var speak in speaks)
            {
                if (Conductor.instance.songPositionInBeatsAsDouble > speak.beat)
                {
                    speaksToRemove.Add(speak);
                }
            }

            if (speaksToRemove.Count > 0)
            {
                foreach (var speak in speaksToRemove)
                {
                    speaks.Remove(speak);
                }
            }

            BeatAction.New(instance, speaks);
        }

        public static void FlipperRollVoiceLine(double beat, int amount, bool now)
        {
            if (now)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountNow", beat, 1, 1, false, 0.037f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f, 1, 1, false, 0.015f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountC", beat + 1f, 1, 1, false, 0.015f),
                }, forcePlay: true);
            }
            else if (amount == 1)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCount1", beat, 1, 1, false, 0.003f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f, 1, 1, false, 0.015f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountC", beat + 1f, 1, 1, false, 0.015f),
                }, forcePlay: true);
            }
            else if (amount == 2)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCount2", beat, 1, 1, false, 0.02f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f, 1, 1, false, 0.015f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountS", beat + 1f, 1, 1, false, 0.015f),
                }, forcePlay: true);
            }
            else if (amount == 3)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCount3", beat, 1, 1, false, 0.02f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f, 1, 1, false, 0.015f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountS", beat + 1f, 1, 1, false, 0.015f),
                }, forcePlay: true);
            }
            else if (amount == 4)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCount4", beat, 1, 1, false, 0.035f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f, 1, 1, false, 0.015f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountS", beat + 1f, 1, 1, false, 0.015f),
                }, forcePlay: true);
            }
            else if (amount == 5)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCount5", beat, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f, 1, 1, false, 0.015f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountS", beat + 1f, 1, 1, false, 0.015f),
                }, forcePlay: true);
            }
            else if (amount == 6)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCount6", beat, 1, 1, false, 0.06f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f, 1, 1, false, 0.015f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountS", beat + 1f, 1, 1, false, 0.015f),
                }, forcePlay: true);
            }
            else if (amount == 7)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCount7", beat, 1, 1, false, 0.03f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCount7B", beat + 0.25f, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f, 1, 1, false, 0.015f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountS", beat + 1f, 1, 1, false, 0.015f),
                }, forcePlay: true);
            }
            else if (amount == 8)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCount8", beat, 1, 1, false, 0.008f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f, 1, 1, false, 0.015f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountS", beat + 1f, 1, 1, false, 0.015f),
                }, forcePlay: true);
            }
            else if (amount == 9)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCount9", beat, 1, 1, false, 0.02f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f, 1, 1, false, 0.015f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountS", beat + 1f, 1, 1, false, 0.015f),
                }, forcePlay: true);
            }
            else if (amount == 10)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCount10", beat, 1, 1, false, 0.01f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f, 1, 1, false, 0.05f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f, 1, 1, false, 0.015f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountS", beat + 1f, 1, 1, false, 0.015f),
                }, forcePlay: true);
            }
            if (GameManager.instance.currentGame == "flipperFlop")
            {
                instance.FlipperRollVoiceLineAnimation(beat);
            }
            else
            {
                queuedFlipperRollVoiceLines.Add(beat);
            }
        }

        public void FlipperRollVoiceLineAnimation(double beat)
        {
            List<BeatAction.Action> speaks = new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                new BeatAction.Action(beat + 0.5f, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                new BeatAction.Action(beat + 0.75f, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                new BeatAction.Action(beat + 1f, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
            };

            List<BeatAction.Action> speaksToRemove = new List<BeatAction.Action>();

            foreach (var speak in speaks)
            {
                if (Conductor.instance.songPositionInBeatsAsDouble > speak.beat) speaksToRemove.Add(speak);
            }

            if (speaksToRemove.Count > 0)
            {
                foreach (var speak in speaksToRemove)
                {
                    speaks.Remove(speak);
                }
            }

            BeatAction.New(instance, speaks);
        }

        public void JustFlip(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                flipperPlayer.Flip(false, true, true);
                return;
            }
            SuccessFlip(false);
        }

        public void JustFlipperRoll(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                flipperPlayer.Flip(true, true, true);
                return;
            }
            SuccessFlip(true);
        }

        public void SuccessFlip(bool roll, bool dontSwitch = false)
        {
            flipperPlayer.Flip(roll, true, false, dontSwitch);
        }

        public void MissFlip(PlayerActionEvent caller)
        {
            flipperPlayer.Flip(false, false);
        }

        public void MissFlipperRoll(PlayerActionEvent caller)
        {
            flipperPlayer.Flip(true, false);
            missed = true;
        }

        public void Nothing(PlayerActionEvent caller) { }
    }
}
