using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio;
using TMPro;

namespace HeavenStudio.StudioDance
{
    public class StudioDanceManager : MonoBehaviour
    {
        [SerializeField] private GameObject windowBase;
        [SerializeField] private Transform windowHolder;
        [SerializeField] private GameObject content;
        [SerializeField] private Dancer dancer;
        [SerializeField] private TMP_Dropdown dropdown;

        public void OpenDanceWindow()
        {
            windowBase.SetActive(true);
            content.SetActive(true);

            dropdown.ClearOptions();
            int startIdx = 0;
            foreach (ChoreographyInfo choreography in dancer.ChoreographyInfos)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(choreography.choreographyName));
                if (choreography == dancer.CurrentChoreography)
                {
                    dropdown.value = startIdx;
                }
                startIdx++;
            }
        }

        public void CloseDanceWindow()
        {
            windowBase.SetActive(false);
            content.SetActive(false);
            Editor.CreditsLegalSettings.MakeSecretInactive();
        }

        public void OnDropdownValueChanged(int index)
        {
            dancer.SetChoreography(index);
        }
    }
}