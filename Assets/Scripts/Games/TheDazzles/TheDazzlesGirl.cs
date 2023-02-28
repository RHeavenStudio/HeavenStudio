using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TheDazzles
{
    public class TheDazzlesGirl : MonoBehaviour
    {
        public enum Emotion
        {
            Neutral = 0,
            Happy = 1,
            Angry = 2,
            Ouch = 3
        }
        public bool canBop = true;
        bool holding = false;
        bool preparingPose = false;
        public Emotion currentEmotion;
        Animator anim;
        [SerializeField] Animator holdEffectAnim;
        [SerializeField] SpriteRenderer headSprite;
        [SerializeField] SpriteRenderer mouthSprite;
        [SerializeField] GameObject blackFlash;
        TheDazzles game;

        void Awake()
        {
            anim = GetComponent<Animator>();
            game = TheDazzles.instance;
        }

        public void Prepare(bool hit = true)
        {
            holdEffectAnim.DoScaledAnimationAsync("HoldBox", 0.25f);
            blackFlash.SetActive(true);
            holding = true;
            if (preparingPose) return;
            anim.Play("Prepare", 0, 0);
        }

        public void StartReleaseBox(float beat)
        {
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1f, delegate { holdEffectAnim.DoScaledAnimationAsync("ReleaseBox", 0.25f); })
            });
        }

        public void Pose(bool hit = true)
        {
            anim.DoScaledAnimationAsync("Pose", 0.5f);
            holdEffectAnim.Play("HoldNothing", 0, 0);
            if (hit) currentEmotion = Emotion.Happy;
            holding = false;
            preparingPose = false;
            blackFlash.SetActive(false);
        }

        public void EndPose()
        {
            if (holding) return;
            anim.DoScaledAnimationAsync("EndPose", 0.5f);
        }

        public void Hold()
        {
            if (!holding) return;
            anim.DoScaledAnimationAsync("Hold", 0.5f);
            preparingPose = true;
        }

        public void UnPrepare()
        {
            game.ScoreMiss(1f);
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
            blackFlash.SetActive(false);
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
                    break;
            }
        }
    }
}

