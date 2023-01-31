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
            return new Minigame("flipperFlop", "Flipper-Flop \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
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
                    preFunction = delegate {var e = eventCaller.currentEntity; FlipperFlop.Flipping(e.beat, e.length, true, e["uh"], e["thatsIt"]); },
                    parameters = new List<Param>()
                    {
                        new Param("uh", false, "Uh", "Whether or not Captain Tuck should say Uh after the flipper roll is done."),
                        new Param("thatsIt", false, "That's it!", "Whether or not Captain Tuck should say -That's it!- on the final flipper roll."),
                    },
                    defaultLength = 4f,
                    resizable = true
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
        [Header("Properties")]
        static List<QueuedFlip> queuedInputs = new List<QueuedFlip>();
        [SerializeField] List<FlipperFlopFlipper> flippers = new List<FlipperFlopFlipper>();
        public struct QueuedFlip
        {
            public float startBeat;
            public float beat;
            public bool roll;
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

        public static void Flipping(float beat, float length, bool roll, bool uh = false, bool thatsIt = false)
        {
            if (GameManager.instance.currentGame == "flipperFlop")
            {
                int flopCount = 1;
                int recounts = 0;
                for (int i = 0; i < length; i++)
                {
                    if (roll)
                    {
                        FlipperFlop.instance.ScheduleInput(beat - 1, 1 + i, InputType.STANDARD_ALT_DOWN, FlipperFlop.instance.JustFlipperRoll, FlipperFlop.instance.MissFlipperRoll, FlipperFlop.instance.Nothing);

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
                            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(beat + i, delegate { Jukebox.PlayOneShotGame(soundToPlay); }),
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
                        FlipperFlop.instance.ScheduleInput(beat - 1, 1 + i, InputType.STANDARD_DOWN, FlipperFlop.instance.JustFlip, FlipperFlop.instance.MissFlip, FlipperFlop.instance.Nothing);
                    }
                }
                if (uh && flopCount != 4)
                {
                    for (int i = 0; i < 4 - flopCount; i++)
                    {
                        string voiceLine = $"flipperFlop/uh{flopCount + i}";
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + length + i, delegate { Jukebox.PlayOneShotGame(voiceLine); }),
                        });
                    }
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    queuedInputs.Add(new QueuedFlip { startBeat = beat, beat = beat + i, roll = roll });
                }
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
                return;
            }
            SuccessFlip(false);
        }

        public void JustFlipperRoll(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SuccessFlip(true);
        }

        public void SuccessFlip(bool roll)
        {
            foreach(var flipper in flippers)
            {
                flipper.Flip(roll, true);
            }
        }

        public void MissFlip(PlayerActionEvent caller)
        {

        }

        public void MissFlipperRoll(PlayerActionEvent caller)
        {

        }

        public void Nothing(PlayerActionEvent caller) {}
    }
}
