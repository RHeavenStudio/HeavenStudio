using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Starpelly;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DrummingPractice
{
    public class Drummer : MonoBehaviour
    {
        DrummingPractice game;

        [Header("References")]
        public Animator animator;
        public List<MiiFace> miiFaces;
        public SpriteRenderer face;

        public bool player = false;

        public int mii = 0;
        public int count = 0;

        private bool hitting = false;

        // in the future: use the MiiStudio API to render any mii from a nintendo account / MNMS / Mii Studio code?
        // figure out how to call the API from unity?
        // used expressions: "normal", "smile", "sorrow"
        [System.Serializable]
        public class MiiFace
        {
            public List<Sprite> Sprites;
        }

        void Awake()
        {
            game = DrummingPractice.instance;
        }

        private void Update()
        {
            if (player && PlayerInput.Pressed() && !DrummingPractice.instance.IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                Hit(false, false);
            }
        }

        public void SetFace(int type)
        {
            face.sprite = miiFaces[mii].Sprites[type];
        }

        public void Bop()
        {
            if (animator.IsAnimationNotPlaying())
                animator.Play("Bop", 0, 0);
        }

        public void Prepare(int type)
        {
            count = type;
            if (count % 2 == 0)
                animator.Play("PrepareLeft", 0, 0);
            else
                animator.Play("PrepareRight", 0, 0);
        }

        public void Hit(bool hit, bool applause, bool force = false)
        {
            if(player && force)
            {
                if (hit)
                {
                    HitSound(applause);
                    DrummingPractice.instance.Streak();
                } else
                    MissSound();
            }

            if (!hitting)
            {
                if (count % 2 == 0)
                    animator.Play("HitLeft", 0, 0);
                else
                    animator.Play("HitRight", 0, 0);
                count++;

                if (player && !force)
                {
                    if (hit)
                        HitSound(applause);
                    else
                        MissSound();
                }

                hitting = true;
            }
        }

        private void HitSound(bool applause)
        {
            Jukebox.PlayOneShotGame("drummingPractice/hit");
            if (applause) Jukebox.PlayOneShot("applause");
        }

        private void MissSound()
        {
            Jukebox.PlayOneShotGame("drummingPractice/miss");
        }

        public void EndHit()
        {
            hitting = false;
        }
    }
}