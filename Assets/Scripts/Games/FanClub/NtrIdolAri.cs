using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_FanClub
{
    public class NtrIdolAri : MonoBehaviour
    {
        [Header("Objects")]
        public ParticleSystem idolClapEffect;
        public ParticleSystem idolWinkEffect;
        public ParticleSystem idolKissEffect;
        public ParticleSystem idolWinkArrEffect;

        [Header("References")]
        public Material coreMat;

        public void ClapParticle()
        {
            idolClapEffect.Play();
        }

        public void WinkParticle()
        {
            idolWinkEffect.Play();
        }

        public void KissParticle()
        {
            idolKissEffect.Play();
        }

        public void WinkArrangeParticle()
        {
            idolWinkArrEffect.Play();
        }

        public void ToSpot(bool unspot = true)
        {
            if (unspot)
                coreMat.SetColor("_AddColor", new Color(0, 0, 0, 0));
            else
                coreMat.SetColor("_AddColor", new Color(0, 100 / 255f, 200 / 255f, 0));
        }
    }
}