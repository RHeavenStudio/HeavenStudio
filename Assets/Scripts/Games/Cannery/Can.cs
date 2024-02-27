using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_Cannery
{
    public class Can : MonoBehaviour
    {
        public double startBeat;

        [Header("Components")]
        // [SerializeField] Animator parentAnim;
        [SerializeField] Animator anim;
        // [SerializeField] SpriteRenderer sr;

        public Cannery game;

        private void Awake()
        {
            int random = Random.Range(-1, 1);
            var pos = transform.position;
            pos.x *= random;

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
            game.cannerAnim.DoScaledAnimationAsync("Can", 0.5f);
            SoundByte.PlayOneShotGame("cannery/can");
            anim.DoScaledAnimationAsync("CanCan", 0.5f, 0, 1);
        }
    }
}
