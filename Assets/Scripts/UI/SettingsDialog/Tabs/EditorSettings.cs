using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Common;

namespace HeavenStudio.Editor 
{
    public class EditorSettings : TabsContent
    {
        [SerializeField] Toggle cursorCheckbox;
        [SerializeField] Toggle discordRPCCheckbox;
        [SerializeField] Button editorScaleDecre, editorScaleIncre;
        [SerializeField] Toggle scaleWSS;
        [SerializeField] Toggle paramTooltipsToggle;
        [SerializeField] Toggle previewNoteSoundsToggle;
        // [SerializeField] Toggle cornerTooltipsToggle;

        private void Start()
        {
            cursorCheckbox.isOn = PersistentDataManager.gameSettings.editorCursorEnable;
            discordRPCCheckbox.isOn = PersistentDataManager.gameSettings.discordRPCEnable;
            scaleWSS.isOn = PersistentDataManager.gameSettings.scaleWScreenSize;
            paramTooltipsToggle.isOn = PersistentDataManager.gameSettings.showParamTooltips;
            previewNoteSoundsToggle.isOn = PersistentDataManager.gameSettings.previewNoteSounds;

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
            Start();
        }

        public override void OnCloseTab()
        {
        }

        public void OnSWSSChanged()
        {
            PersistentDataManager.gameSettings.scaleWScreenSize = scaleWSS.isOn;
            scaleWSS.isOn = PersistentDataManager.gameSettings.scaleWScreenSize;
        }

        public void OnParamTooltipsChanged()
        {
            PersistentDataManager.gameSettings.showParamTooltips = paramTooltipsToggle.isOn;
        }

        public void OnPreviewNoteSoundsChanged()
        {
            PersistentDataManager.gameSettings.previewNoteSounds = previewNoteSoundsToggle.isOn;
        }

        // public void OnCornerTooltipsChanged()
        // {
        //     PersistentDataManager.gameSettings.showParamTooltips = cornerTooltipsToggle.isOn;
        // }

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