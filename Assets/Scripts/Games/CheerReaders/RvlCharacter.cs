using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_CheerReaders
{
    public class RvlCharacter : MonoBehaviour
    {
        public Animator BaseAnim;
        bool bookIsWhite = true;

        void Awake()
        {
            BaseAnim = GetComponent<Animator>();
        }

        public void FlipBook()
        {
            BaseAnim.DoScaledAnimationAsync(bookIsWhite ? "WhitetoBlack" : "BlacktoWhite", 0.5f);
            bookIsWhite = !bookIsWhite;
        }
    }
}

