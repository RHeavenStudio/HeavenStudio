using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MannequinFactory
{
    public class MannequinHead : MonoBehaviour
    {
        public double startBeat;
        public bool needSlap;
        
        [Header("Animators")]
        [SerializeField] SpriteRenderer headSr;
        [SerializeField] Sprite[] heads;
        [SerializeField] SpriteRenderer eyesSr;
        [SerializeField] Sprite[] eyes;
        [SerializeField] Animator headAnim;

        int turnStatus;

        public MannequinFactory game;

        private void Start() 
        {
            game.slapScheduledBeat = startBeat + 4;

            turnStatus = needSlap ? 0 : 1;
            headSr.sprite = heads[turnStatus];

            BeatAction.New(game, new List<BeatAction.Action> {
                new(startBeat + 1, delegate { headAnim.DoScaledAnimationAsync("Move1", 0.3f); }),
                new(startBeat + 3, delegate { if (game.gameManager.autoplay) headAnim.DoScaledAnimationAsync("Move2", 0.3f); }),
                new(startBeat + 4, delegate {
                    PlayerActionEvent input;
                    if (turnStatus == 1) {
                        input = game.ScheduleInput(startBeat, 5, MannequinFactory.InputAction_Second, StampJust, StampMiss, null);
                    } else {
                        input = game.ScheduleUserInput(startBeat, 5, MannequinFactory.InputAction_Second, StampUnJust, StampMiss, null);
                    }
                    input.IsHittable = () => !game.StampAnim.IsPlayingAnimationNames("StampEmpty", "StampJust");
                }),
            });

            PlayerActionEvent input;
            if (needSlap) {
                input = game.ScheduleInput(startBeat, 3, MannequinFactory.InputAction_First, SlapJust, SlapMiss, null);
            } else {
                input = game.ScheduleUserInput(startBeat, 3, MannequinFactory.InputAction_First, SlapUnJust, SlapMiss, null);
            }
            input.IsHittable = () => !game.HandAnim.IsPlayingAnimationNames("SlapEmpty", "SlapJust");
        }

        private void SlapJust(PlayerActionEvent caller, float state)
        {
            SlapHit(state);
            headSr.sprite = heads[turnStatus];
        }

        private void SlapUnJust(PlayerActionEvent caller, float state)
        { 
            eyesSr.transform.localScale = new Vector2(-1, 1);
            headSr.transform.localScale = new Vector2(-1, 1);
            headSr.sprite = heads[0];
            game.ScoreMiss();
            SlapHit(state);
        }

        private void SlapHit(float state)
        {
            if (state >= 1f || state <= -1f) SoundByte.PlayOneShot("nearMiss");
            turnStatus++;
            SoundByte.PlayOneShotGame("mannequinFactory/slap");
            game.HandAnim.DoScaledAnimationAsync("SlapJust", 0.3f);
            headAnim.Play("Slapped", 0, 0);
        }

        private void SlapMiss(PlayerActionEvent caller)
        { 
            headAnim.DoScaledAnimationAsync("Move2", 0.3f);
        }

        private void StampHit(float state)
        {
            if (state >= 1f || state <= -1f) SoundByte.PlayOneShot("nearMiss");
            headAnim.DoScaledAnimationAsync("Stamp", 0.3f);
            game.StampAnim.DoScaledAnimationAsync("StampJust", 0.3f);
            SoundByte.PlayOneShotGame("mannequinFactory/eyes");
            eyesSr.gameObject.SetActive(true);
        }

        private void StampJust(PlayerActionEvent caller, float state)
        {
            StampHit(state);

            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("mannequinFactory/claw1", startBeat + 6),
                new MultiSound.Sound("mannequinFactory/claw2", startBeat + 6.5),
            });
            BeatAction.New(game, new List<BeatAction.Action> {
                new BeatAction.Action(startBeat + 5.75, delegate { headAnim.DoScaledAnimationAsync("Grabbed1", 0.3f); }),
                new BeatAction.Action(startBeat + 6   , delegate { headAnim.DoScaledAnimationAsync("Grabbed2", 0.3f); }),
            });
        }

        private void StampUnJust(PlayerActionEvent caller, float state)
        {
            StampHit(state);
            eyesSr.sprite = eyes[1];

            BeatAction.New(game, new List<BeatAction.Action> {
                new BeatAction.Action(startBeat + 6, delegate {
                    SoundByte.PlayOneShotGame("mannequinFactory/miss");
                    headAnim.DoScaledAnimationAsync("StampMiss", 0.3f);
                }),
            });
        }

        private void StampMiss(PlayerActionEvent caller)
        {
            headAnim.DoScaledAnimationAsync("Move3", 0.3f);
            BeatAction.New(game, new List<BeatAction.Action> {
                new BeatAction.Action(startBeat + 6.5, delegate {
                    SoundByte.PlayOneShotGame("mannequinFactory/miss");
                    headAnim.DoScaledAnimationAsync("Miss", 0.3f);
                }),
            });
        }

        // animation event
        public void DestroySelf()
        {
            Destroy(this);
        }
    }
}
