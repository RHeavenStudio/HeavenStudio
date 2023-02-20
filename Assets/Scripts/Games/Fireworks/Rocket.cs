using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_Fireworks
{
    public class Rocket : PlayerActionObject
    {
        [SerializeField] ParticleSystem particleEffect;
        public bool isSparkler;
        private Fireworks game;

        void Awake()
        {
            game = Fireworks.instance;
        }

        public void Init(float beat)
        {
            if (isSparkler) Jukebox.PlayOneShotGame("fireworks/sparkler");
            else Jukebox.PlayOneShotGame("fireworks/rocket");
            game.ScheduleInput(beat, isSparkler ? 1f : 3f, InputType.STANDARD_DOWN, Just, Out, Out);
        }

        void Update()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + (isSparkler ? 0.05f : 0.01f), transform.position.z);
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            Success();
        }

        void Success()
        {
            Jukebox.PlayOneShotGame("fireworks/explodeRocket");
            Instantiate(particleEffect.gameObject, transform, false);
            Destroy(gameObject);
        }

        void Out(PlayerActionEvent caller) { }
    }
}
