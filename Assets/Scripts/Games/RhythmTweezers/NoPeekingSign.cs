using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_RhythmTweezers
{
    public class NoPeekingSign : MonoBehaviour
    {
        private double peekBeat = -1;
        private bool peekRising;
        private bool shouldDelete;
        private Animator anim;
        private SpriteRenderer sr;
        [SerializeField] private Sprite noPeekFullSprite;
        [SerializeField] private Sprite noPeekHalfLeftSprite;
        [SerializeField] private Sprite noPeekHalfRightSprite;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            sr = GetComponent<SpriteRenderer>();
        }

        public void Init(double beat, float length, int type)
        {
            peekBeat = beat - 1;
            peekRising = true;
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate { peekBeat = beat + length; peekRising = false; shouldDelete = true; })
            });
            sr.sprite = type switch
            {
                (int)RhythmTweezers.NoPeekSignType.Full => noPeekFullSprite,
                (int)RhythmTweezers.NoPeekSignType.HalfLeft => noPeekHalfLeftSprite,
                (int)RhythmTweezers.NoPeekSignType.HalfRight => noPeekHalfRightSprite,
                _ => throw new System.NotImplementedException(),
            };
        }

        public void Update()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(peekBeat, 1);
            if (normalizedBeat >= 0f && normalizedBeat <= 1f)
            {
                anim.DoNormalizedAnimation(peekRising ? "NoPeekRise" : "NoPeekLower", normalizedBeat);
            }
            if (normalizedBeat > 1f && !peekRising && shouldDelete)
            {
                Destroy(gameObject);
            }
        }
    }

}

