using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TapTroupe
{
    public class TapTroupeTapper : MonoBehaviour
    {
        private Animator anim;

        private TapTroupe game;

        void Awake()
        {
            game = TapTroupe.instance;
            anim = GetComponent<Animator>();
        }

        public void Bop()
        {
            anim.DoScaledAnimationAsync("BopFeet", 0.3f);
        }
    }
}


