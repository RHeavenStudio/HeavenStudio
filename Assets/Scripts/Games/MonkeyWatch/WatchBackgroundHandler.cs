using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using Starpelly;

namespace HeavenStudio.Games.Scripts_MonkeyWatch
{
    public class WatchBackgroundHandler : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer[] srsIn;
        [SerializeField] private SpriteRenderer[] srsOut;
        [SerializeField] private Transform anchorHour;
        [SerializeField] private Transform anchorMinute;

        private double fadeBeat = -1;
        private float fadeLength = 0f;
        private bool fadeOut = false;
        private bool realTime = true;

        private void Awake()
        {
            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;

            float normalizedBeat = Mathf.Clamp01(cond.GetPositionFromBeat(fadeBeat, fadeLength));

            if (fadeOut)
            {
                foreach (var s in srsIn)
                {
                    s.color = new Color(s.color.r, s.color.g, s.color.b, 1 - normalizedBeat);
                }

                foreach (var s in srsOut)
                {
                    s.color = new Color(s.color.r, s.color.g, s.color.b, normalizedBeat);
                }
            }
            else
            {
                foreach (var s in srsIn)
                {
                    s.color = new Color(s.color.r, s.color.g, s.color.b, normalizedBeat);
                }

                foreach (var s in srsOut)
                {
                    s.color = new Color(s.color.r, s.color.g, s.color.b, 1 - normalizedBeat);
                }
            }

            if (realTime)
            {
                var nowTime = System.DateTime.Now;

                SetArrowsToTime(nowTime.Hour, nowTime.Minute, nowTime.Second);
            }
        }

        public void SetFade(double beat, float length, bool outFade, MonkeyWatch.TimeMode timeMode, int hours, int minutes)
        {
            fadeBeat = beat;
            fadeLength = length;
            fadeOut = outFade;
            realTime = timeMode == MonkeyWatch.TimeMode.RealTime;
            if (!realTime)
            {
                SetArrowsToTime(hours, minutes, 0);
            }
            Update();
        }

        private void SetArrowsToTime(int hours, int minutes, int seconds)
        {
            float normalizedHour = Mathp.Normalize(hours + (minutes / 60f) + (seconds / 3600f), 0f, 12f);
            anchorHour.localEulerAngles = new Vector3(0, 0, -Mathf.LerpUnclamped(0, 360, normalizedHour));

            float normalizedMinute = Mathp.Normalize(minutes + (seconds / 60f), 0, 60);
            anchorMinute.localEulerAngles = new Vector3(0, 0, -Mathf.LerpUnclamped(0, 360, normalizedMinute));
        }
    }
}

