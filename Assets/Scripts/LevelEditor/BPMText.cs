using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

namespace HeavenStudio.Editor
{
    public class BPMText : MonoBehaviour
    {
        public const int maxPressTimes = 50;

        [SerializeField] private TMP_Text BPM;
        [SerializeField] private TMP_Text BPMRounded;

        private List<double> pressTimes = new();

        public void ChangeText(double timePressed)
        {
            pressTimes.Add(timePressed);

            // First press isn't good to work with.
            if (pressTimes.Count < 2) return;

            // Limit the number of press times stored.
            if (pressTimes.Count > maxPressTimes)
                pressTimes.RemoveAt(0);

            double averageTime = pressTimes.GetRange(1, pressTimes.Count - 1).Average();

            double thisBPM = 60 / averageTime;
            BPM.text = $"{thisBPM:0.000}";
            BPMRounded.text = $"{(int)Math.Round(thisBPM)}";
        }

        public void ResetText()
        {
            pressTimes.Clear();

            BPM.text = "---";
            BPMRounded.text = "---";
        }

        public void ClearSamples()
        {
            if (pressTimes.Count < 2) return;

            if (pressTimes.Count > maxPressTimes)
                pressTimes.RemoveAt(0);

            double averageTime = pressTimes.GetRange(1, pressTimes.Count - 1).Average();

            double thisBPM = 60 / averageTime;
            BPM.text = $"<color=\"yellow\">{thisBPM:0.000}";
            BPMRounded.text = $"<color=\"yellow\">{(int)Math.Round(thisBPM)}";

            pressTimes.Clear();
        }
    }
}
