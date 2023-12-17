using System;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.StudioDance
{
    public class Dancer : MonoBehaviour
    {
        [SerializeField] ChoreographyInfo debugChoreography;
        [SerializeField] ChoreographyInfo[] choreographies;
        private Animator animator;
        private double currentBeat = 0f;

        private bool isDance = false;

        private ChoreographyInfo currentChoreography;
        private double totalChoreographyLength = 0f;

        public ChoreographyInfo[] ChoreographyInfos { get => choreographies; }
        public ChoreographyInfo CurrentChoreography { get => currentChoreography; }

        public void SetChoreography(ChoreographyInfo choreography)
        {
            currentChoreography = choreography;
            totalChoreographyLength = 0f;
            foreach (var step in choreography.choreographySteps)
            {
                totalChoreographyLength += step.beatLength;
            }
        }

        public void SetChoreography(int index)
        {
            if (index < 0 || index >= choreographies.Length) return;
            var choreography = choreographies[index];
            currentChoreography = choreography;
            totalChoreographyLength = 0f;
            foreach (var step in choreography.choreographySteps)
            {
                totalChoreographyLength += step.beatLength;
            }

            if (!Conductor.instance.isPlaying)
            {
                animator.Play(currentChoreography.introState);
            }
        }

        private void Start()
        {
            animator = GetComponent<Animator>();

            var gm = GameManager.instance;
            if (gm != null)
            {
                gm.onBeatPulse += OnBeatPulse;
            }

            if (debugChoreography != null)
            {
                SetChoreography(debugChoreography);
            }

            animator.Play(currentChoreography.introState);
        }

        private void OnBeatPulse(double beat)
        {
            currentBeat = beat;
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (currentChoreography == null || cond == null) return;
            if (!cond.isPlaying)
            {
                if (!isDance) return;
                if (currentBeat % 2 != 0)
                {
                    animator.Play(currentChoreography.poseStateOdd);
                }
                else
                {
                    animator.Play(currentChoreography.poseStateEven);
                }
                isDance = false;
                return;
            }
            isDance = true;

            double choreoBeat = cond.songPositionInBeatsAsDouble % totalChoreographyLength;
            double cycleStartBeat = Math.Floor(cond.songPositionInBeatsAsDouble / totalChoreographyLength) * totalChoreographyLength;

            double beatSum = 0.0;
            double stepLength = 0.0;
            string stepState = "";
            foreach (ChoreographyInfo.ChoreographyStep s in currentChoreography.choreographySteps)
            {
                if (choreoBeat > beatSum && choreoBeat < beatSum + s.beatLength)
                {
                    stepLength = s.beatLength;
                    stepState = s.stateName;
                    break;
                }
                beatSum += s.beatLength;
            }
            if (stepState is not null or "")
            {
                animator.DoScaledAnimation(stepState, cycleStartBeat + beatSum, stepLength);
            }
        }
    }
}