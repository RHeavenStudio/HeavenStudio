using System.Collections;
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
                    function = delegate { TheDazzles.instance.PoseThree(eventCaller.currentEntity.beat); },
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
        public static TheDazzles instance;

        [Header("Components")]
        [SerializeField] List<TheDazzlesGirl> npcGirls = new List<TheDazzlesGirl>();
        [SerializeField] TheDazzlesGirl player;
        [SerializeField] ParticleSystem poseEffect;
        [SerializeField] ParticleSystem starsEffect;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    player.Prepare(false);
                }
                if (PlayerInput.PressedUp() && !IsExpectingInputNow(InputType.STANDARD_UP))
                {
                    player.UnPrepare();
                }
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

        public void PoseThree(float beat)
        {
            ScheduleInput(beat, 2f, InputType.STANDARD_UP, JustPose, MissPose, Nothing);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("theDazzles/posePartner", beat),
                new MultiSound.Sound("theDazzles/posePartner", beat + 1f),
            }, forcePlay: true);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    npcGirls[1].Pose();
                    npcGirls[4].Pose();
                }),
                new BeatAction.Action(beat + 1f, delegate
                {
                    npcGirls[0].Pose();
                    npcGirls[3].Pose();
                }),
                new BeatAction.Action(beat + 2f, delegate
                {
                    npcGirls[2].Pose();
                }),
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
            if (state >= 1f || state <= -1f)
            {
                player.Pose(false);
                return;
            }
            SuccessPose(false);
        }

        void SuccessPose(bool stars)
        {
            player.Pose();
            poseEffect.Play();
            Jukebox.PlayOneShotGame("theDazzles/posePlayer");
            if (stars) starsEffect.Play();
        }

        void MissPose(PlayerActionEvent caller)
        {

        }

        void Nothing(PlayerActionEvent caller) { }
    }
}

