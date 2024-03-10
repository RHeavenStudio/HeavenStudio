using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_Cannery
{
    public class Can : MonoBehaviour
    {
        public double startBeat;

        [Header("Components")]
        [SerializeField] Animator anim;

        public Cannery game;

        private void Awake()
        {
            if (Random.Range(0, 1f) >= 0.5f) anim.Play("Flip", 0);

            game.ScheduleInput(startBeat, 1, Minigame.InputAction_BasicPress, Hit, null, null);
        }

        private void Update()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, 2);
            if (normalizedBeat > 1) Destroy(gameObject);
            anim.DoNormalizedAnimation("Move", normalizedBeat, 0);
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("cannery/can");
            anim.DoScaledAnimationAsync("Can", 0.5f, 0, 1);
            if (state is >= 1 or <= -1) {
                SoundByte.PlayOneShot("miss");
                game.cannerAnim.DoScaledAnimationAsync("CanBarely", 0.5f);
                double beat = caller.startBeat + caller.timer;
                BeatAction.New(this, new() {
                    new(beat + 0.35f, () => anim.DoScaledAnimationAsync("Reopen", 0.5f, 0, 1))
                });
            } else {
                game.cannerAnim.DoScaledAnimationAsync("Can", 0.5f);
            }
        }
    }
}
