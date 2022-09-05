using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HeavenStudio.Editor 
{
    public class EditorSettings : TabsContent
    {
        public Toggle cursorCheckbox;

        public void OnCursorCheckboxChanged()
        {
            Editor.instance.isCursorEnabled = cursorCheckbox.isOn;
            if (!Editor.instance.fullscreen)
            {
                GameManager.instance.CursorCam.enabled = Editor.instance.isCursorEnabled;
            }
        }

        public override void OnOpenTab()
        {
        }

        public override void OnCloseTab()
        {
        }
    }
}