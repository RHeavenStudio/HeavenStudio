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

        private List<float> pressTimes = new List<float>();

        public void ChangeText(float timePressed)
        {
            pressTimes.Add(timePressed);

            // First press isn't good to work with.
            if (pressTimes.Count < 2) return;

            // Limit the number of press times stored.
            if (pressTimes.Count > maxPressTimes)
                pressTimes.RemoveAt(0);

            var averageTime = pressTimes.GetRange(1, pressTimes.Count - 1).Average();

            float thisBPM = 60 / averageTime; // BPM = 60/t
            BPM.text = $"{thisBPM}";
            BPMRounded.text = $"{Mathf.RoundToInt(thisBPM)}";
        }
        public void ResetText()
        {
            pressTimes.Clear();

            BPM.text = "---";
            BPMRounded.text = "---";
        }
    }
}
