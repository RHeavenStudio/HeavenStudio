using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio
{
    public class AudioDspTimeKeeper : MonoBehaviour
    {
        [SerializeField] private List<double> xValuesL = new List<double>();
        [SerializeField] private List<double> yValuesL = new List<double>();

        private double coeff1, coeff2;

        private double audioDspStartTime;

        private AudioSource audioSource;

        public double currentSmoothedDSPTime;

        public double dspTime;
        public float audioTime;

        private double musicDrift;

        private Conductor conductor;

        public float latencyAdjustment;

        public void Play()
        {
            audioSource.PlayScheduled(audioDspStartTime);
            audioDspStartTime = AudioSettings.dspTime;
        }

        private void Start()
        {
            conductor = GetComponent<Conductor>();
            audioSource = conductor.musicSource;
        }

        private void Update()
        {
            if (!audioSource.isPlaying) return;

            float currentGameTime = Time.realtimeSinceStartup;
            double currentDspTime = AudioSettings.dspTime;

            // Update our linear regression model by adding another data point.
            UpdateLinearRegression(currentGameTime, currentDspTime);
            CheckForDrift();

            dspTime = GetCurrentTimeInSong();
            audioTime = audioSource.time;
        }

        public double SmoothedDSPTime()
        {
            double result = Time.unscaledTimeAsDouble * coeff1 + coeff2;
            if (result > currentSmoothedDSPTime)
            {
                currentSmoothedDSPTime = result;
            }
            return currentSmoothedDSPTime;
        }

        public double GetCurrentTimeInSong()
        {
            return this.SmoothedDSPTime() - audioDspStartTime - latencyAdjustment;
        }

        private void CheckForDrift()
        {
            double timeFromDSP = this.SmoothedDSPTime() - audioDspStartTime;
            double timeFromAudioSource = audioSource.timeSamples / (float)audioSource.clip.frequency;

            double drift = timeFromDSP - timeFromAudioSource;
            musicDrift = drift;

            if (Mathf.Abs((float)drift) > 0.05)
            {
                Debug.LogWarningFormat("Music drift of {0} detected, resyncing!", musicDrift);
                audioDspStartTime += musicDrift;
            }
        }

        private void UpdateLinearRegression(float currentGameTime, double currentDspTime)
        {
            if (xValuesL.Count > 3000)
            {
                xValuesL.RemoveRange(0, 2000);
                yValuesL.RemoveRange(0, 2000);
            }

            xValuesL.Add((double)currentGameTime);
            var xVals = xValuesL.ToArray();

            yValuesL.Add((double)currentDspTime);
            var yVals = yValuesL.ToArray();

            if (xVals.Length != yVals.Length)
            {
                throw new Exception("Input values should be with the same length.");
            }

            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double sumCodeviates = 0;

            for (var i = 0; i < xVals.Length; i++)
            {
                var x = xVals[i];
                var y = yVals[i];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }

            var count = xVals.Length;
            var ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            var ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

            var rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            var rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
            var sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            var meanX = sumOfX / count;
            var meanY = sumOfY / count;
            var dblR = rNumerator / Math.Sqrt(rDenom);

            // coeff1 = dblR * dblR;
            coeff2 = meanY - ((sCo / ssX) * meanX);
            coeff1 = sCo / ssX;
        }
    }
}