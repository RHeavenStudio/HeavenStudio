using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using TMPro;
using Jukebox;
using UnityEngine.Playables;

namespace HeavenStudio
{
    [RequireComponent(typeof(PlayableDirector), typeof(AudioSource))]
    public class JudgementManager : MonoBehaviour
    {
        public enum InputCategory : int
        {
            Normal = 0,
            Keep = 1,
            Aim = 2,
            Repeat = 3
            // higher values are (will be) custom categories
        }

        [Serializable]
        public struct MedalInfo
        {
            public double beat;
            public string name;
            public double score;
            public bool cleared;
        }

        [Serializable]
        public struct InputInfo
        {
            public double beat;
            public double accuracyState;
            public double timeOffset;
            public float weight;
            public int category;
        }

        [Serializable]
        public struct JudgementInfo
        {
            public List<InputInfo> inputs;
            public List<MedalInfo> medals;

            public double finalScore;
            public bool star, perfect;
            public DateTime time;
        }

        const string MessageAdd = "Also... ";

        static JudgementInfo judgementInfo;
        static RiqBeatmap playedBeatmap;

        public static void SetPlayInfo(JudgementInfo info, RiqBeatmap beatmap)
        {
            judgementInfo = info;
            playedBeatmap = beatmap;
        }

        [SerializeField] TMP_Text message0;
        [SerializeField] TMP_Text message1;
        [SerializeField] TMP_Text message2;
        [SerializeField] TMP_Text barText;
        [SerializeField] Slider barSlider;

        [SerializeField] AudioClip messageMid, messageLast;

        [SerializeField] GameObject rankLogo;
        [SerializeField] Animator rankAnim;
        [SerializeField] CanvasScaler scaler;

        AudioSource audioSource;
        bool twoMessage = false, barStarted = false;

        public void PrepareJudgement()
        {
            barText.text = "0";
            barSlider.value = 0;
            twoMessage = true;
            if (twoMessage)
            {
                message0.gameObject.SetActive(false);
                message1.gameObject.SetActive(true);
                message2.gameObject.SetActive(true);
                message1.text = " ";
                message2.text = " ";
            }
            else
            {
                message0.gameObject.SetActive(true);
                message1.gameObject.SetActive(false);
                message2.gameObject.SetActive(false);
                message0.text = " ";
            }
        }

        public void ShowMessage0()
        {
            if (twoMessage) return;
            audioSource.PlayOneShot(messageLast);
            message0.text = "single line message";
        }

        public void ShowMessage1()
        {
            if (!twoMessage) return;
            audioSource.PlayOneShot(messageMid);
            message1.text = "message line 1";
        }

        public void ShowMessage2()
        {
            if (!twoMessage) return;
            audioSource.PlayOneShot(messageLast);
            message2.text = "message line 2";
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            float w = Screen.width / 1920f;
            float h = Screen.height / 1080f;
            scaler.scaleFactor = Mathf.Min(w, h);
        }
    }
}