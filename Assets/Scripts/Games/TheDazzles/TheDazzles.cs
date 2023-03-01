using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrDazzlesLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("theDazzles", "The Dazzles", "E7A59C", false, false, new List<GameAction>()
            {
                new GameAction("crouch", "Crouch")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PreCrouch(e.beat, e.length, e["countIn"]);  },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("countIn", TheDazzles.CountInType.DS, "Count In Type", "Should the count-In be from megamix, DS or random?")
                    }
                },
                new GameAction("crouchStretch", "Crouch (Stretchable)")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PreCrouch(e.beat, e.length, e["countIn"]); },
                    defaultLength = 3f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("countIn", TheDazzles.CountInType.DS, "Count In Type", "Should the count-In be from megamix, DS or random?")
                    }
                },
                new GameAction("poseThree", "Pose Horizontal")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, 0f, 1f, 2f, 0f, 1f, 2f, e["toggle"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Stars", "Should stars appear when successfully posing?")
                    }
                },
                new GameAction("poseTwo", "Pose Vertical")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, 0f, 0f, 0f, 2f, 2f, 2f, e["toggle"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Stars", "Should stars appear when successfully posing?")
                    }
                },
                new GameAction("poseSixDiagonal", "Pose Diagonal")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, 0f, 2.75f, 1.5f, 2f, 0.75f, 3.5f, e["toggle"]); },
                    defaultLength = 4.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Stars", "Should stars appear when successfully posing?")
                    }
                },
                new GameAction("poseSixColumns", "Pose Rows")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, 0f, 0.5f, 1f, 2f, 2.5f, 3f, e["toggle"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Stars", "Should stars appear when successfully posing?")
                    }
                },
                new GameAction("poseSix", "Pose Six")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, 0f, 0.5f, 1f, 1.5f, 2f, 2.5f, e["toggle"]); },
                    defaultLength = 4.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Stars", "Should stars appear when successfully posing?")
                    }
                },
                new GameAction("customPose", "Custom Pose")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, e["upLeft"], e["upMiddle"], e["upRight"], e["downLeft"], e["downMiddle"], e["player"], e["toggle"]); },
                    defaultLength = 3f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("upLeft", new EntityTypes.Float(0, 30f, 0f), "Up Left Girl Pose Beat", "How many beats after the event has started will this girl pose?"),
                        new Param("upMiddle", new EntityTypes.Float(0, 30f, 1f), "Up Middle Girl Pose Beat", "How many beats after the event has started will this girl pose?"),
                        new Param("upRight", new EntityTypes.Float(0, 30f, 2f), "Up Right Girl Pose Beat", "How many beats after the event has started will this girl pose?"),
                        new Param("downLeft", new EntityTypes.Float(0, 30f, 0f), "Down Left Girl Pose Beat", "How many beats after the event has started will this girl pose?"),
                        new Param("downMiddle", new EntityTypes.Float(0, 30f, 1f), "Down Middle Girl Pose Beat", "How many beats after the event has started will this girl pose?"),
                        new Param("player", new EntityTypes.Float(0, 30f, 2f), "Player Pose Beat", "How many beats after the event has started should the player pose?"),
                        new Param("toggle", false, "Stars", "Should stars appear when successfully posing?")
                    }
                },
                new GameAction("forceHold", "Force Hold")
                {
                    function = delegate { TheDazzles.instance.ForceHold(); },
                    defaultLength = 0.5f
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate { TheDazzles.instance.shouldBop = eventCaller.currentEntity["toggle"]; },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Should bop?", "Should the dazzles bop?")
                    }
                },

            });
        }
    }
}
namespace HeavenStudio.Games
{
    using Scripts_TheDazzles;
    public class TheDazzles : Minigame
    {
        public struct PosesToPerform : IComparable<PosesToPerform>
        {
            public int CompareTo(PosesToPerform other)
            {
                if (other == null) return 1;

                return beat.CompareTo(other.beat);
            }

            public static bool operator > (PosesToPerform operand1, PosesToPerform operand2)
            {
                return operand1.CompareTo(operand2) > 0;
            }

            public static bool operator <(PosesToPerform operand1, PosesToPerform operand2)
            {
                return operand1.CompareTo(operand2) < 0;
            }

            public static bool operator >=(PosesToPerform operand1, PosesToPerform operand2)
            {
                return operand1.CompareTo(operand2) >= 0;
            }

            public static bool operator <=(PosesToPerform operand1, PosesToPerform operand2)
            {
                return operand1.CompareTo(operand2) <= 0;
            }

            public static bool operator ==(PosesToPerform operand1, PosesToPerform operand2)
            {
                return operand1.CompareTo(operand2) == 0;
            }

            public static bool operator !=(PosesToPerform operand1, PosesToPerform operand2)
            {
                return operand1.CompareTo(operand2) != 0;
            }
            public int girlIndex;
            public float beat;
        }
        public struct QueuedPose
        {
            public float beat;
            public float length;
            public float upLeftBeat;
            public float upMiddleBeat;
            public float upRightBeat;
            public float downLeftBeat;
            public float downMiddleBeat;
            public float playerBeat;
            public bool stars;
        }
        public struct QueuedCrouch
        {
            public float beat;
            public float length;
            public int countInType;
        }
        public enum CountInType
        {
            DS = 0,
            Megamix = 1,
            Random = 2,
        }
        public static TheDazzles instance;

        [Header("Variables")]
        bool canBop = true;
        bool doingPoses = false;
        bool shouldHold = false;
        float crouchEndBeat;
        public bool shouldBop = true;
        public GameEvent bop = new GameEvent();
        static List<QueuedPose> queuedPoses = new List<QueuedPose>();
        static List<QueuedCrouch> queuedCrouches = new List<QueuedCrouch>();
        [Header("Components")]
        [SerializeField] List<TheDazzlesGirl> npcGirls = new List<TheDazzlesGirl>();
        [SerializeField] TheDazzlesGirl player;
        [SerializeField] ParticleSystem poseEffect;
        [SerializeField] ParticleSystem starsEffect;

        void OnDestroy()
        {
            if (queuedPoses.Count > 0) queuedPoses.Clear();
            if (queuedCrouches.Count > 0) queuedCrouches.Clear();
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
                if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
                {
                    if (shouldBop)
                    {
                        foreach (var girl in npcGirls)
                        {
                            girl.Bop();
                        }
                        player.Bop();
                    }
                }
                if (queuedPoses.Count > 0)
                {
                    foreach (var pose in queuedPoses)
                    {
                        Pose(pose.beat, pose.length, pose.upLeftBeat, pose.upMiddleBeat, pose.upRightBeat, pose.downLeftBeat, pose.downMiddleBeat, pose.playerBeat, pose.stars);
                    }
                    queuedPoses.Clear();
                }
                if (queuedCrouches.Count > 0)
                {
                    foreach (var crouch in queuedCrouches)
                    {
                        CrouchStretchable(crouch.beat, crouch.length, crouch.countInType);
                    }
                    queuedCrouches.Clear();
                }
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    player.Prepare(false);
                    Jukebox.PlayOneShotGame("theDazzles/miss");
                    foreach (var girl in npcGirls)
                    {
                        if (girl.currentEmotion != TheDazzlesGirl.Emotion.Ouch) girl.currentEmotion = TheDazzlesGirl.Emotion.Angry;
                    }
                }
                if (PlayerInput.PressedUp() && !IsExpectingInputNow(InputType.STANDARD_UP))
                {
                    if (doingPoses)
                    {
                        player.Pose(false);
                        Jukebox.PlayOneShotGame("theDazzles/miss");
                        foreach (var girl in npcGirls)
                        {
                            girl.Ouch();
                        }
                    }
                    else
                    {
                        player.UnPrepare();
                    }
                    shouldHold = false;
                }
                else if (!PlayerInput.Pressing() && !IsExpectingInputNow(InputType.STANDARD_UP) && shouldHold && !GameManager.instance.autoplay)
                {
                    if (doingPoses)
                    {
                        player.Pose(false);
                        Jukebox.PlayOneShotGame("theDazzles/miss");
                        foreach (var girl in npcGirls)
                        {
                            girl.Ouch();
                        }
                    }
                    else
                    {
                        player.UnPrepare();
                    }
                    shouldHold = false;
                }
            }
            else if (!cond.isPlaying && !cond.isPaused)
            {
                if (queuedPoses.Count > 0) queuedPoses.Clear();
                if (queuedCrouches.Count > 0) queuedCrouches.Clear();
            }
        }

        public void ForceHold()
        {
            shouldHold = true;
            foreach (var girl in npcGirls)
            {
                girl.Prepare();
            }
            player.Prepare();
        }

        public static void PreCrouch(float beat, float length, int countInType)
        {
            float actualLength = length / 3;
            int realCountInType = countInType;
            if (countInType == (int)CountInType.Random) realCountInType = UnityEngine.Random.Range(0, 2);
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>();
            switch (realCountInType)
            {
                case (int)CountInType.DS:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("theDazzles/holdDS3", beat, 1, 0.75f, false, 0.212f),
                        new MultiSound.Sound("theDazzles/holdDS2", beat + 1f * actualLength, 1, 0.75f, false, 0.242f),
                        new MultiSound.Sound("theDazzles/hold1", beat + 2f * actualLength, 1, 1, false, 0.019f),
                    });
                    break;
                case (int)CountInType.Megamix:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("theDazzles/hold3", beat, 1, 1, false, 0.267f),
                        new MultiSound.Sound("theDazzles/hold2", beat + 1f * actualLength, 1, 1, false, 0.266f),
                        new MultiSound.Sound("theDazzles/hold1", beat + 2f * actualLength, 1, 1, false, 0.019f),
                    });
                    break;
                default:
                    break;
            }
            MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);
            if (GameManager.instance.currentGame == "theDazzles")
            {
                instance.CrouchStretchable(beat, length, countInType);
            }
            else
            {
                queuedCrouches.Add(new QueuedCrouch { beat = beat, length = length, countInType = countInType });
            }
        }

        public void CrouchStretchable(float beat, float length, int countInType)
        {
            float actualLength = length / 3;
            crouchEndBeat = beat + length;
            ScheduleInput(beat, 2f * actualLength, InputType.STANDARD_DOWN, JustCrouch, Nothing, Nothing);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    npcGirls[1].canBop = false;
                    npcGirls[4].canBop = false;
                    npcGirls[1].Prepare();
                    npcGirls[4].Prepare();
                }),
                new BeatAction.Action(beat + 1f * actualLength, delegate
                {
                    npcGirls[0].canBop = false;
                    npcGirls[3].canBop = false;
                    npcGirls[0].Prepare();
                    npcGirls[3].Prepare();
                }),
                new BeatAction.Action(beat + 2f * actualLength, delegate
                {
                    npcGirls[2].canBop = false;
                    npcGirls[2].Prepare();
                }),
            });
        }

        public static void PrePose(float beat, float length, float upLeftBeat, float upMiddleBeat, float upRightBeat, float downLeftBeat, float downMiddleBeat, float playerBeat, bool stars)
        {
            if (GameManager.instance.currentGame == "theDazzles")
            {
                instance.Pose(beat, length, upLeftBeat, upMiddleBeat, upRightBeat, downLeftBeat, downMiddleBeat, playerBeat, stars);
            }
            else
            {
                queuedPoses.Add(new QueuedPose { beat = beat, upLeftBeat = upLeftBeat, stars = stars, length = length, 
                    downLeftBeat = downLeftBeat, playerBeat = playerBeat, upMiddleBeat = upMiddleBeat, downMiddleBeat = downMiddleBeat, upRightBeat = upRightBeat});
            }
        }

        public void Pose(float beat, float length, float upLeftBeat, float upMiddleBeat, float upRightBeat, float downLeftBeat, float downMiddleBeat, float playerBeat, bool stars)
        {
            if (stars)
            {
                ScheduleInput(beat, playerBeat, InputType.STANDARD_UP, JustPoseStars, MissPose, Nothing);
            }
            else
            {
                ScheduleInput(beat, playerBeat, InputType.STANDARD_UP, JustPose, MissPose, Nothing);
            }
            float crouchBeat = beat - 1f;
            if (crouchBeat < crouchEndBeat) 
            { 
                crouchBeat = crouchEndBeat - 1f;
            }
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("theDazzles/crouch", crouchBeat),
            }, forcePlay: true);
            List<float> soundBeats = new List<float>()
            {
                upLeftBeat,
                upMiddleBeat,
                upRightBeat,
                downLeftBeat,
                downMiddleBeat,
            };
            List<float> soundsToRemove = new List<float>();
            foreach (var sound in soundBeats)
            {
                if (sound == playerBeat) soundsToRemove.Add(sound);
            }
            if (soundsToRemove.Count > 0)
            {
                foreach (var sound in soundsToRemove)
                {
                    soundBeats.Remove(sound);
                }
            }
            soundBeats = soundBeats.Distinct().ToList();
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>();
            foreach (var sound in soundBeats)
            {
                soundsToPlay.Add(new MultiSound.Sound("theDazzles/pose", beat + sound));
            }
            MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);
            List<PosesToPerform> posesToPerform = new List<PosesToPerform>()
            {
                new PosesToPerform { beat = upLeftBeat, girlIndex = 4},
                new PosesToPerform { beat = upMiddleBeat, girlIndex = 3},
                new PosesToPerform { beat = upRightBeat, girlIndex = 2},
                new PosesToPerform { beat = downLeftBeat, girlIndex = 1},
                new PosesToPerform { beat = downMiddleBeat, girlIndex = 0},
            };
            posesToPerform.Sort();
            foreach(var pose in posesToPerform)
            {
                npcGirls[pose.girlIndex].StartReleaseBox(beat + pose.beat);
            }
            player.StartReleaseBox(beat + playerBeat);
            List<BeatAction.Action> posesToDo = new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1f, delegate
                {
                    foreach (var girl in npcGirls)
                    {
                        girl.canBop = false;
                        girl.Hold();
                    }
                    player.canBop = false;
                    player.Hold();
                }),
                new BeatAction.Action(beat, delegate
                {
                    doingPoses = true;
                }),

            };
            for (int i = 0; i < posesToPerform.Count; i++)
            {
                int index = posesToPerform[i].girlIndex;
                posesToDo.Add(new BeatAction.Action(beat + posesToPerform[i].beat, delegate
                {
                    npcGirls[index].Pose();
                }));
            }
            posesToDo.Add(new BeatAction.Action(beat + playerBeat, delegate
            {
                doingPoses = false;
            }));
            posesToDo.Add(new BeatAction.Action(beat + length, delegate
            {
                foreach (var girl in npcGirls)
                {
                    girl.EndPose();
                }
                player.EndPose();
            }));
            posesToDo.Add(new BeatAction.Action(beat + length + 0.1f, delegate
            {
                foreach (var girl in npcGirls)
                {
                    girl.canBop = true;
                }
                player.canBop = true;
            }));
            BeatAction.New(instance.gameObject, posesToDo);
        }

        void JustCrouch(PlayerActionEvent caller, float state)
        {
            player.canBop = false;
            if (state >= 1f || state <= -1f)
            {
                player.Prepare();
                return;
            }
            SuccessCrouch();
        }

        void SuccessCrouch()
        {
            player.Prepare();
        }

        void JustPose(PlayerActionEvent caller, float state)
        {
            shouldHold = false;
            Jukebox.PlayOneShotGame("theDazzles/pose");
            Jukebox.PlayOneShotGame("theDazzles/posePlayer");
            if (state >= 1f || state <= -1f)
            {
                player.Pose();
                return;
            }
            SuccessPose(false);
        }

        void JustPoseStars(PlayerActionEvent caller, float state)
        {
            shouldHold = false;
            Jukebox.PlayOneShotGame("theDazzles/pose");
            Jukebox.PlayOneShotGame("theDazzles/posePlayer");
            if (state >= 1f || state <= -1f)
            {
                player.Pose();
                return;
            }
            SuccessPose(true);
        }

        void SuccessPose(bool stars)
        {
            player.Pose();
            Jukebox.PlayOneShotGame("theDazzles/applause");
            foreach (var girl in npcGirls)
            {
                girl.currentEmotion = TheDazzlesGirl.Emotion.Happy;
            }
            player.currentEmotion = TheDazzlesGirl.Emotion.Happy;
            if (stars) 
            {
                starsEffect.Play();
                Jukebox.PlayOneShotGame($"theDazzles/stars{UnityEngine.Random.Range(1, 6)}");
            } 
            else poseEffect.Play();
        }

        void MissPose(PlayerActionEvent caller)
        {
            foreach (var girl in npcGirls)
            {
                if (girl.currentEmotion != TheDazzlesGirl.Emotion.Ouch) girl.currentEmotion = TheDazzlesGirl.Emotion.Angry;
            }
            player.hasOuched = true;
        }

        void Nothing(PlayerActionEvent caller) { }
    }
}

