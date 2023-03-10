using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Common;

namespace HeavenStudio.Editor
{
    public class GameSettings : TabsContent
    {
        [SerializeField] Toggle editorOverlaysToggle;
        [SerializeField] Toggle perfectChallengeToggle;
        [SerializeField] Toggle sectionMedalsToggle;
        [SerializeField] Toggle timingDispMinModeToggle;

        [Header("Layout Settings - General")]
        [SerializeField] Toggle ElementToggle;

        [SerializeField] TMP_InputField XPosInput;
        [SerializeField] TMP_InputField YPosInput;
        [SerializeField] Slider XPosSlider;
        [SerializeField] Slider YPosSlider;

        [SerializeField] TMP_InputField RotationInput;
        [SerializeField] Slider RotationSlider;

        [SerializeField] TMP_InputField ScaleInput;
        [SerializeField] Slider ScaleSlider;

        [Header("Layout Settings - Timing Display")]
        [SerializeField] TMP_Dropdown TimingDispTypeDropdown;

        List<OverlaysManager.OverlayOption> lytElements = new List<OverlaysManager.OverlayOption>();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnEditorOverlaysToggleChanged()
        {
            PersistentDataManager.gameSettings.overlaysInEditor = editorOverlaysToggle.isOn;
        }
        public void OnPerfectChallengeToggleChanged()
        {
            PersistentDataManager.gameSettings.perfectChallengeType = perfectChallengeToggle.isOn ? PersistentDataManager.PerfectChallengeType.On : PersistentDataManager.PerfectChallengeType.Off;
        }

        public void OnSectionMedalsToggleChanged()
        {
            PersistentDataManager.gameSettings.isMedalOn = sectionMedalsToggle.isOn;
        }

        public void OnTimingDispMinModeToggleChanged()
        {
            PersistentDataManager.gameSettings.timingDisplayMinMode = timingDispMinModeToggle.isOn;
        }

        public override void OnOpenTab()
        {
            editorOverlaysToggle.isOn = PersistentDataManager.gameSettings.overlaysInEditor;
            perfectChallengeToggle.isOn = PersistentDataManager.gameSettings.perfectChallengeType != PersistentDataManager.PerfectChallengeType.Off;
            sectionMedalsToggle.isOn = PersistentDataManager.gameSettings.isMedalOn;
            timingDispMinModeToggle.isOn = PersistentDataManager.gameSettings.timingDisplayMinMode;

            lytElements = new List<OverlaysManager.OverlayOption>();
            foreach (var c in PersistentDataManager.gameSettings.timingDisplayComponents) { lytElements.Add(c); }
            foreach (var c in PersistentDataManager.gameSettings.skillStarComponents) { lytElements.Add(c); }
            foreach (var c in PersistentDataManager.gameSettings.sectionComponents) { lytElements.Add(c); }
        }

        public override void OnCloseTab()
        {
        }
    }
}