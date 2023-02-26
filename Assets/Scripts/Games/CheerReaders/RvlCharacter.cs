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
        bool bookIsOpen;
        bool noBop;
        CheerReaders game;
        [SerializeField] SpriteRenderer bookLeft;
        [SerializeField] SpriteRenderer bookRight;
        [SerializeField] List<Sprite> bookLeftSprites = new List<Sprite>();
        [SerializeField] List<Sprite> bookRightSprites = new List<Sprite>();
        [SerializeField] List<Sprite> bookLeftMissSprites = new List<Sprite>();
        [SerializeField] List<Sprite> bookRightMissSprites = new List<Sprite>();

        void Awake()
        {
            BaseAnim = GetComponent<Animator>();
            game = CheerReaders.instance;
        }

        public void ResetPose()
        {
            BaseAnim.Play(bookIsWhite ? "WhiteIdle" : "BlackIdle", 0, 0);
        }

        public void SetBookSprites(int whichSprite, bool hit)
        {
            if (hit)
            {
                bookLeft.sprite = bookLeftSprites[whichSprite];
                bookRight.sprite = bookRightSprites[whichSprite];
            }
            else
            {
                bookLeft.sprite = bookLeftMissSprites[whichSprite];
                bookRight.sprite = bookRightMissSprites[whichSprite];
            }
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

        public void FlipBook(bool hit = true)
        {
            if (bookIsWhite != game.shouldBeBlack && hit) 
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
            BaseAnim.DoScaledAnimationAsync(bookIsWhite ? "RepositionToWhite" : "RepositionToBlack", 0.5f);
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

