using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using HeavenStudio.Util;
using Starpelly;

namespace HeavenStudio
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] private Button createButton;

        [SerializeField] private Animator logoAnim;

        [SerializeField] private List<Animator> starAnims;

        [SerializeField] private float bpm = 114f;

        [SerializeField] private RawImage bg;

        [SerializeField] private float bgXSpeed;
        [SerializeField] private float bgYSpeed;

        private AudioSource musicSource;

        private double songPosBeat;
        private double songPos;
        private double time;
        private double targetBopBeat;

        private int loops;

        private double lastAbsTime;

        private bool altBop;

        private void Start()
        {
            musicSource = GetComponent<AudioSource>();
            createButton.onClick.AddListener(delegate { GlobalGameManager.LoadScene("Editor"); Jukebox.PlayOneShot("ui/UIEnter"); });
            musicSource.Play();
        }

        private void Update()
        {
            bg.uvRect = new Rect(bg.uvRect.position + (new Vector2(bgXSpeed, bgYSpeed) * Time.deltaTime), bg.uvRect.size);
            if (songPos >= musicSource.clip.length)
            {
                time = 0;
                targetBopBeat = 1;
                loops++;
            }
            double absTime = Time.realtimeSinceStartup;
            double dt = absTime - lastAbsTime;
            lastAbsTime = absTime;

            time += dt;

            songPos = time;

            songPosBeat = SecsToBeats(songPos);
            if (loops == 0)
            {
                float normalizedBeat = GetPositionFromBeat(4, 1);
                if (normalizedBeat > 0 && normalizedBeat <= 1f)
                {
                    logoAnim.DoNormalizedAnimation("Reveal", normalizedBeat);
                }
                else if (normalizedBeat < 0)
                {
                    logoAnim.DoNormalizedAnimation("Reveal", 0);
                }
            }
            if (songPosBeat >= targetBopBeat)
            {
                if (targetBopBeat > 4 || loops > 0) 
                {
                    logoAnim.Play(altBop ? "LogoBop2" : "LogoBop", 0, 0);
                    altBop = !altBop;
                }
                foreach (var star in starAnims)
                {
                    star.Play("StarBop", 0, 0);
                }
                targetBopBeat += 1;
            }
        }

        public double SecsToBeats(double s)
        {
            return s / 60f * bpm;
        }

        public float GetPositionFromBeat(float startBeat, float length)
        {
            float a = Mathp.Normalize((float)songPosBeat, startBeat, startBeat + length);
            return a;
        }
    }
}

