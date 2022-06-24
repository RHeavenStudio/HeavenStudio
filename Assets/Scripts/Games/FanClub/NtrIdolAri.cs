using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NtrIdolAri : MonoBehaviour
{
    [Header("Objects")]
    public ParticleSystem idolClapEffect;
    public ParticleSystem idolWinkEffect;
    public ParticleSystem idolKissEffect;
    public ParticleSystem idolWinkArrEffect;

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
}
