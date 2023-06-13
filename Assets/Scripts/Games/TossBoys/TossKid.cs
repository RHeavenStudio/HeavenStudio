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
        bool preparing;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            game = TossBoys.instance;
            DoAnimationScaledAsync("Idle", 20f);
        }

        public void HitBall(bool hit = true)
        {
            if (hit)
            {
                ParticleSystem spawnedEffect = Instantiate(_hitEffect, transform);
                spawnedEffect.Play();
                DoAnimationScaledAsync(crouch ? "CrouchHit" : "Hit", 0.5f);
            }
            else if (!anim.IsPlayingAnimationName(prefix + "Whiff") && !anim.IsPlayingAnimationName(prefix + "Miss"))
            {
                DoAnimationScaledAsync("Whiff", 0.5f);
                SoundByte.PlayOneShotGame("tossBoys/whiff");
            }
        }

        public void Bop()
        {
            if (!anim.IsAnimationNotPlaying() || crouch || preparing) return;
            DoAnimationScaledAsync("Bop", 0.5f);
        }

        public void Crouch()
        {
            DoAnimationScaledAsync("Crouch", 0.5f);
            crouch = true;
        }

        public void PopBall()
        {
            DoAnimationScaledAsync("Slap", 0.5f);
            preparing = false;
        }

        public void PopBallPrepare()
        {
            if (preparing) return;
            DoAnimationScaledAsync("PrepareHand", 0.5f);
            preparing = true;
        }

        public void Miss()
        {
            DoAnimationScaledAsync("Miss", 0.5f);
        }

        public void Barely()
        {
            DoAnimationScaledAsync("Barely", 0.5f);
        }

        public void ShowArrow(double startBeat, float length)
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

