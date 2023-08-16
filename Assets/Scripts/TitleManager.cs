using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using HeavenStudio.Util;
using Starpelly;
using System.Linq;

namespace HeavenStudio
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] private Button createButton;

        [SerializeField] private Animator logoAnim;

        [SerializeField] private List<Animator> starAnims;

        [SerializeField] private Animator pressAnyKeyAnim;

        [SerializeField] private float bpm = 114f;
        [SerializeField] private float offset = 0f;

        [SerializeField] private RawImage bg;

        [SerializeField] private float bgXSpeed;
        [SerializeField] private float bgYSpeed;

        [SerializeField] private Collider2D logoHoverCollider;

        private AudioSource musicSource;

        private double songPosBeat;
        private double songPos;
        private double time;
        private double targetBopBeat;

        private int loops;

        private double lastAbsTime;

        private bool altBop;

        private bool logoRevealed;

        private bool menuMode;

        private Animator menuAnim;

        private void Start()
        {
            menuAnim = GetComponent<Animator>();
            musicSource = GetComponent<AudioSource>();
            musicSource.Play();
            var _rand = new System.Random();
            starAnims = starAnims.OrderBy(_ => _rand.Next()).ToList();
        }

        private void Update()
        {
            bg.uvRect = new Rect(bg.uvRect.position + (new Vector2(bgXSpeed, bgYSpeed) * Time.deltaTime), bg.uvRect.size);
            if (songPos >= musicSource.clip.length)
            {
                time = 0;
                targetBopBeat = 0;
                loops++;
            }
            double absTime = Time.realtimeSinceStartup;
            double dt = absTime - lastAbsTime;
            lastAbsTime = absTime;

            time += dt;

            songPos = time + offset;

            songPosBeat = SecsToBeats(songPos);
            if (logoRevealed && !menuMode && Input.anyKeyDown)
            {
                menuMode = true;
                menuAnim.Play("Revealed", 0, 0);
                pressAnyKeyAnim.Play("PressKeyFadeOut", 0, 0);
            }
            if (loops == 0)
            {
                float normalizedBeat = GetPositionFromBeat(4, 1);
                if (normalizedBeat > 0 && normalizedBeat <= 1f)
                {
                    logoAnim.DoNormalizedAnimation("Reveal", normalizedBeat);
                    pressAnyKeyAnim.DoNormalizedAnimation("PressKeyFadeIn", normalizedBeat);
                }
                else if (normalizedBeat < 0)
                {
                    logoAnim.DoNormalizedAnimation("Reveal", 0);
                }
                else if (normalizedBeat > 1f)
                {
                    logoRevealed = true;
                }
            }
            if (songPosBeat - 1 >= targetBopBeat)
            {
                if (targetBopBeat <= 3 && loops == 0)
                {
                    starAnims[(int)targetBopBeat].Play("StarAppearBop", 0, 0);
                    if (targetBopBeat == 3) starAnims[4].Play("StarAppearBop", 0, 0);
                    for (int i = 0; i < (int)targetBopBeat; i++)
                    {
                        starAnims[i].Play("StarBopNoRot", 0, 0);
                    }
                }
                else
                {
                    foreach (var star in starAnims)
                    {
                        star.Play("StarBop", 0, 0);
                    }
                }
                if (targetBopBeat > 3 || loops > 0) 
                {
                    logoAnim.Play(altBop ? "LogoBop2" : "LogoBop", 0, 0);
                    altBop = !altBop;
                }
                targetBopBeat += 1;
                if (logoRevealed && logoHoverCollider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
                {
                    SoundByte.PlayOneShotScheduled("count-ins/cowbell", AudioSettings.dspTime + BeatsToSecs(targetBopBeat - songPosBeat, bpm));
                }
            }
        }

        public double SecsToBeats(double s)
        {
            return s / 60f * bpm;
        }

        public double BeatsToSecs(double beats, float bpm)
        {
            return beats / bpm * 60f;
        }

        public float GetPositionFromBeat(float startBeat, float length)
        {
            float a = Mathp.Normalize((float)songPosBeat, startBeat, startBeat + length);
            return a;
        }

        public void CreatePressed()
        {
            GlobalGameManager.LoadScene("Editor");
            SoundByte.PlayOneShot("ui/UIEnter");
        }

        public void PlayPressed()
        {
            // go into the play mode menu
        }

        public void SocialsPressed()
        {
            // show a panel with our SNS links
        }

        public void SettingsPressed()
        {
            // show the settings menu prefab
        }

        public void QuitPressed()
        {
            Application.Quit();
        }
    }
}

