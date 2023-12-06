using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrDazzlesLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("theDazzles", "The Dazzles", "9cfff7", false, false, new List<GameAction>()
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
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, 0f, 1f, 2f, 0f, 1f, 2f, e["toggle"], e["toggle2"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Stars", "Should stars appear when successfully posing?"),
                        new Param("toggle2", true, "Cheer Sounds")
                    }
                },
                new GameAction("poseTwo", "Pose Vertical")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, 0f, 0f, 0f, 2f, 2f, 2f, e["toggle"], e["toggle2"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Stars", "Should stars appear when successfully posing?"),
                        new Param("toggle2", true, "Cheer Sounds")
                    }
                },
                new GameAction("poseSixDiagonal", "Pose Diagonal")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, 0f, 2.75f, 1.5f, 2f, 0.75f, 3.5f, e["toggle"], e["toggle2"]); },
                    defaultLength = 4.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Stars", "Should stars appear when successfully posing?"),
                        new Param("toggle2", true, "Cheer Sounds")
                    }
                },
                new GameAction("poseSixColumns", "Pose Rows")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, 0f, 0.5f, 1f, 2f, 2.5f, 3f, e["toggle"], e["toggle2"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Stars", "Should stars appear when successfully posing?"),
                        new Param("toggle2", true, "Cheer Sounds")
                    }
                },
                new GameAction("poseSix", "Pose Six")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, 0f, 0.5f, 1f, 1.5f, 2f, 2.5f, e["toggle"], e["toggle2"]); },
                    defaultLength = 4.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Stars", "Should stars appear when successfully posing?"),
                        new Param("toggle2", true, "Cheer Sounds")
                    }
                },
                new GameAction("customPose", "Custom Pose")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, e["upLeft"], e["upMiddle"], e["upRight"], e["downLeft"], e["downMiddle"], e["player"], e["toggle"], e["toggle2"]); },
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
                        new Param("toggle", false, "Stars", "Should stars appear when successfully posing?"),
                        new Param("toggle2", true, "Cheer Sounds")
                    }
                },
                new GameAction("forceHold", "Force Hold")
                {
                    function = delegate { TheDazzles.instance.ForceHold(); },
                    defaultLength = 0.5f
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; TheDazzles.instance.Bop(e.beat, e.length, e["toggle2"], e["toggle"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle2", true, "Should bop?", "Should the dazzles bop?"),
                        new Param("toggle", false, "Should auto bop?", "Should the dazzles auto bop?")
                    }
                },

            },
            new List<string>() {"ntr", "normal"},
            "ntrboxshow", "en",
            new List<string>() {"en"}
            );
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
            // override object.Equals
            public override bool Equals(object obj)
            {
                //
                // See the full list of guidelines at
                //   http://go.microsoft.com/fwlink/?LinkID=85237
                // and also the guidance for operator== at
                //   http://go.microsoft.com/fwlink/?LinkId=85238
                //
                
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                
                // TODO: write your implementation of Equals() here
                throw new System.NotImplementedException();
            }
            
            // override object.GetHashCode
            public override int GetHashCode()
            {
                // TODO: write your implementation of GetHashCode() here
                throw new System.NotImplementedException();
            }
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
            public double beat;
            public float length;
            public float upLeftBeat;
            public float upMiddleBeat;
            public float upRightBeat;
            public float downLeftBeat;
            public float downMiddleBeat;
            public float playerBeat;
            public bool stars;
            public bool cheer;
        }
        public struct QueuedCrouch
        {
            public double beat;
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
        double crouchEndBeat;
        static List<QueuedPose> queuedPoses = new List<QueuedPose>();
        static List<QueuedCrouch> queuedCrouches = new List<QueuedCrouch>();
        [Header("Components")]
        [SerializeField] List<TheDazzlesGirl> npcGirls = new List<TheDazzlesGirl>();
        [SerializeField] TheDazzlesGirl player;
        [SerializeField] ParticleSystem poseEffect;
        [SerializeField] ParticleSystem starsEffect;

        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("NtrBoxshowTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        void OnDestroy()
        {
            if (queuedPoses.Count > 0) queuedPoses.Clear();
            if (queuedCrouches.Count > 0) queuedCrouches.Clear();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        void Awake()
        {
            instance = this;
            SetupBopRegion("theDazzles", "bop", "toggle");
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat))
            {
                foreach (var girl in npcGirls)
                {
                    girl.Bop();
                }
                player.Bop();
            }
        }

        void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedPoses.Count > 0)
                {
                    foreach (var pose in queuedPoses)
                    {
                        Pose(pose.beat, pose.length, pose.upLeftBeat, pose.upMiddleBeat, pose.upRightBeat, pose.downLeftBeat, pose.downMiddleBeat, pose.playerBeat, pose.stars, pose.cheer);
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

                if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
                {
                    player.Prepare(false);
                    SoundByte.PlayOneShotGame("theDazzles/miss");
                    foreach (var girl in npcGirls)
                    {
                        if (girl.currentEmotion != TheDazzlesGirl.Emotion.Ouch) girl.currentEmotion = TheDazzlesGirl.Emotion.Angry;
                    }
                }
                if (PlayerInput.GetIsAction(InputAction_FlickRelease) && !IsExpectingInputNow(InputAction_FlickRelease))
                {
                    if (doingPoses || PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
                    {
                        player.Pose(false, doingPoses);
                        SoundByte.PlayOneShotGame("theDazzles/miss");
                        if (doingPoses)
                        {
                            foreach (var girl in npcGirls)
                            {
                                girl.Ouch();
                            }
                        }
                    }
                    else
                    {
                        player.UnPrepare();
                    }
                    shouldHold = false;
                }
                if (PlayerInput.GetIsAction(InputAction_TouchRelease) && !GameManager.instance.autoplay)
                {
                    player.UnPrepare();
                    shouldHold = false;
                }
                if (PlayerInput.GetIsAction(InputAction_BasicRelease) && shouldHold && !GameManager.instance.autoplay)
                {
                    if (doingPoses)
                    {
                        player.Pose(false);
                        SoundByte.PlayOneShotGame("theDazzles/miss");
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

        public void Bop(double beat, float length, bool goBop, bool autoBop)
        {
            if (goBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            foreach (var girl in npcGirls)
                            {
                                girl.Bop();
                            }
                            player.Bop();
                        })
                    });
                }
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

        public static void PreCrouch(double beat, float length, int countInType)
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

        public void CrouchStretchable(double beat, float length, int countInType)
        {
            float actualLength = length / 3;
            crouchEndBeat = beat + length;
            ScheduleInput(beat, 2f * actualLength, InputAction_BasicPress, JustCrouch, Nothing, Nothing);

            BeatAction.New(instance, new List<BeatAction.Action>()
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

        public static void PrePose(double beat, float length, float upLeftBeat, float upMiddleBeat, float upRightBeat, float downLeftBeat, float downMiddleBeat, float playerBeat, bool stars, bool cheer)
        {
            if (GameManager.instance.currentGame == "theDazzles")
            {
                instance.Pose(beat, length, upLeftBeat, upMiddleBeat, upRightBeat, downLeftBeat, downMiddleBeat, playerBeat, stars, cheer);
            }
            else
            {
                queuedPoses.Add(new QueuedPose { beat = beat, upLeftBeat = upLeftBeat, stars = stars, length = length, 
                    downLeftBeat = downLeftBeat, playerBeat = playerBeat, upMiddleBeat = upMiddleBeat, downMiddleBeat = downMiddleBeat, upRightBeat = upRightBeat, cheer = cheer});
            }
        }

        public void Pose(double beat, float length, float upLeftBeat, float upMiddleBeat, float upRightBeat, float downLeftBeat, float downMiddleBeat, float playerBeat, bool stars, bool cheer)
        {
            if (stars)
            {
                ScheduleInput(beat, playerBeat, InputAction_FlickRelease, cheer ? JustPoseStars : JustPoseStarsNoCheer, MissPose, Nothing);
            }
            else
            {
                ScheduleInput(beat, playerBeat, InputAction_FlickRelease, cheer ? JustPose : JustPoseNoCheer, MissPose, Nothing);
            }
            double crouchBeat = beat - 1f;
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
            BeatAction.New(instance, posesToDo);
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
            SoundByte.PlayOneShotGame("theDazzles/pose");
            SoundByte.PlayOneShotGame("theDazzles/posePlayer");
            if (state >= 1f || state <= -1f)
            {
                player.Pose();
                return;
            }
            SuccessPose(false, true);
        }

        void JustPoseStars(PlayerActionEvent caller, float state)
        {
            shouldHold = false;
            SoundByte.PlayOneShotGame("theDazzles/pose");
            SoundByte.PlayOneShotGame("theDazzles/posePlayer");
            if (state >= 1f || state <= -1f)
            {
                player.Pose();
                return;
            }
            SuccessPose(true, true);
        }

        void JustPoseNoCheer(PlayerActionEvent caller, float state)
        {
            shouldHold = false;
            SoundByte.PlayOneShotGame("theDazzles/pose");
            SoundByte.PlayOneShotGame("theDazzles/posePlayer");
            if (state >= 1f || state <= -1f)
            {
                player.Pose();
                return;
            }
            SuccessPose(false, false);
        }

        void JustPoseStarsNoCheer(PlayerActionEvent caller, float state)
        {
            shouldHold = false;
            SoundByte.PlayOneShotGame("theDazzles/pose");
            SoundByte.PlayOneShotGame("theDazzles/posePlayer");
            if (state >= 1f || state <= -1f)
            {
                player.Pose();
                return;
            }
            SuccessPose(true, false);
        }

        void SuccessPose(bool stars, bool cheer)
        {
            player.Pose();
            if (cheer) SoundByte.PlayOneShotGame("theDazzles/applause");
            foreach (var girl in npcGirls)
            {
                girl.currentEmotion = TheDazzlesGirl.Emotion.Happy;
            }
            player.currentEmotion = TheDazzlesGirl.Emotion.Happy;
            if (stars) 
            {
                starsEffect.Play();
                SoundByte.PlayOneShotGame($"theDazzles/stars{UnityEngine.Random.Range(1, 6)}");
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

