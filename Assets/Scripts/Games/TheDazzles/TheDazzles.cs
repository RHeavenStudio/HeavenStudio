using System.Collections;
using System.Linq;
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
                new GameAction("poseThree", "Pose Horizontal")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TheDazzles.PrePose(e.beat, e.length, 0f, 1f, 2f, 0f, 1f, 2f, false); },
                    defaultLength = 3f
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
            List<float> poseBeats = soundBeats;
            poseBeats.Sort();
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
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1f, delegate
                {
                    foreach (var girl in npcGirls)
                    {
                        girl.Hold();
                    }
                    player.Hold();
                }),
                new BeatAction.Action(beat + downMiddleBeat, delegate
                {
                    npcGirls[0].Pose();
                }),
                new BeatAction.Action(beat + downLeftBeat, delegate
                {
                    npcGirls[1].Pose();
                }),
                new BeatAction.Action(beat + upRightBeat, delegate
                {
                    npcGirls[2].Pose();
                }),
                new BeatAction.Action(beat + upMiddleBeat, delegate
                {
                    npcGirls[3].Pose();
                }),
                new BeatAction.Action(beat + upLeftBeat, delegate
                {
                    npcGirls[4].Pose();
                }),
                new BeatAction.Action(beat + length, delegate
                {
                    foreach (var girl in npcGirls)
                    {
                        girl.EndPose();
                    }
                    player.EndPose();
                })
            });
        }

        public void PoseTwo(float beat)
        {
            ScheduleInput(beat, 2f, InputType.STANDARD_UP, JustPoseStars, MissPose, Nothing);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("theDazzles/posePartner", beat),
            }, forcePlay: true);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1f, delegate
                {
                    foreach (var girl in npcGirls)
                    {
                        girl.Hold();
                    }
                    player.Hold();
                }),
                new BeatAction.Action(beat, delegate
                {
                    npcGirls[2].Pose();
                    npcGirls[3].Pose();
                    npcGirls[4].Pose();
                }),
                new BeatAction.Action(beat + 2f, delegate
                {
                    npcGirls[0].Pose();
                    npcGirls[1].Pose();
                }),
                new BeatAction.Action(beat + 4f, delegate
                {
                    foreach (var girl in npcGirls)
                    {
                        girl.EndPose();
                    }
                    player.EndPose();
                })
            });
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
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.timer + caller.startBeat + 1f, delegate
                {
                    player.EndPose();
                })
            });
            if (state >= 1f || state <= -1f)
            {
                player.Pose(false);
                return;
            }
            SuccessPose(false);
        }

        void JustPoseStars(PlayerActionEvent caller, float state)
        {
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.timer + caller.startBeat + 1f, delegate
                {
                    player.EndPose();
                })
            });
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
            poseEffect.Play();
            Jukebox.PlayOneShotGame("theDazzles/posePlayer" + (stars ? "Stars" : ""));
            if (stars) starsEffect.Play();
        }

        void MissPose(PlayerActionEvent caller)
        {

        }

        void Nothing(PlayerActionEvent caller) { }
    }
}

