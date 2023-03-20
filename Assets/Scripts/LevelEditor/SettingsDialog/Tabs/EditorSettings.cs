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

        private void Start() {
            cursorCheckbox.isOn = PersistentDataManager.gameSettings.editorCursorEnable;
            discordRPCCheckbox.isOn = PersistentDataManager.gameSettings.discordRPCEnable;
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
        }

        public override void OnCloseTab()
        {
        }
    }
}