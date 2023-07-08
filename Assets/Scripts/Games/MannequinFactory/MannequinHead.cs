using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MannequinFactory
{
    public class MannequinHead : MonoBehaviour
    {
        public double startBeat;
        public bool needClap;
        
        [Header("Animators")]
        [SerializeField] SpriteRenderer headSr;
        [SerializeField] Sprite[] heads;
        [SerializeField] SpriteRenderer eyesSr;
        [SerializeField] Sprite[] eyes;
        [SerializeField] Animator headAnim;

        int turnStatus;

        private MannequinFactory game;
        
        private void Awake()
        {
            game = MannequinFactory.instance;
        }

        private void Start() 
        {
            turnStatus = needClap ? 0 : 1;
            headSr.sprite = heads[turnStatus];

            BeatAction.New(gameObject, new List<BeatAction.Action> {
                new BeatAction.Action(startBeat + 1, delegate { headAnim.DoScaledAnimationAsync("Move1", 0.3f); }),
                new BeatAction.Action(startBeat + 3, delegate { headAnim.DoScaledAnimationAsync("Move2", 0.3f); }),
                new BeatAction.Action(startBeat + 4, delegate {
                    if (turnStatus == 1) game.ScheduleInput(startBeat, 5, InputType.STANDARD_DOWN, StampJust, StampMiss, Nothing);
                    else game.ScheduleUserInput(startBeat, 5, InputType.STANDARD_DOWN, StampUnJust, StampMiss, Nothing);
                }),
            });
            if (needClap) SoundByte.PlayOneShotGame("mannequinFactory/whoosh", beat: startBeat + 3);
            SoundByte.PlayOneShotGame("mannequinFactory/whoosh", beat: startBeat + 5);

            if (needClap) game.ScheduleInput(startBeat, 3, InputType.DIRECTION_LEFT_DOWN, ClapJust, ClapMiss, Nothing);
            else      game.ScheduleUserInput(startBeat, 3, InputType.DIRECTION_LEFT_DOWN, ClapUnJust, ClapMiss, Nothing);
        }

        void ClapJust(PlayerActionEvent caller, float state)
        {
            ClapHit(state);
            headSr.sprite = heads[turnStatus];
        }

        void ClapUnJust(PlayerActionEvent caller, float state)
        { 
            eyesSr.transform.localScale = new Vector2(-1, 1);
            headSr.transform.localScale = new Vector2(-1, 1);
            headSr.sprite = heads[0];
            game.ScoreMiss();
            ClapHit(state);
        }

        void ClapHit(float state)
        {
            turnStatus++;
            SoundByte.PlayOneShotGame("mannequinFactory/slap");
            game.HandAnim.DoScaledAnimationAsync("SlapJust");
            headAnim.Play("Slapped", 0, 0);
        }

        void ClapMiss(PlayerActionEvent caller) { }

        void StampHit(float state)
        {
            if (state >= 1f || state <= -1f) SoundByte.PlayOneShot("nearMiss");
            headAnim.DoScaledAnimationAsync("Stamp", 0.3f);
            game.StampAnim.DoScaledAnimationAsync("StampJust", 0.2f);
            SoundByte.PlayOneShotGame("mannequinFactory/eyes");
            eyesSr.gameObject.SetActive(true);
        }

        void StampJust(PlayerActionEvent caller, float state)
        {
            StampHit(state);

            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("mannequinFactory/claw1", startBeat + 6),
                new MultiSound.Sound("mannequinFactory/claw2", startBeat + 6.5),
            });
            BeatAction.New(gameObject, new List<BeatAction.Action> {
                new BeatAction.Action(startBeat + 5.75, delegate { headAnim.DoScaledAnimationAsync("Grabbed1", 0.3f); }),
                new BeatAction.Action(startBeat + 6   , delegate { headAnim.DoScaledAnimationAsync("Grabbed2", 0.3f); }),
            });
        }

        void StampUnJust(PlayerActionEvent caller, float state)
        {
            StampHit(state);
            eyesSr.sprite = eyes[1];

            BeatAction.New(gameObject, new List<BeatAction.Action> {
                new BeatAction.Action(startBeat + 6, delegate {
                    SoundByte.PlayOneShotGame("mannequinFactory/miss");
                    headAnim.DoScaledAnimationAsync("StampMiss", 0.3f);
                }),
            });
        }

        void StampMiss(PlayerActionEvent caller)
        {
            headAnim.DoScaledAnimationAsync("Move3", 0.3f);
            BeatAction.New(gameObject, new List<BeatAction.Action> {
                new BeatAction.Action(startBeat + 6.5, delegate {
                    SoundByte.PlayOneShotGame("mannequinFactory/miss");
                    headAnim.DoScaledAnimationAsync("Miss", 0.3f);
                }),
            });
        }

        void Nothing(PlayerActionEvent caller) { }

        // animation event
        public void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
