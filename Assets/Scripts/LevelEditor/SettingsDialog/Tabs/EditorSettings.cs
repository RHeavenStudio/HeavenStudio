using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HeavenStudio.Editor 
{
    public class EditorSettings : TabsContent
    {
        public Toggle cursorCheckbox;

        public Toggle discordRPCCheckbox;

        public void OnCursorCheckboxChanged()
        {
            Editor.instance.isCursorEnabled = cursorCheckbox.isOn;
            if (!Editor.instance.fullscreen)
            {
                GameManager.instance.CursorCam.enabled = Editor.instance.isCursorEnabled;
            }
        }

        public void OnRPCCheckboxChanged()
        {
            Editor.instance.isDiscordEnabled = discordRPCCheckbox.isOn;
            Debug.Log("Value changed");
        }

        public override void OnOpenTab()
        {
        }

        public override void OnCloseTab()
        {
        }
    }
}