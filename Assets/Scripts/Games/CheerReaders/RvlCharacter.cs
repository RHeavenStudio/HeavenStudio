using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_CheerReaders
{
    public class RvlCharacter : MonoBehaviour
    {
        Animator BaseAnim;
        [SerializeField] Animator faceAnim;
        public bool bookIsWhite = true;
        public bool shouldReposition;
        bool bookIsOpen;
        bool noBop;

        void Awake()
        {
            BaseAnim = GetComponent<Animator>();
        }

        public void OneTwoThree(int count)
        {
            switch (count)
            {
                case 1:
                    faceAnim.DoScaledAnimationAsync("FaceOne", 0.5f);
                    break;
                case 2:
                    faceAnim.DoScaledAnimationAsync("FaceTwo", 0.5f);
                    break;
                case 3:
                    faceAnim.DoScaledAnimationAsync("FaceThree", 0.5f);
                    break;
                default:
                    faceAnim.DoScaledAnimationAsync("FaceOne", 0.5f);
                    break;
            }
        }

        public void Bop()
        {
            if (noBop) return;
            if (bookIsOpen)
            {
                BaseAnim.DoScaledAnimationAsync(bookIsWhite ? "WhiteBop" : "BlackBop", 0.5f);
            }
            else
            {
                BaseAnim.DoScaledAnimationAsync("Bop", 0.5f);
            }
        }

        public void Miss()
        {
            BaseAnim.Play(bookIsWhite ? "MissWhite" : "MissBlack", 0, 0);
            noBop = true;
        }

        public void FlipBook()
        {
            if (shouldReposition) 
            {
                RepositionBook(); 
                return;
            } 
            BaseAnim.DoScaledAnimationAsync(bookIsWhite ? "WhitetoBlack" : "BlacktoWhite", 0.5f);
            bookIsWhite = !bookIsWhite;
            bookIsOpen = true;
            noBop = false;
        }

        public void RepositionBook()
        {
            BaseAnim.DoScaledAnimationAsync(bookIsWhite ? "RepositiontoBlack" : "RepositiontoWhite", 0.5f);
            shouldReposition = false;
            bookIsOpen = true;
            noBop = false;
        }

        public void StartSpinBook()
        {
            BaseAnim.DoScaledAnimationAsync(bookIsWhite ? "SpinfromWhite" : "SpinfromBlack", 0.5f);
            bookIsOpen = true;
            noBop = false;
        }

        public void StopSpinBook()
        {
            BaseAnim.DoScaledAnimationAsync("OpenBook", 0.5f);
            bookIsOpen = true;
            noBop = false;
        }
    }
}

