using HeavenStudio.Util;
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
            return new Minigame("flipperFlop", "Flipper-Flop", "ffd0ff", false, false, new List<GameAction>()
            {
                new GameAction("attentionCompany", "Attention Company!")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; FlipperFlop.AttentionCompany(e.beat); },
                    defaultLength = 4f,
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
                    preFunction = delegate {var e = eventCaller.currentEntity; FlipperFlop.Flipping(e.beat, e.length, true, e["uh"], e["thatsIt"], e["appreciation"], e["heart"]); },
                    parameters = new List<Param>()
                    {
                        new Param("uh", false, "Uh", "Whether or not Captain Tuck should say Uh after the flipper roll is done."),
                        new Param("thatsIt", false, "That's it!", "Whether or not Captain Tuck should say -That's it!- on the final flipper roll."),
                        new Param("appreciation", FlipperFlop.AppreciationType.None, "Appreciation", "Which appreciation line should Captain Tuck say?"),
                        new Param("heart", false, "Hearts", "Should Captain Tuck blush and make hearts appear when he expresses his appreciation?")
                    },
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("bop", "Bop") 
                {
                    function = delegate {var e = eventCaller.currentEntity; FlipperFlop.instance.Bop(e["whoBops"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("whoBops", FlipperFlop.WhoBops.Both, "Who Bops?", "Who will bop?")
                    }
                },
            });
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
        [Header("Properties")]
        private bool missed;
        bool isMoving;
        bool moveLeft;
        static List<QueuedFlip> queuedInputs = new List<QueuedFlip>();
        [SerializeField] FlipperFlopFlipper flipperPlayer;
        [SerializeField] List<FlipperFlopFlipper> flippers = new List<FlipperFlopFlipper>();
        List<float> queuedMovements = new List<float>();
        static List<float> queuedAttentions = new List<float>();
        float lastXPos;
        float currentXPos;
        float lastCameraXPos;
        float currentCameraXPos;
        CaptainTuckBopType currentCaptainBop;
        public struct QueuedFlip
        {
            public float beat;
            public float length;
            public bool roll;
            public bool uh;
            public bool thatsIt;
            public int appreciation;
            public bool heart;
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
            Both = 2
        }
        public enum CaptainTuckBopType
        {
            Normal = 0,
            Roll = 1,
            Miss = 2,
            Success = 3
        }
        public static FlipperFlop instance;

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
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if(cond.isPlaying && !cond.isPaused)
            {
                if (queuedInputs.Count > 0)
                {
                    foreach (var input in queuedInputs) 
                    { 
                        QueueFlips(input.beat, input.length, input.roll, input.uh, input.thatsIt, input.appreciation, input.heart);
                    }
                    queuedInputs.Clear();
                }
                if (queuedAttentions.Count > 0)
                {
                    foreach (var attention in queuedAttentions)
                    {
                        AttentionCompanyAnimation(attention);
                    }
                    queuedAttentions.Clear();
                }
                if (queuedMovements.Count > 0)
                {
                    if (!isMoving) 
                    {
                        currentXPos = flippersMovement.position.x + (moveLeft ? -2f : 2f); 
                        isMoving = true; 
                        currentCameraXPos = GameCamera.additionalPosition.x + (moveLeft ? -2f : 2f);
                    } 
                    if (cond.songPositionInBeats >= queuedMovements[0])
                    {
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
                            GameCamera.additionalPosition = new Vector3(newCameraPosX, 0, 0);
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
            }
        }

        public void Bop(int whoBops)
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

        public static void Flipping(float beat, float length, bool roll, bool uh = false, bool thatsIt = false, int appreciation = 0, bool heart = false)
        {
            if (GameManager.instance.currentGame == "flipperFlop")
            {
                FlipperFlop.instance.QueueFlips(beat, length, roll, uh, thatsIt, appreciation, heart);
            }
            else
            {
                queuedInputs.Add(new QueuedFlip { beat = beat, length = length, roll = roll, uh = uh, thatsIt = thatsIt, appreciation = appreciation, heart = heart });
            }
        }

        public void QueueFlips(float beat, float length, bool roll, bool uh = false, bool thatsIt = false, int appreciation = 0, bool heart = false)
        {
            int flopCount = 1;
            int recounts = 0;
            for (int i = 0; i < length; i++)
            {
                if (roll)
                {
                    ScheduleInput(beat - 1, 1 + i, InputType.STANDARD_ALT_DOWN, JustFlipperRoll, MissFlipperRoll, Nothing);
                    queuedMovements.Add(beat + i);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i - 0.5f, delegate { moveLeft = flippers[0].left;})
                    });
                    foreach (var flipper in flippers)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
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
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + i, delegate 
                            { 
                                Jukebox.PlayOneShotGame("flipperFlop/appreciation/thatsit1");
                                Jukebox.PlayOneShotGame(soundToPlay);
                                captainTuckAnim.DoScaledAnimationAsync("CaptainTuckBop", 0.5f);
                                captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f);
                            }),
                            new BeatAction.Action(beat + i + 0.5f, delegate { Jukebox.PlayOneShotGame("flipperFlop/appreciation/thatsit2"); captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                        });
                    }
                    else
                    {
                        string failingSoundToPlay = $"flipperFlop/count/flopCountFail{flopCount}";
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
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

                                Jukebox.PlayOneShotGame(voiceLine); 
                            }),
                        });
                    }

                    if (appreciation != (int)AppreciationType.None && !uh && i + 1 == length)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + i + 1f, delegate
                            {
                                AppreciationVoiceLine(beat + i, appreciation, heart);
                                if (!missed)
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
                            new BeatAction.Action(beat + i + 1.1f, delegate
                            {
                                currentCaptainBop = CaptainTuckBopType.Normal;
                            }),
                            new BeatAction.Action(beat + i + 2f, delegate
                            {
                                captainTuckFaceAnim.Play("CaptainTuckNeutralExpression", 0, 0); missed = false;
                            }),
                        });
                    }
                    if (appreciation == (int)AppreciationType.None && !uh && i + 1 == length)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
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
                    ScheduleInput(beat - 1, 1 + i, InputType.STANDARD_DOWN, JustFlip, MissFlip, Nothing);
                    foreach (var flipper in flippers)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + i, delegate { flipper.Flip(roll, true); CaptainTuckBop(); })
                        });
                    }
                }
            }
            if (uh && flopCount != 4)
            {
                for (int i = 0; i < 4 - flopCount; i++)
                {
                    string voiceLine = $"flipperFlop/uh{flopCount + i}";
                    string failVoiceLine = $"flipperFlop/uhfail{flopCount + i}";

                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + length + i, delegate {
                            string voiceLineToPlay = voiceLine;
                            string failVoiceLineToPlay = failVoiceLine;
                            if (missed) 
                            {
                                voiceLineToPlay = failVoiceLineToPlay;
                                captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckMissSpeakExpression", 0.5f);
                                CaptainTuckBop();
                            } 
                            Jukebox.PlayOneShotGame(voiceLineToPlay); 
                        }),
                    });
                }
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length + 4 - flopCount, delegate 
                    { 
                        AppreciationVoiceLine(beat + length + 4 - flopCount, appreciation, heart); 
                        if (!missed)
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
                    new BeatAction.Action(beat + length + 4 - flopCount + 0.1f, delegate
                    {
                        currentCaptainBop = CaptainTuckBopType.Normal;
                    }),
                    new BeatAction.Action(beat + length + 4 - flopCount + 1f, delegate
                    {
                        captainTuckFaceAnim.Play("CaptainTuckNeutralExpression", 0, 0); missed = false;
                    }),
                });
            }
            else if (uh && flopCount == 4)
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length, delegate
                    {
                        AppreciationVoiceLine(beat + length, appreciation, heart);
                        if (!missed)
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
                    new BeatAction.Action(beat + length + 0.1f, delegate
                    {
                        currentCaptainBop = CaptainTuckBopType.Normal;
                    }),
                    new BeatAction.Action(beat + length + 1f, delegate
                    {
                        captainTuckFaceAnim.Play("CaptainTuckNeutralExpression", 0, 0); missed = false;
                    }),
                });
            }
        }

        public static void AppreciationVoiceLine(float beat, int appreciation, bool heart)
        {
            if (FlipperFlop.instance.missed) return;
            if (appreciation == (int)AppreciationType.Random) appreciation = UnityEngine.Random.Range(1, 6);
            switch (appreciation)
            {
                case (int)AppreciationType.None:
                    break;
                case (int)AppreciationType.Good:
                    Jukebox.PlayOneShotGame("flipperFlop/appreciation/good");
                    if (heart)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckBlushExpression", 0.5f); })
                        });
                    }
                    else
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); })
                        });
                    }
                    break;
                case (int)AppreciationType.GoodJob:
                    Jukebox.PlayOneShotGame("flipperFlop/appreciation/goodjob");
                    if (heart)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 1f); }),
                            new BeatAction.Action(beat + 0.5f, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckBlushExpression", 0.5f); })
                        });
                    }
                    else
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 1f); }),
                            new BeatAction.Action(beat + 0.5f, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); })
                        });
                    }
                    break;
                case (int)AppreciationType.Nice:
                    Jukebox.PlayOneShotGame("flipperFlop/appreciation/nice");
                    if (heart)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckBlushExpression", 0.5f); })
                        });
                    }
                    else
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); })
                        });
                    }
                    break;
                case (int)AppreciationType.WellDone:
                    Jukebox.PlayOneShotGame("flipperFlop/appreciation/welldone");
                    if (heart)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 1f); }),
                            new BeatAction.Action(beat + 0.5f, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckBlushExpression", 0.5f); })
                        });
                    }
                    else
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 1f); }),
                            new BeatAction.Action(beat + 0.5f, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); })
                        });
                    }
                    break;
                case (int)AppreciationType.Yes:
                    Jukebox.PlayOneShotGame("flipperFlop/appreciation/yes");
                    if (heart)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckBlushExpression", 0.5f); })
                        });
                    }
                    else
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { instance.captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); })
                        });
                    }
                    break;
                case (int)AppreciationType.Random:
                    break;
            }
        }

        public void TripleFlip(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("flipperFlop/ding", beat),
                new MultiSound.Sound("flipperFlop/ding", beat + 0.5f),
                new MultiSound.Sound("flipperFlop/ding", beat + 1f),
            }, forcePlay: true);

            ScheduleInput(beat, 2, InputType.STANDARD_DOWN, JustFlip, MissFlip, Nothing);
            ScheduleInput(beat, 2.5f, InputType.STANDARD_DOWN, JustFlip, MissFlip, Nothing);
            ScheduleInput(beat, 3, InputType.STANDARD_DOWN, JustFlip, MissFlip, Nothing);
            foreach (var flipper in flippers)
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2, delegate { flipper.Flip(false, true);}),
                    new BeatAction.Action(beat + 2.5f, delegate { flipper.Flip(false, true);}),
                    new BeatAction.Action(beat + 3, delegate { flipper.Flip(false, true);})
                });
            }
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2, delegate {captainTuckAnim.DoScaledAnimationAsync("CaptainBop", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate {captainTuckAnim.DoScaledAnimationAsync("CaptainBop", 0.5f); }),
            });
        }

        public static void AttentionCompany(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("flipperFlop/attention/attention1", beat - 0.25f),
                new MultiSound.Sound("flipperFlop/attention/attention2", beat),
                new MultiSound.Sound("flipperFlop/attention/attention3", beat + 0.5f),
                new MultiSound.Sound("flipperFlop/attention/attention4", beat + 2f),
                new MultiSound.Sound("flipperFlop/attention/attention5", beat + 2.25f),
                new MultiSound.Sound("flipperFlop/attention/attention6", beat + 2.5f),
                new MultiSound.Sound("flipperFlop/attention/attention7", beat + 3),
            }, forcePlay: true);
            if (GameManager.instance.currentGame == "flipperFlop")
            {
                instance.AttentionCompanyAnimation(beat);
            }
            else
            {
                queuedAttentions.Add(beat);
            }
        }

        public void AttentionCompanyAnimation(float beat)
        {
            List<BeatAction.Action> speaks = new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.25f, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                new BeatAction.Action(beat, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                new BeatAction.Action(beat + 0.5f, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.0625f); }),
                new BeatAction.Action(beat + 2f, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                new BeatAction.Action(beat + 2.25f, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                new BeatAction.Action(beat + 2.5f, delegate { captainTuckFaceAnim.DoScaledAnimationAsync("CaptainTuckSpeakExpression", 0.5f); }),
                new BeatAction.Action(beat + 3f, delegate 
                { 
                    foreach (var flipper in flippers)
                    {
                        flipper.PrepareFlip();
                    }
                    flipperPlayer.PrepareFlip();
                }),
            };

            List<BeatAction.Action> speaksToRemove = new List<BeatAction.Action>();

            foreach (var speak in speaks)
            {
                if (Conductor.instance.songPositionInBeats > speak.beat)
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

            BeatAction.New(instance.gameObject, speaks);
        }

        public static void FlipperRollVoiceLine(float beat, int amount, bool now)
        {
            if (now)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountNow", beat),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountC", beat + 1f),
                }, forcePlay: true);
            }
            else
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCount{amount}", beat),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountA", beat + 0.5f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountB", beat + 0.75f),
                    new MultiSound.Sound($"flipperFlop/count/flipperRollCountC", beat + 1f),
                }, forcePlay: true);
            }
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

        public void Nothing(PlayerActionEvent caller) {}
    }
}
