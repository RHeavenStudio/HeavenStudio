using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
namespace HeavenStudio.Common
{
    public class SkillStarManager : MonoBehaviour
    {
        public static SkillStarManager instance { get; private set; }
        [SerializeField] private Animator starAnim;
        [SerializeField] private ParticleSystem starParticle;

        float starStart = float.MaxValue;
        float starLength = float.MaxValue;
        bool starObtained = false;

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (Conductor.instance.songPositionInBeats > starStart && !starObtained)
            {
                starAnim.DoScaledAnimation("StarIn", starStart, starLength);
            }
        }

        public void DoStarIn(float beat, float length)
        {
            starObtained = false;
            starStart = beat;
            starLength = length;
            starAnim.DoScaledAnimation("StarIn", beat, length);
        }

        public void DoStarJust()
        {
            starObtained = true;
            starAnim.Play("StarJust", -1, 0f);
            starParticle.Play();
            Jukebox.PlayOneShot("skillStar");
        }
    }
}