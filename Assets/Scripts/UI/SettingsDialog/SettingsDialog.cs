using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Editor;

namespace HeavenStudio.Common 
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
                dialog.SetActive(false);

                PersistentDataManager.SaveSettings();
                tabsManager.CleanTabs();

                if (GameManager.instance != null && GameManager.instance.CircleCursor != null && GameManager.instance.playMode)
                {
                    GameManager.instance.CircleCursor.LockCursor(true);
                }

                if (Editor.Editor.instance == null) return;
                Editor.Editor.instance.canSelect = true;
                Editor.Editor.instance.inAuthorativeMenu = false;
            } else {
                ResetAllDialogs();
                dialog.SetActive(true);

                tabsManager.GenerateTabs(tabs);

                BuildDateDisplay.text = GlobalGameManager.buildTime;


                if (GameManager.instance != null && GameManager.instance.CircleCursor != null && GameManager.instance.playMode)
                {
                    GameManager.instance.CircleCursor.LockCursor(false);
                }

                if (Editor.Editor.instance == null) return;
                Editor.Editor.instance.canSelect = false;
                Editor.Editor.instance.inAuthorativeMenu = true;
            }
        }

        private void Update() {}
    }
}