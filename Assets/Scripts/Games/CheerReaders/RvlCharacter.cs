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
        [SerializeField] GameObject blushLeft;
        [SerializeField] GameObject blushRight;
        public bool bookIsWhite = true;
        public GameObject posterBook;
        bool bookIsOpen;
        bool noBop;
        bool canOpenBook;
        public bool player;
        float currentBlushBeat;
        bool missed;
        [SerializeField] bool shouldLookDown;
        CheerReaders game;

        void Awake()
        {
            BaseAnim = GetComponent<Animator>();
            game = CheerReaders.instance;
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                float normalizedBeat = cond.GetPositionFromBeat(currentBlushBeat, 3f);
                if (normalizedBeat > 1f)
                {
                    blushLeft.SetActive(false);
                    blushRight.SetActive(false);
                }
                if (!game.doingCue && missed)
                {
                    faceAnim.Play("FaceBlush", 0, 0);
                }
            }
        }

        public void HappyFace(bool forceIt = false)
        {
            if (game.doingCue && !forceIt) return;
            faceAnim.Play(player ? "FaceItsOnHappy" : "FaceItsOnNPC", 0, 0);
        }

        public void Yay()
        {
            faceAnim.DoScaledAnimationAsync("FaceYay", 0.5f);
            BaseAnim.DoScaledAnimationAsync(bookIsWhite ? "WhiteYay" : "BlackYay", 0.5f);
        }

        public void ResetFace()
        {
            if (faceAnim.IsAnimationNotPlaying()) faceAnim.Play("FaceIdle", 0, 0);
        }

        public void ResetPose()
        {
            BaseAnim.Play(bookIsWhite ? "WhiteIdle" : "BlackIdle", 0, 0);
            ResetFace();
        }

        public void Boom()
        {
            faceAnim.DoScaledAnimationAsync(player ? "FaceBoom" : "FaceBoomNPC", 0.5f);
        }

        public void ItsUpToYou(int count)
        {
            switch (count)
            {
                case 1:
                    faceAnim.DoScaledAnimationAsync("FaceIts", 0.5f);
                    break;
                case 2:
                    faceAnim.DoScaledAnimationAsync("FaceUp", 0.5f);
                    break;
                case 3:
                    faceAnim.DoScaledAnimationAsync("FaceTo", 0.5f);
                    break;
                case 4:
                    faceAnim.DoScaledAnimationAsync("FaceYou", 0.5f);
                    break;
                default:
                    faceAnim.DoScaledAnimationAsync("FaceIts", 0.5f);
                    break;
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
            currentBlushBeat = Conductor.instance.songPositionInBeats;
            blushLeft.SetActive(true);
            blushRight.SetActive(true);
            BaseAnim.Play(bookIsWhite ? "MissWhite" : "MissBlack", 0, 0);
            noBop = true;
            missed = true;
        }

        public void Stare()
        {
            faceAnim.Play(shouldLookDown ? "FaceNPCLook2" : "FaceNPCLook1", 0, 0);
        }

        public void FlipBook(bool hit = true)
        {
            posterBook.SetActive(false);
            if (hit) canOpenBook = false;
            if (bookIsWhite != game.shouldBeBlack && hit && player) 
            {
                RepositionBook(); 
                return;
            } 
            BaseAnim.DoScaledAnimationAsync(bookIsWhite ? "WhitetoBlack" : "BlacktoWhite", 0.5f);
            bookIsWhite = !bookIsWhite;
            bookIsOpen = true;
            noBop = false;
            missed = !hit;
        }

        public void RepositionBook()
        {
            BaseAnim.DoScaledAnimationAsync(bookIsWhite ? "RepositionToWhite" : "RepositionToBlack", 0.5f);
            bookIsOpen = true;
            noBop = false;
        }

        public void StartSpinBook()
        {
            posterBook.SetActive(false);
            canOpenBook = true;
            BaseAnim.DoScaledAnimationAsync(bookIsWhite ? "SpinfromWhite" : "SpinfromBlack", 0.5f);
            bookIsOpen = true;
            noBop = false;
        }

        public void StopSpinBook()
        {
            if (!canOpenBook) return;
            posterBook.SetActive(true);
            BaseAnim.DoScaledAnimationAsync("OpenBook", 0.5f);
            bookIsOpen = true;
            noBop = false;
        }
    }
}

