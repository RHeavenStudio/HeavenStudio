using HeavenStudio.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlTapTroupeLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tapTroupe", "Tap Troupe", "TAPTAP", false, false, new List<GameAction>()
            {
                new GameAction("stepping", "Stepping")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TapTroupe.PreStepping(e.beat, e.length, e["startTap"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("startTap", false, "Start Tap Voice Line", "Whether or not it should say -Tap!- on the first step.")
                    }
                },
                new GameAction("tapping", "Tapping")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TapTroupe.PreTapping(e.beat, e.length, e["okay"], e["okayType"], e["animType"], e["popperBeats"], e["randomVoiceLine"]); },
                    defaultLength = 3f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("okay", true, "Okay Voice Line", "Whether or not the tappers should say -Okay!- after successfully tapping."),
                        new Param("okayType", TapTroupe.OkayType.Random, "Okay Type", "Which version of the okay voice line should the tappers say?"),
                        new Param("animType", TapTroupe.OkayAnimType.Normal, "Okay Animation", "Which animations should be played when the tapper say OK?"),
                        new Param("popperBeats", new EntityTypes.Float(0f, 80f, 2f), "Popper Beats", "How many beats until the popper will pop?"),
                        new Param("randomVoiceLine", true, "Extra Random Voice Line", "Whether there should be randomly said woos or laughs after the tappers say OK!")
                    }
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate {TapTroupe.instance.Bop(); },
                    defaultLength = 1f
                },
                new GameAction("spotlights", "Toggle Spotlights")
                {
                    function = delegate {var e = eventCaller.currentEntity; TapTroupe.instance.Spotlights(e["toggle"], e["player"], e["middleLeft"], e["middleRight"], e["leftMost"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Darkness On", "Whether or not it should be dark."),
                        new Param("player", true, "Player Spotlight", "Whether or not the player spotlight should be turned on or off."),
                        new Param("middleLeft", false, "Middleleft Tapper Spotlight", "Whether or not the middleleft tapper spotlight should be turned on or off."),
                        new Param("middleRight", false, "Middleright Tapper Spotlight", "Whether or not the middleright tapper spotlight should be turned on or off."),
                        new Param("leftMost", false, "Leftmost Tapper Spotlight", "Whether or not the leftmost tapper spotlight should be turned on or off."),
                    }
                },
                new GameAction("tutorialMissFace", "Toggle Tutorial Miss Face")
                {
                    function = delegate { var e = eventCaller.currentEntity;  TapTroupe.instance.ToggleMissFace(e["toggle"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Use it?", "Use the faces they do when you miss in the tutorial of Tap Troupe?")
                    }
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_TapTroupe;
    public class TapTroupe : Minigame
    {
        [Header("Components")]
        [SerializeField] TapTroupeTapper playerTapper;
        [SerializeField] TapTroupeCorner playerCorner;
        [SerializeField] List<TapTroupeTapper> npcTappers = new List<TapTroupeTapper>();
        [SerializeField] List<TapTroupeCorner> npcCorners = new List<TapTroupeCorner>();
        [SerializeField] GameObject spotlightPlayer;
        [SerializeField] GameObject spotlightMiddleLeft;
        [SerializeField] GameObject spotlightMiddleRight;
        [SerializeField] GameObject spotlightLeftMost;
        [SerializeField] GameObject darkness;
        [Header("Properties")]
        private static List<QueuedSteps> queuedSteps = new List<QueuedSteps>();
        private static List<QueuedTaps> queuedTaps = new List<QueuedTaps>();
        public static bool prepareTap;
        private bool tapping;
        private bool shouldSwitchStep;
        private bool shouldDoSecondBam;
        private bool missedTaps;
        private bool canSpit = true;
        private bool useTutorialMissFace;
        private TapTroupeTapper.TapAnim currentTapAnim;
        public struct QueuedSteps
        {
            public float beat;
            public float length;
            public bool startTap;
        }
        public struct QueuedTaps
        {
            public float beat;
            public float length;
            public bool okay;
            public int okayType;
            public int animType;
            public float popperBeats;
            public bool randomVoiceLine;
        }
        public enum OkayType
        {
            OkayA = 0,
            OkayB = 1,
            OkayC = 2,
            Random = 3
        }
        public enum OkayAnimType
        {
            Normal = 0,
            Popper = 1,
            OkSign = 2,
            Random = 3
        }
        private int stepSound = 1;

        public static TapTroupe instance;

        void OnDestroy()
        {
            if (queuedSteps.Count > 0) queuedSteps.Clear();
            if (queuedTaps.Count > 0) queuedTaps.Clear();
            prepareTap = false;
        }

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedSteps.Count > 0)
                {
                    foreach (var step in queuedSteps)
                    {
                        Stepping(step.beat, step.length, step.startTap);
                    }
                    queuedSteps.Clear();
                }
                if (queuedTaps.Count > 0)
                {
                    foreach (var tap in queuedTaps)
                    {
                        Tapping(tap.beat, tap.length, tap.okay, tap.okayType, tap.animType, tap.popperBeats, tap.randomVoiceLine);
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(tap.beat - 1.1f, delegate { prepareTap = true; }),
                            new BeatAction.Action(tap.beat, delegate { prepareTap = false; })
                        });
                    }
                    queuedTaps.Clear();
                }
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    if (canSpit && !useTutorialMissFace) Jukebox.PlayOneShotGame("tapTroupe/spit");
                    Jukebox.PlayOneShotGame("tapTroupe/miss");
                    TapTroupe.instance.ScoreMiss(0.5f);
                    foreach (var corner in npcCorners)
                    {
                        if (useTutorialMissFace)
                        {
                            corner.SetMissFace(TapTroupeCorner.MissFace.LOL);
                        }
                        else
                        {
                            corner.SetMissFace(TapTroupeCorner.MissFace.Spit);
                        }
                    }
                    if (tapping)
                    {
                        missedTaps = true;
                        playerTapper.Tap(currentTapAnim, false, shouldSwitchStep);
                        playerCorner.Bop();
                    }
                    else
                    {
                        playerTapper.Step(false);
                        playerCorner.Bop();
                    }
                    canSpit = false;
                }
            }
        }

        public static void PreStepping(float beat, float length, bool startTap)
        {
            if (GameManager.instance.currentGame == "tapTroupe")
            {
                TapTroupe.instance.Stepping(beat, length, startTap);

            }
            else
            {
                queuedSteps.Add(new QueuedSteps { beat = beat, length = length, startTap = startTap });
            }
        }

        public void Stepping(float beat, float length, bool startTap)
        {
            for (int i = 0; i < length; i++)
            {
                TapTroupe.instance.ScheduleInput(beat - 1, 1 + i, InputType.STANDARD_DOWN, TapTroupe.instance.JustStep, TapTroupe.instance.MissStep, TapTroupe.instance.Nothing);
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + i, delegate
                    {
                        TapTroupe.instance.NPCStep();
                    })
                });
            }
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1, delegate
                {
                    if (tapping) return;
                    TapTroupe.instance.NPCStep(false, false);
                    TapTroupe.instance.playerTapper.Step(false, false);
                    TapTroupe.instance.playerCorner.Bop();
                }),
                new BeatAction.Action(beat, delegate { if (startTap) Jukebox.PlayOneShotGame("tapTroupe/startTap"); })
            });
        }

        public static void PreTapping(float beat, float length, bool okay, int okayType, int animType, float popperBeats, bool randomVoiceLine)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTroupe/tapReady1", beat - 2f),
                new MultiSound.Sound("tapTroupe/tapReady2", beat - 1f),
            }, forcePlay: true);
            if (GameManager.instance.currentGame == "tapTroupe")
            {
                TapTroupe.instance.Tapping(beat, length, okay, okayType, animType, popperBeats, randomVoiceLine);
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 1.1f, delegate { prepareTap = true; }),
                    new BeatAction.Action(beat, delegate { prepareTap = false; })
                });
            }
            else
            {
                queuedTaps.Add(new QueuedTaps { beat = beat, length = length, okay = okay, okayType = okayType, animType = animType, popperBeats = popperBeats, randomVoiceLine = randomVoiceLine });
            }
        }

        public void Tapping(float beat, float length, bool okay, int okayType, int animType, float popperBeats, bool randomVoiceLine)
        {
            float actualLength = length - 0.5f;
            actualLength -= actualLength % 0.75f;
            bool secondBam = false;
            float finalBeatToSpawn = 0f;
            if (actualLength < 2.25f) actualLength = 2.25f;
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>
            {
                new MultiSound.Sound("tapTroupe/tapAnd", beat)
            };
            for (float i = 0; i < actualLength; i += 0.75f)
            {
                string soundToPlay = "bamvoice1";
                float beatToSpawn = beat + i + 0.5f;
                if (i + 0.75f >= actualLength)
                {
                    soundToPlay = "startTap";
                    beatToSpawn = Mathf.Ceil(beat + i);
                    finalBeatToSpawn = beatToSpawn;
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beatToSpawn - 0.3f, delegate { currentTapAnim = TapTroupeTapper.TapAnim.LastTap; shouldSwitchStep = false; }),
                        new BeatAction.Action(beatToSpawn, delegate { NPCTap(TapTroupeTapper.TapAnim.LastTap, true, false);}),
                        new BeatAction.Action(beatToSpawn + 0.1f, delegate { tapping = false; })
                    });
                }
                else if (i + 1.5f >= actualLength)
                {
                    soundToPlay = "tapvoice2";
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beatToSpawn - 0.3f, delegate { currentTapAnim = TapTroupeTapper.TapAnim.Tap; shouldSwitchStep = false; }),
                        new BeatAction.Action(beatToSpawn, delegate { NPCTap(TapTroupeTapper.TapAnim.Tap, true, false); })
                    });
                }
                else if (i + 2.25f >= actualLength)
                {
                    soundToPlay = "tapvoice1";
                    if (actualLength == 2.25f)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beatToSpawn - 0.3f, delegate { currentTapAnim = TapTroupeTapper.TapAnim.Tap; shouldSwitchStep = true; }),
                            new BeatAction.Action(beatToSpawn, delegate { NPCTap(TapTroupeTapper.TapAnim.Tap); })
                        });
                    }
                    else
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beatToSpawn - 0.3f, delegate { currentTapAnim = TapTroupeTapper.TapAnim.Tap; shouldSwitchStep = false; }),
                            new BeatAction.Action(beatToSpawn, delegate { NPCTap(TapTroupeTapper.TapAnim.Tap, true, false); })
                        });
                    }
                }
                else
                {
                    if (secondBam) soundToPlay = "bamvoice2";
                    if (i + 3f >= actualLength)
                    {
                        if (actualLength == 3f)
                        {
                            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(beatToSpawn - 0.3f, delegate { currentTapAnim = TapTroupeTapper.TapAnim.Tap; shouldSwitchStep = true; }),
                                new BeatAction.Action(beatToSpawn, delegate { NPCTap(TapTroupeTapper.TapAnim.Tap); })
                            });
                        }
                        else
                        {
                            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(beatToSpawn - 0.3f, delegate { currentTapAnim = TapTroupeTapper.TapAnim.BamTapReady; shouldSwitchStep = true; }),
                                new BeatAction.Action(beatToSpawn, delegate { NPCTap(TapTroupeTapper.TapAnim.BamTapReady); })
                            });
                        }
                    }
                    else if (i == 0)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beatToSpawn - 0.3f, delegate { currentTapAnim = TapTroupeTapper.TapAnim.BamReady; shouldSwitchStep = false; }),
                            new BeatAction.Action(beatToSpawn, delegate { NPCTap(TapTroupeTapper.TapAnim.BamReady, true, false); })
                        });
                    }
                    else
                    {
                        
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beatToSpawn - 0.3f, delegate { currentTapAnim = TapTroupeTapper.TapAnim.Bam; shouldSwitchStep = true; }),
                            new BeatAction.Action(beatToSpawn, delegate { NPCTap(TapTroupeTapper.TapAnim.Bam); })
                        });
                    }
                }
                soundsToPlay.Add(new MultiSound.Sound($"tapTroupe/{soundToPlay}", beatToSpawn));
                shouldDoSecondBam = secondBam;
                secondBam = !secondBam;
                ScheduleInput(beatToSpawn - 1, 1f, InputType.STANDARD_DOWN, JustTap, MissTap, Nothing);
            }
            int actualOkayType = okayType;
            if (actualOkayType == (int)OkayType.Random) actualOkayType = UnityEngine.Random.Range(0, 3);
            string okayVoiceLine = "A";
            switch (actualOkayType)
            {
                case (int)OkayType.OkayA:
                    okayVoiceLine = "A";
                    break;
                case (int)OkayType.OkayB:
                    okayVoiceLine = "B";
                    break;
                case (int)OkayType.OkayC:
                    okayVoiceLine = "C";
                    break;
                default:
                    okayVoiceLine = "A";
                    break;
            }
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { tapping = true; missedTaps = false; }),
                new BeatAction.Action(finalBeatToSpawn, delegate 
                {
                    if (missedTaps || animType != (int)OkayAnimType.Popper) return;
                    npcCorners[0].PartyPopper(finalBeatToSpawn + popperBeats);
                }),
                new BeatAction.Action(finalBeatToSpawn + 0.5f, delegate
                {
                    if (missedTaps || !okay) return;
                    playerCorner.Okay();
                    foreach (var corner in npcCorners)
                    {
                        corner.Okay();
                    }
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound($"tapTroupe/okay{okayVoiceLine}1", finalBeatToSpawn + 0.5f),
                        new MultiSound.Sound($"tapTroupe/okay{okayVoiceLine}2", finalBeatToSpawn + 1f),
                    }, forcePlay: true);
                }),
                new BeatAction.Action(finalBeatToSpawn + 1f, delegate
                {
                    if (randomVoiceLine && UnityEngine.Random.Range(1, 50) == 1)
                    {
                        Jukebox.PlayOneShotGame("tapTroupe/woo");
                    }
                    else if (randomVoiceLine && UnityEngine.Random.Range(1, 50) == 1)
                    {
                        Jukebox.PlayOneShotGame("tapTroupe/laughter", -1, 1, 0.4f);
                    }
                    if (missedTaps || animType != (int)OkayAnimType.OkSign) return;
                    playerCorner.OkaySign();
                    foreach (var corner in npcCorners)
                    {
                        corner.OkaySign();
                    }
                })
            });
            MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);
        }

        public void Bop()
        {
            playerTapper.Bop();
            playerCorner.Bop();
            foreach (var tapper in npcTappers)
            {
                tapper.Bop();
            }
            foreach (var corner in npcCorners)
            {
                corner.Bop();
            }
        }

        public void ToggleMissFace(bool isOn)
        {
            useTutorialMissFace = isOn;
        }

        public void Spotlights(bool isOn, bool player, bool middleLeft, bool middleRight, bool leftMost)
        {
            if (isOn)
            {
                darkness.SetActive(true);
                spotlightPlayer.SetActive(player);
                spotlightMiddleLeft.SetActive(middleLeft);
                spotlightMiddleRight.SetActive(middleRight);
                spotlightLeftMost.SetActive(leftMost);
            }
            else
            {
                darkness.SetActive(false);
                spotlightPlayer.SetActive(false);
                spotlightMiddleLeft.SetActive(false);
                spotlightMiddleRight.SetActive(false);
                spotlightLeftMost.SetActive(false);
            }
        }
        
        public void NPCStep(bool hit = true, bool switchFeet = true)
        {
            foreach (var tapper in npcTappers)
            {
                tapper.Step(hit, switchFeet);
            }
            foreach (var corner in npcCorners)
            {
                corner.Bop();
            }
        }

        public void NPCTap(TapTroupeTapper.TapAnim animType, bool hit = true, bool switchFeet = true)
        {
            foreach (var tapper in npcTappers)
            {
                tapper.Tap(animType, hit, switchFeet);
            }
            foreach (var corner in npcCorners)
            {
                corner.Bop();
            }
        }

        void JustStep(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                canSpit = true;
                playerTapper.Step(false);
                playerCorner.Bop();
                Jukebox.PlayOneShotGame($"tapTroupe/step{stepSound}");
                if (stepSound == 1)
                {
                    stepSound = 2;
                }
                else
                {
                    stepSound = 1;
                }
                foreach (var corner in npcCorners)
                {
                    if (useTutorialMissFace)
                    {
                        corner.SetMissFace(TapTroupeCorner.MissFace.LOL);
                    }
                    else
                    {
                        corner.SetMissFace(TapTroupeCorner.MissFace.Sad);
                    }
                }
                return;
            }
            SuccessStep();
        }

        void SuccessStep()
        {
            canSpit = true;
            playerTapper.Step();

            playerCorner.Bop();
            Jukebox.PlayOneShotGame($"tapTroupe/step{stepSound}");
            if (stepSound == 1)
            {
                stepSound = 2;
            }
            else
            {
                stepSound = 1;
            }
            foreach (var corner in npcCorners)
            {
                corner.ResetFace();
            }
        }

        void MissStep(PlayerActionEvent caller)
        {
            if (canSpit && !useTutorialMissFace) Jukebox.PlayOneShotGame("tapTroupe/spit");
            foreach (var corner in npcCorners)
            {
                if (useTutorialMissFace)
                {
                    corner.SetMissFace(TapTroupeCorner.MissFace.LOL);
                }
                else
                {
                    corner.SetMissFace(TapTroupeCorner.MissFace.Spit);
                }
            }
            canSpit = false;
        }

        void JustTap(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                missedTaps = true;
                canSpit = true;
                playerTapper.Tap(currentTapAnim, false, shouldSwitchStep);
                playerCorner.Bop();
                switch (currentTapAnim)
                {
                    case TapTroupeTapper.TapAnim.LastTap:
                        Jukebox.PlayOneShotGame("tapTroupe/tap3");
                        break;
                    default:
                        if (shouldDoSecondBam)
                        {
                            Jukebox.PlayOneShotGame("tapTroupe/bam2");
                        }
                        else
                        {
                            Jukebox.PlayOneShotGame("tapTroupe/bam1");
                        }
                        break;
                }
                foreach (var corner in npcCorners)
                {
                    if (useTutorialMissFace)
                    {
                        corner.SetMissFace(TapTroupeCorner.MissFace.LOL);
                    }
                    else
                    {
                        corner.SetMissFace(TapTroupeCorner.MissFace.Sad);
                    }
                }
                return;
            }
            SuccessTap();
        }
        
        void SuccessTap()
        {
            canSpit = true;
            playerTapper.Tap(currentTapAnim, true, shouldSwitchStep);
            playerCorner.Bop();
            switch (currentTapAnim)
            {
                case TapTroupeTapper.TapAnim.LastTap:
                    Jukebox.PlayOneShotGame("tapTroupe/tap3");
                    break;
                default:
                    if (shouldDoSecondBam)
                    {
                        Jukebox.PlayOneShotGame("tapTroupe/bam2");
                    }
                    else
                    {
                        Jukebox.PlayOneShotGame("tapTroupe/bam1");
                    }
                    break;
            }
            foreach (var corner in npcCorners)
            {
                corner.ResetFace();
            }
        }

        void MissTap(PlayerActionEvent caller)
        {
            missedTaps = true;
            if (canSpit && !useTutorialMissFace) Jukebox.PlayOneShotGame("tapTroupe/spit");
            foreach (var corner in npcCorners)
            {
                if (useTutorialMissFace)
                {
                    corner.SetMissFace(TapTroupeCorner.MissFace.LOL);
                }
                else
                {
                    corner.SetMissFace(TapTroupeCorner.MissFace.Spit);
                }
            }
            canSpit = false;
        }

        void Nothing(PlayerActionEvent caller) { }
    }
}
