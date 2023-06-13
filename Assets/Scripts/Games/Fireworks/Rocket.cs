using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_Fireworks
{
    public class Rocket : MonoBehaviour
    {
        [SerializeField] ParticleSystem particleBarelyEffect;
        [SerializeField] private List<ParticleSystem> particleEffects = new List<ParticleSystem>();
        [SerializeField] ParticleSystem selectedParticleEffect;
        [SerializeField] Animator anim;
        public bool isSparkler;
        private Fireworks game;
        public double startBeat;
        public bool applause;
        private bool exploded;
        private float startY;
        public float offSet;
        [SerializeField] List<ParticleSystem> mixedCircularPS = new List<ParticleSystem>();
        [SerializeField] Sprite GreenOne;
        [SerializeField] Sprite GreenTwo;
        [SerializeField] Sprite BlueOne;
        [SerializeField] Sprite BlueTwo;
        [SerializeField] Sprite RedOne;
        [SerializeField] Sprite RedTwo;

        void Awake()
        {
            game = Fireworks.instance;
            List<string> colors = new List<string>()
            {
                "Green",
                "Red",
                "Blue"
            };
            for (int i = 0; i < mixedCircularPS.Count; i++)
            {
                var ts = mixedCircularPS[i].textureSheetAnimation;
                var pickedColor = colors[UnityEngine.Random.Range(0, colors.Count)];
                switch (pickedColor)
                {
                    case "Green":
                        ts.AddSprite(GreenOne);
                        ts.AddSprite(GreenTwo);
                        break;
                    case "Red":
                        ts.AddSprite(RedOne);
                        ts.AddSprite(RedTwo);
                        break;
                    case "Blue":
                        ts.AddSprite(BlueOne);
                        ts.AddSprite(BlueTwo);
                        break;
                    default:
                        ts.AddSprite(GreenOne);
                        ts.AddSprite(GreenTwo);
                        break;
                }
                colors.Remove(pickedColor);
            }
        }

        public void Init(double beat, int explosionToChoose)
        {
            startBeat = beat;
            startY = transform.position.y - offSet;
            game.ScheduleInput(beat, isSparkler ? 1f : 3f, InputType.STANDARD_DOWN, Just, Out, Out);
            anim.DoScaledAnimationAsync(isSparkler ? "Sparkler" : "Rocket", isSparkler ? 1f : 0.5f);
            selectedParticleEffect = particleEffects[explosionToChoose];
        }

        void Update()
        {
            var cond = Conductor.instance;
            float normalizedBeat = cond.GetPositionFromBeat(startBeat, isSparkler ? 1f : 3f);
            if (!exploded && cond.isPlaying && !cond.isPaused) 
            {
                EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.Linear);
                float newPosY = func(startY, 7f - offSet, normalizedBeat * (isSparkler ? 0.5f : 0.4f));
                transform.position = new Vector3(transform.position.x, newPosY, transform.position.z);
            } 
            if (normalizedBeat > 3f && !selectedParticleEffect.isPlaying) Destroy(gameObject);
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("fireworks/miss");
                particleBarelyEffect.Play();
                anim.gameObject.SetActive(false);
                return;
            }
            Success(caller);
        }

        void Success(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("fireworks/explode_5");
            selectedParticleEffect.Play();
            anim.gameObject.SetActive(false);
            if (applause) SoundByte.PlayOneShot("applause", caller.timer + caller.startBeat + 1f);
        }

        void Out(PlayerActionEvent caller) { }
    }
}
