using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class SnapDialog : Dialog
    {
        [SerializeField] private TMP_Text snapText;
        private Timeline timeline;

        private static float[] CommonDenominators = { 1, 2, 3, 4, 6, 8, 12, 16};
        private int currentCommon = 3;
        private void Start() 
        {
            timeline = Timeline.instance;
        }

        public void SwitchSnapDialog()
        {
            if(dialog.activeSelf) {
                dialog.SetActive(false);
            } else {
                ResetAllDialogs();
                dialog.SetActive(true);
            }
        }

        public void ChangeCommon(bool down = false)
        {
            if(down) {
                currentCommon--;
            } else {
                currentCommon++;
            }
            if(currentCommon < 0) {
                currentCommon = 0;
            } else if(currentCommon >= CommonDenominators.Length) {
                currentCommon = CommonDenominators.Length - 1;
            }
            timeline.SetSnap(1f / CommonDenominators[currentCommon]);
        }

        private void Update()
        {
            snapText.text = $"1/{CommonDenominators[currentCommon]}";
        }
    }
}