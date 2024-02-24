using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DoubleDate
{
    public class DoubleDateWeasels : MonoBehaviour
    {
        bool canBop = true;
        Animator anim;
        private DoubleDate game;
        bool notHit = true;
        float lastGacha = float.MinValue;

        void Awake()
        {
            game = DoubleDate.instance;
            anim = GetComponent<Animator>();
        }

        public void Bop()
        {
            if (canBop && notHit && Conductor.instance.songPositionInBeats > lastGacha)
            {
                anim.DoScaledAnimationAsync("WeaselsBop", 0.5f);
            }
        }

        public void Happy()
        {
            if (notHit && Conductor.instance.songPositionInBeats > lastGacha)
                anim.DoScaledAnimationAsync("WeaselsHappy", 0.5f);
        }

        public void Jump()
        {
            if (notHit && Conductor.instance.songPositionInBeats > lastGacha)
            {
                lastGacha = Conductor.instance.songPositionInBeats + 1f;
                anim.DoScaledAnimationAsync("WeaselsJump", 0.5f);
            }
        }

        public void Hide(double beat)
        {
            if (notHit)
            {
                notHit = false;
                anim.DoScaledAnimationAsync("WeaselsHide", 0.5f);
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 1.45f, delegate
                    {
                        lastGacha = Conductor.instance.songPositionInBeats + 0.5f;
                        anim.DoScaledAnimationAsync("WeaselsAppearUpset", 0.5f);
                        notHit = true;
                    }),
                });
            }
        }

        public void Surprise()
        {
            if (notHit && Conductor.instance.songPositionInBeats > lastGacha)
            {
                lastGacha = Conductor.instance.songPositionInBeats + 0.5f;
                anim.DoScaledAnimationAsync("WeaselsSurprised", 0.5f);
            }
        }

        public void Hit(double beat)
        {
            if (notHit)
            {
                notHit = false;
                anim.DoScaledAnimationAsync("WeaselsHit", 0.5f);
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2f, delegate
                    {
                        lastGacha = Conductor.instance.songPositionInBeats + 0.5f;
                        anim.DoScaledAnimationAsync("WeaselsAppearUpset", 1f);
                        notHit = true;
                    }),
                });
            }
        }

        public void ToggleBop()
        {
            canBop = !canBop;
        }
    }
}

