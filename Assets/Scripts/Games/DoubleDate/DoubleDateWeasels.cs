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

        void Awake()
        {
            game = DoubleDate.instance;
            anim = GetComponent<Animator>();
        }

        public void Bop()
        {
            if (canBop && notHit)
            {
                anim.DoScaledAnimationAsync("WeaselsBop", 1f);
            }
        }

        public void Happy()
        {
            anim.DoScaledAnimationAsync("WeaselsHappy", 0.5f);
        }

        public void Hit(float beat)
        {
            notHit = false;
            anim.DoScaledAnimationAsync("WeaselsHit", 0.5f);
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 4f, delegate
                {
                    anim.DoScaledAnimationAsync("WeaselsAppearUpset", 1f);
                }),
                new BeatAction.Action(beat + 4.5f, delegate
                {
                    notHit = true;
                }),
            });
        }

        public void ToggleBop()
        {
            canBop = !canBop;
        }
    }
}

