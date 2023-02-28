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
                    function = delegate { TheDazzles.instance.Crouch(eventCaller.currentEntity.beat); },
                    defaultLength = 3f,
                },
                new GameAction("crouchStretch", "Crouch (Stretchable)")
                {
                    function = delegate { TheDazzles.instance.CrouchStretchable(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); },
                    defaultLength = 3f,
                    resizable = true
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
                }
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
        public static TheDazzles instance;

        [Header("Variables")]
        static List<QueuedPose> queuedPoses = new List<QueuedPose>();
        [Header("Components")]
        [SerializeField] List<TheDazzlesGirl> npcGirls = new List<TheDazzlesGirl>();
        [SerializeField] TheDazzlesGirl player;
        [SerializeField] ParticleSystem poseEffect;
        [SerializeField] ParticleSystem starsEffect;

        void OnDestroy()
        {
            if (queuedPoses.Count > 0) queuedPoses.Clear();
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
                if (queuedPoses.Count > 0)
                {
                    foreach (var pose in queuedPoses)
                    {
                        Pose(pose.beat, pose.length, pose.upLeftBeat, pose.upMiddleBeat, pose.upRightBeat, pose.downLeftBeat, pose.downMiddleBeat, pose.playerBeat, pose.stars);
                    }
                    queuedPoses.Clear();
                }
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    player.Prepare(false);
                }
                if (PlayerInput.PressedUp() && !IsExpectingInputNow(InputType.STANDARD_UP))
                {
                    player.UnPrepare();
                }
            }
            else if (!cond.isPlaying && !cond.isPaused)
            {
                if (queuedPoses.Count > 0) queuedPoses.Clear();
            }
        }

        public void Crouch(float beat)
        {
            ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, JustCrouch, MissCrouch, Nothing);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("theDazzles/hold3", beat),
                new MultiSound.Sound("theDazzles/hold2", beat + 1f),
                new MultiSound.Sound("theDazzles/hold1", beat + 2f),
            }, forcePlay: true);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    npcGirls[1].Prepare();
                    npcGirls[4].Prepare();
                }),
                new BeatAction.Action(beat + 1f, delegate
                {
                    npcGirls[0].Prepare();
                    npcGirls[3].Prepare();
                }),
                new BeatAction.Action(beat + 2f, delegate
                {
                    npcGirls[2].Prepare();
                }),
            });
        }

        public void CrouchStretchable(float beat, float length)
        {
            float actualLength = length / 3;
            ScheduleInput(beat, 2f * actualLength, InputType.STANDARD_DOWN, JustCrouch, MissCrouch, Nothing);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("theDazzles/hold3", beat),
                new MultiSound.Sound("theDazzles/hold2", beat + 1f * actualLength),
                new MultiSound.Sound("theDazzles/hold1", beat + 2f * actualLength),
            }, forcePlay: true);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    npcGirls[1].Prepare();
                    npcGirls[4].Prepare();
                }),
                new BeatAction.Action(beat + 1f * actualLength, delegate
                {
                    npcGirls[0].Prepare();
                    npcGirls[3].Prepare();
                }),
                new BeatAction.Action(beat + 2f * actualLength, delegate
                {
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
                soundsToPlay.Add(new MultiSound.Sound("theDazzles/posePartner", beat + sound));
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
                        girl.Hold();
                    }
                    player.Hold();
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
            posesToDo.Add(new BeatAction.Action(beat + length, delegate
            {
                foreach (var girl in npcGirls)
                {
                    girl.EndPose();
                }
                player.EndPose();
            }));
            BeatAction.New(instance.gameObject, posesToDo);
        }

        void JustCrouch(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SuccessCrouch();
        }

        void SuccessCrouch()
        {
            player.Prepare();
            Jukebox.PlayOneShotGame("theDazzles/crouch");
        }

        void MissCrouch(PlayerActionEvent caller)
        {

        }

        void JustPose(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                player.Pose(false);
                return;
            }
            SuccessPose(false);
        }

        void JustPoseStars(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                player.Pose(false);
                return;
            }
            SuccessPose(true);
        }

        void SuccessPose(bool stars)
        {
            player.Pose();
            Jukebox.PlayOneShotGame("theDazzles/posePlayer" + (stars ? "Stars" : ""));
            if (stars) starsEffect.Play();
            else poseEffect.Play();
        }

        void MissPose(PlayerActionEvent caller)
        {

        }

        void Nothing(PlayerActionEvent caller) { }
    }
}

