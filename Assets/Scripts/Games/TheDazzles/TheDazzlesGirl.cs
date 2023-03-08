using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TheDazzles
{
    public class TheDazzlesGirl : MonoBehaviour
    {
        [SerializeField] List<Sprite> headSpriteEmotions = new List<Sprite>();
        public enum Emotion
        {
            Neutral = 0,
            Happy = 1,
            Angry = 2,
            Ouch = 3
        }
        public enum Expression
        {
            Neutral = 0,
            Happy = 1,
            Angry = 2,
            Ouch = 3,
            OpenMouth = 4
        }
        public bool canBop = true;
        bool holding = false;
        bool preparingPose = false;
        public bool hasOuched;
        public Emotion currentEmotion;
        Animator anim;
        [SerializeField] Animator lightingAnim;
        [SerializeField] Animator holdEffectAnim;
        [SerializeField] SpriteRenderer headSprite;
        [SerializeField] GameObject blackFlash;
        [SerializeField] List<SpriteRenderer> bodyParts = new List<SpriteRenderer>();
        TheDazzles game;

        void Awake()
        {
            anim = GetComponent<Animator>();
            game = TheDazzles.instance;
        }

        public void PickHead(Expression expression)
        {
            headSprite.sprite = headSpriteEmotions[(int)expression];
        }

        public void Prepare(bool hit = true)
        {
            if (hit)
            {
                holdEffectAnim.DoScaledAnimationAsync("HoldBox", 0.25f);
                blackFlash.SetActive(true);
                lightingAnim.Play("Dark", 0, 0);
            }
            holding = true;
            hasOuched = false;
            if (preparingPose)
            {
                Hold();
                return;
            }
            anim.Play("Prepare", 0, 0);
        }

        public void StartReleaseBox(float beat)
        {
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1f, delegate {if (holding) holdEffectAnim.DoScaledAnimationAsync("ReleaseBox", 0.25f);})
            });
        }

        public void Pose(bool hit = true)
        {
            if (hit)
            {
                anim.DoScaledAnimationAsync("Pose", 0.5f);
                lightingAnim.DoScaledAnimationAsync("PoseFlash", 0.5f);
                hasOuched = false;
            }
            else
            {
                anim.DoScaledAnimationAsync("MissPose", 0.5f);
                lightingAnim.DoScaledAnimationAsync("MissFlash", 0.5f);
                currentEmotion = Emotion.Ouch;
                hasOuched = true;
            }
            holdEffectAnim.Play("HoldNothing", 0, 0);
            holding = false;
            preparingPose = false;
            blackFlash.SetActive(false);
        }

        public void EndPose()
        {
            if (holding || hasOuched) return;
            anim.DoScaledAnimationAsync("EndPose", 0.5f);
        }

        public void Hold()
        {
            preparingPose = true;
            if (!holding) return;
            anim.DoScaledAnimationAsync("Hold", 0.5f);
        }

        public void Ouch()
        {
            anim.DoScaledAnimationAsync("Ouch", 0.5f);
            currentEmotion = Emotion.Ouch;
            hasOuched = true;
        }

        public void UnPrepare()
        {
            game.ScoreMiss(1f);
            holdEffectAnim.Play("HoldNothing", 0, 0);
            canBop = true;
            if (preparingPose)
            {
                anim.DoScaledAnimationAsync("StopHold", 0.5f);
            }
            else
            {
                anim.DoScaledAnimationAsync("EndPrepare", 0.5f);
            }
            holding = false;
            preparingPose = false;
            hasOuched = true;
            blackFlash.SetActive(false);
            lightingAnim.Play("Lit", 0, 0);
        }

        public void Bop()
        {
            if (!canBop || holding) return;
            switch (currentEmotion)
            {
                case Emotion.Neutral:
                    anim.DoScaledAnimationAsync("IdleBop", 0.4f);
                    break;
                case Emotion.Happy:
                    anim.DoScaledAnimationAsync("HappyBop", 0.4f);
                    break;
                case Emotion.Angry:
                    PickHead(Expression.Angry);
                    anim.DoScaledAnimationAsync("IdleBop", 0.4f);
                    break;
                case Emotion.Ouch:
                    anim.DoScaledAnimationAsync("OuchBop", 0.4f);
                    break;
            }
        }
    }
}

