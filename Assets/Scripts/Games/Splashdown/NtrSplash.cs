using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_Splashdown
{
    public class NtrSplash : MonoBehaviour
    {
        private Animator anim;
        [SerializeField] private ParticleSystem smallSplashParticles;
        [SerializeField] private ParticleSystem bigSplashParticles;

        public void Init(string animName)
        {
            anim = GetComponent<Animator>();
            anim.DoScaledAnimationAsync(animName, 0.5f);
            switch (animName)
            {
                case "GodownSplash":
                    smallSplashParticles.PlayScaledAsync(0.5f);
                    break;
                case "BigSplash":
                case "Appearsplash":
                    bigSplashParticles.PlayScaledAsync(0.5f);
                    break;
                default:
                    break;
            }
            StartCoroutine(deletionCo());
        }

        private IEnumerator deletionCo()
        {
            while (!anim.IsAnimationNotPlaying() || smallSplashParticles.isPlaying || bigSplashParticles.isPlaying)
            {
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}

