using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] private Button createButton;

        [SerializeField] private Animator logoAnim;

        [SerializeField] private float bpm = 114f;

        private AudioSource musicSource;

        private double songPosBeat;
        private double songPos;
        private double time;
        private double targetBopBeat;

        private double lastAbsTime;
        private void Start()
        {
            musicSource = GetComponent<AudioSource>();
            createButton.onClick.AddListener(delegate { GlobalGameManager.LoadScene("Editor"); Jukebox.PlayOneShot("ui/UIEnter"); });
            musicSource.Play();
        }

        private void Update()
        {
            if (songPos >= musicSource.clip.length)
            {
                time = 0;
                targetBopBeat = 0;
            }
            double absTime = Time.realtimeSinceStartup;
            double dt = absTime - lastAbsTime;
            lastAbsTime = absTime;

            time += dt;

            songPos = time;

            songPosBeat = SecsToBeats(songPos);

            if (songPosBeat >= targetBopBeat)
            {
                logoAnim.Play("LogoBop", 0, 0);
                targetBopBeat += 1;
            }
        }

        public double SecsToBeats(double s)
        {
            return s / 60f * bpm;
        }
    }
}

