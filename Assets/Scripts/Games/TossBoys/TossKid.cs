using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TossBoys
{
    public class TossKid : MonoBehaviour
    {
        [SerializeField] ParticleSystem _hitEffect;
        [SerializeField] GameObject arrow;
        Animator anim;
        [SerializeField] string prefix;
        TossBoys game;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            game = TossBoys.instance;
        }

        public void HitBall(bool hit = true)
        {
            if (hit)
            {
                ParticleSystem spawnedEffect = Instantiate(_hitEffect, transform);
                spawnedEffect.Play();
                DoAnimationScaledAsync("Hit", 0.5f);
            }
            else
            {
                DoAnimationScaledAsync("Whiff", 0.5f);
            }
        }

        public void PopBall()
        {
            DoAnimationScaledAsync("Slap", 0.5f);
        }

        public void PopBallPrepare()
        {
            DoAnimationScaledAsync("PrepareHand", 0.5f);
        }

        public void Miss()
        {
            DoAnimationScaledAsync("Miss", 0.5f);
        }

        public void Barely()
        {
            DoAnimationScaledAsync("Barely", 0.5f);
        }

        public void ShowArrow(float startBeat, float length)
        {
            BeatAction.New(game.gameObject, new List<BeatAction.Action>(){
                new BeatAction.Action(startBeat, delegate { arrow.SetActive(true); }),
                new BeatAction.Action(startBeat + length, delegate { arrow.SetActive(false); }),
            });
        }

        void DoAnimationScaledAsync(string name, float time)
        {
            anim.DoScaledAnimationAsync(prefix + name, time);
        }
    }
}

