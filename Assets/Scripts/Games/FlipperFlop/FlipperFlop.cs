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
            return new Minigame("flipperFlop", "Flipper-Flop", "SEAL05", false, false, new List<GameAction>()
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
                    preFunction = delegate {var e = eventCaller.currentEntity; FlipperFlop.Flipping(e.beat, e.length, true, e["uh"], e["thatsIt"], e["appreciation"]); },
                    parameters = new List<Param>()
                    {
                        new Param("uh", false, "Uh", "Whether or not Captain Tuck should say Uh after the flipper roll is done."),
                        new Param("thatsIt", false, "That's it!", "Whether or not Captain Tuck should say -That's it!- on the final flipper roll."),
                        new Param("appreciation", FlipperFlop.AppreciationType.None, "Appreciation", "Which appreciation line should Captain Tuck say?")
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
        [Header("Properties")]
        private bool missed;
        static List<QueuedFlip> queuedInputs = new List<QueuedFlip>();
        [SerializeField] FlipperFlopFlipper flipperPlayer;
        [SerializeField] List<FlipperFlopFlipper> flippers = new List<FlipperFlopFlipper>();
        public struct QueuedFlip
        {
            public float beat;
            public float length;
            public bool roll;
            public bool uh;
            public bool thatsIt;
            public int appreciation;
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

        public static FlipperFlop instance;

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }
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
                        QueueFlips(input.beat, input.length, input.roll, input.uh, input.thatsIt, input.appreciation);
                    }
                    queuedInputs.Clear();
                }
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
                    captainTuckAnim.DoScaledAnimationAsync("CaptainBop", 0.5f);
                    break;
                case (int)WhoBops.Both:
                    foreach (var flipper in flippers)
                    {
                        flipper.Bop();
                    }
                    flipperPlayer.Bop();
                    captainTuckAnim.DoScaledAnimationAsync("CaptainBop", 0.5f);
                    break;
            }

        }

        public static void Flipping(float beat, float length, bool roll, bool uh = false, bool thatsIt = false, int appreciation = 0)
        {
            if (GameManager.instance.currentGame == "flipperFlop")
            {
                FlipperFlop.instance.QueueFlips(beat, length, roll, uh, thatsIt, appreciation);
            }
            else
            {
                queuedInputs.Add(new QueuedFlip { beat = beat, length = length, roll = roll, uh = uh, thatsIt = thatsIt, appreciation = appreciation });
            }
        }

        public void QueueFlips(float beat, float length, bool roll, bool uh = false, bool thatsIt = false, int appreciation = 0)
        {
            missed = false;
            int flopCount = 1;
            int recounts = 0;
            for (int i = 0; i < length; i++)
            {
                if (roll)
                {
                    ScheduleInput(beat - 1, 1 + i, InputType.STANDARD_ALT_DOWN, JustFlipperRoll, MissFlipperRoll, Nothing);
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
                            new BeatAction.Action(beat + i, delegate { Jukebox.PlayOneShotGame("flipperFlop/appreciation/thatsit1"); }),
                            new BeatAction.Action(beat + i, delegate { Jukebox.PlayOneShotGame(soundToPlay); }),
                            new BeatAction.Action(beat + i + 0.5f, delegate { Jukebox.PlayOneShotGame("flipperFlop/appreciation/thatsit2"); }),
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
                                }
                                else
                                {
                                    captainTuckAnim.DoScaledAnimationAsync("CaptainRoll", 0.5f);
                                }

                                Jukebox.PlayOneShotGame(voiceLine); 
                            }),
                        });
                    }

                    if (appreciation != (int)AppreciationType.None && !uh && i + 1 == length)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + i + 1f, delegate { AppreciationVoiceLine(appreciation); }),
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
                            new BeatAction.Action(beat + i, delegate { flipper.Flip(roll, true); captainTuckAnim.DoScaledAnimationAsync("CaptainBop", 0.5f); })
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
                            if (missed) voiceLineToPlay = failVoiceLineToPlay;
                            Jukebox.PlayOneShotGame(voiceLineToPlay); 
                        }),
                    });
                }
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length + 4 - flopCount, delegate { AppreciationVoiceLine(appreciation); }),
                });
            }
            else if (uh && flopCount == 4)
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length, delegate { AppreciationVoiceLine(appreciation); }),
                });
            }
        }

        public static void AppreciationVoiceLine(int appreciation)
        {
            if (FlipperFlop.instance.missed) return;
            if (appreciation == (int)AppreciationType.Random) appreciation = UnityEngine.Random.Range(1, 6);
            switch (appreciation)
            {
                case (int)AppreciationType.None:
                    break;
                case (int)AppreciationType.Good:
                    Jukebox.PlayOneShotGame("flipperFlop/appreciation/good");
                    break;
                case (int)AppreciationType.GoodJob:
                    Jukebox.PlayOneShotGame("flipperFlop/appreciation/goodjob");
                    break;
                case (int)AppreciationType.Nice:
                    Jukebox.PlayOneShotGame("flipperFlop/appreciation/nice");
                    break;
                case (int)AppreciationType.WellDone:
                    Jukebox.PlayOneShotGame("flipperFlop/appreciation/welldone");
                    break;
                case (int)AppreciationType.Yes:
                    Jukebox.PlayOneShotGame("flipperFlop/appreciation/yes");
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

        public void SuccessFlip(bool roll)
        {
            flipperPlayer.Flip(roll, true);
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
