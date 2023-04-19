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
        public bool crouch;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            game = TossBoys.instance;
        }

        public void HitBall(bool hit = true, bool uncrouch = false)
        {
            if (uncrouch) crouch = false;
            if (hit)
            {
                ParticleSystem spawnedEffect = Instantiate(_hitEffect, transform);
                spawnedEffect.Play();
                DoAnimationScaledAsync(crouch ? "CrouchHit" : "Hit", 0.5f);
            }
            else
            {
                DoAnimationScaledAsync("Whiff", 0.5f);
            }
        }

        public void Crouch()
        {
            DoAnimationScaledAsync("Crouch", 0.5f);
            crouch = true;
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

        public void Barely(bool uncrouch = false)
        {
            if (uncrouch) crouch = false;
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

