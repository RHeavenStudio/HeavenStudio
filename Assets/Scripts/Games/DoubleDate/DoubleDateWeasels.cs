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

        void Awake()
        {
            game = DoubleDate.instance;
            anim = GetComponent<Animator>();
        }

        public void Bop()
        {
            if (canBop)
            {
                anim.DoScaledAnimationAsync("WeaselsBop", 1f);
            }
        }

        public void Happy()
        {
            anim.DoScaledAnimationAsync("WeaselsHappy", 1f);
        }

        public void ToggleBop()
        {
            canBop = !canBop;
        }
    }
}

