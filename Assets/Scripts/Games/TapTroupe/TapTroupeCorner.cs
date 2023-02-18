using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TapTroupe
{
    public class TapTroupeCorner : MonoBehaviour
    {
        private Animator anim;
        [SerializeField] Animator expressionAnim;

        private TapTroupe game;

        void Awake()
        {
            game = TapTroupe.instance;
            anim = GetComponent<Animator>();
        }

        public void Bop()
        {
            anim.DoScaledAnimationAsync("Bop", 0.3f);
        }

        public void Okay()
        {
            expressionAnim.DoScaledAnimationAsync("Okay", 0.25f);
        }
    }
}


