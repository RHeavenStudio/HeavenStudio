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
        [SerializeField] Animator anim;
        [SerializeField] SpriteRenderer sr;

        [Header("Sprites")]
        [SerializeField] Sprite[] canSprites;
        [SerializeField] Sprite cannedSprite;

        Cannery game;

        private void Awake()
        {
            game = Cannery.instance;
            int random = Random.Range(0, 2);
            Debug.Log(random);
            sr.sprite = canSprites[random];

            game.ScheduleInput(startBeat, 1, Minigame.InputAction_BasicPress, Hit, null, null);
        }

        private void Update()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, 2);
            Debug.Log(normalizedBeat);
            if (normalizedBeat > 1) Destroy(gameObject);
            anim.DoNormalizedAnimation("Move", normalizedBeat);
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            game.cannerAnim.DoScaledAnimationAsync("Can", 0.5f);
            SoundByte.PlayOneShotGame("cannery/can");
            sr.sprite = cannedSprite;
        }
    }
}
