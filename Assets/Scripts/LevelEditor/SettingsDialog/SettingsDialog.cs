using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Common;

namespace HeavenStudio.Editor 
{
    public class SettingsDialog : Dialog
    {
        [SerializeField] private TMP_Text BuildDateDisplay;
        [SerializeField] private TabsManager tabsManager;
        [SerializeField] private TabsManager.TabsEntry[] tabs;

        private void Start() {}

        public void SwitchSettingsDialog()
        {
            if(dialog.activeSelf) {
                Editor.instance.canSelect = true;
                Editor.instance.inAuthorativeMenu = false;
                dialog.SetActive(false);

                PersistentDataManager.SaveSettings();
                tabsManager.CleanTabs();
            } else {
                ResetAllDialogs();
                Editor.instance.canSelect = false;
                Editor.instance.inAuthorativeMenu = true;
                dialog.SetActive(true);

                tabsManager.GenerateTabs(tabs);

                BuildDateDisplay.text = GlobalGameManager.buildTime;
            }
        }

        private void Update() {}
    }
}