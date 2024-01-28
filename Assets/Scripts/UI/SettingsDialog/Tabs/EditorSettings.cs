using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Common;

namespace HeavenStudio.Editor 
{
    public class EditorSettings : TabsContent
    {
        public Toggle cursorCheckbox;
        public Toggle discordRPCCheckbox;
        public Button editorScaleDecre, editorScaleIncre;
        public Toggle scaleWSS;

        private void Start()
        {
            cursorCheckbox.isOn = PersistentDataManager.gameSettings.editorCursorEnable;
            discordRPCCheckbox.isOn = PersistentDataManager.gameSettings.discordRPCEnable;
            scaleWSS.isOn = PersistentDataManager.gameSettings.scaleWScreenSize;


            SetDecreIncreInteractable();
        }

        public void OnCursorCheckboxChanged()
        {
            Editor.instance.isCursorEnabled = cursorCheckbox.isOn;
            PersistentDataManager.gameSettings.editorCursorEnable = cursorCheckbox.isOn;
            if (Editor.instance != null && !Editor.instance.fullscreen)
            {
                GameManager.instance.CursorCam.enabled = Editor.instance.isCursorEnabled;
            }
        }

        public void OnRPCCheckboxChanged()
        {
            PersistentDataManager.gameSettings.discordRPCEnable = discordRPCCheckbox.isOn;
            Editor.instance.isDiscordEnabled = discordRPCCheckbox.isOn;
        }

        public override void OnOpenTab()
        {
            cursorCheckbox.isOn = PersistentDataManager.gameSettings.editorCursorEnable;
            discordRPCCheckbox.isOn = PersistentDataManager.gameSettings.discordRPCEnable;
            scaleWSS.isOn = PersistentDataManager.gameSettings.scaleWScreenSize;


            SetDecreIncreInteractable();
        }

        public override void OnCloseTab()
        {
        }

        public void OnSWSSChanged()
        {
            PersistentDataManager.gameSettings.scaleWScreenSize = scaleWSS.isOn;
            scaleWSS.isOn = PersistentDataManager.gameSettings.scaleWScreenSize;
        }

        public void OnEditorScaleDecre()
        {
            PersistentDataManager.gameSettings.editorScale--;
            if (PersistentDataManager.gameSettings.editorScale < -3)
                PersistentDataManager.gameSettings.editorScale = -3;

            SetDecreIncreInteractable();
        }

        public void OnEditorScaleIncre()
        {
            PersistentDataManager.gameSettings.editorScale++;
            if (PersistentDataManager.gameSettings.editorScale > 5)
                PersistentDataManager.gameSettings.editorScale = 5;

            SetDecreIncreInteractable();
        }

        public void OnEditorScaleReset()
        {
            PersistentDataManager.gameSettings.editorScale = 0;
            SetDecreIncreInteractable();
        }

        private void SetDecreIncreInteractable()
        {
            editorScaleDecre.interactable = PersistentDataManager.gameSettings.editorScale > -3; // hardcoded? We might not change.
            editorScaleIncre.interactable = PersistentDataManager.gameSettings.editorScale < 5;
        }
    }
}