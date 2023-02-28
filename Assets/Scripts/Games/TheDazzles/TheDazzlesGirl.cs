using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TheDazzles
{
    public class TheDazzlesGirl : MonoBehaviour
    {
        enum Emotion
        {
            Neutral = 0,
            Happy = 1,
            Angry = 2
        }
        Emotion currentEmotion;
        Animator anim;
        [SerializeField] Animator holdEffectAnim;
        [SerializeField] SpriteRenderer headSprite;
        [SerializeField] SpriteRenderer mouthSprite;
        TheDazzles game;

        void Awake()
        {
            anim = GetComponent<Animator>();
            game = TheDazzles.instance;
        }

        public void Prepare(bool hit = true)
        {
            anim.Play("Prepare", 0, 0);
        }

        public void Pose(bool hit = true)
        {
            anim.DoScaledAnimationAsync("Pose", 0.5f);
        }

        public void EndPose()
        {
            anim.DoScaledAnimationAsync("EndPose", 0.5f);
        }

        public void Hold()
        {
            anim.DoScaledAnimationAsync("Hold", 0.5f);
        }

        public void UnPrepare()
        {
            game.ScoreMiss(1f);
            anim.Play("Prepare", 0, 0);
        }

        public void Bop()
        {
            switch (currentEmotion)
            {
                case Emotion.Neutral:
                    anim.DoScaledAnimationAsync("IdleBop", 0.5f);
                    break;
                case Emotion.Happy:
                    anim.DoScaledAnimationAsync("HappyBop", 0.5f);
                    break;
                case Emotion.Angry:
                    break;
            }
        }
    }
}

