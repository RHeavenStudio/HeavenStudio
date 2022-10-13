using UnityEngine;
using TMPro;

namespace HeavenStudio.Editor 
{
    public class SettingsDialog : Dialog
    {
        [SerializeField] private TMP_Text BuildDateDisplay;
        private void Start() {}

        public void SwitchSettingsDialog()
        {
            if(dialog.activeSelf) {
                Editor.instance.canSelect = true;
                Editor.instance.inAuthorativeMenu = false;
                dialog.SetActive(false);
            } else {
                ResetAllDialogs();
                Editor.instance.canSelect = false;
                Editor.instance.inAuthorativeMenu = true;
                dialog.SetActive(true);

                BuildDateDisplay.text = GlobalGameManager.buildTime;
            }
        }

        private void Update() {}
    }
}