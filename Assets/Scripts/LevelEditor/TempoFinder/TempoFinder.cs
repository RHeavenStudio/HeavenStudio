using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Editor
{
    public class TempoFinder : Dialog
    {
        [SerializeField] private BPMText bpmText;
        private bool pressed;
        private double timePressed;
        private double lastTimePressed = double.MinValue;

        private void Awake()
        {
            pressed = false;
            timePressed = 0f;
            lastTimePressed = double.MinValue;
        }

        public void SwitchTempoDialog()
        {
            if (dialog.activeSelf)
            {
                dialog.SetActive(false);
                timePressed = 0;
                lastTimePressed = double.MinValue;
                bpmText.ResetText();
            }
            else
            {
                ResetAllDialogs();
                dialog.SetActive(true);
            }
        }

        public void TapBPM()
        {
            pressed = true;
        }

        private void Update()
        {
            bool conductorTimeSource = Conductor.instance != null && Conductor.instance.NotStopped();
            timePressed += Time.deltaTime;
            if (timePressed > 2)
            {
                timePressed = 0;
                lastTimePressed = double.MinValue;
                bpmText.ClearSamples();
                pressed = false;
            }

            if (pressed)
            {
                if (!conductorTimeSource)
                {
                    bpmText.ChangeText(timePressed);
                    lastTimePressed = double.MinValue;
                }
                else
                {
                    if (lastTimePressed == double.MinValue)
                    {
                        bpmText.ChangeText(timePressed);
                    }
                    else
                    {
                        bpmText.ChangeText(Conductor.instance.songPositionAsDouble - lastTimePressed);
                    }
                    lastTimePressed = Conductor.instance.songPositionAsDouble;
                }
                timePressed = 0;
                pressed = false;
            }
        }
    }
}