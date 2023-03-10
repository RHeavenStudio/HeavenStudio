using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        [Header("Layout Settings - Header")]
        [SerializeField] TMP_Text ElementNameText;

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
        [SerializeField] GameObject TimingDispTypeContainer;
        [SerializeField] TMP_Dropdown TimingDispTypeDropdown;

        List<OverlaysManager.OverlayOption> lytElements = new List<OverlaysManager.OverlayOption>();
        static int currentElementIdx = 0;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void CreateDefaultLayout()
        {
            PersistentDataManager.gameSettings.timingDisplayComponents = new List<OverlaysManager.TimingDisplayComponent>()
            {
                OverlaysManager.TimingDisplayComponent.CreateDefaultDual()
            };
            PersistentDataManager.gameSettings.skillStarComponents = new List<OverlaysManager.SkillStarComponent>()
            {
                OverlaysManager.SkillStarComponent.CreateDefault()
            };
            PersistentDataManager.gameSettings.sectionComponents = new List<OverlaysManager.SectionComponent>()
            {
                OverlaysManager.SectionComponent.CreateDefault()
            };
            PersistentDataManager.SaveSettings();
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
            TimingDispTypeDropdown.ClearOptions();
            TimingDispTypeDropdown.AddOptions(Enum.GetNames(typeof(OverlaysManager.TimingDisplayComponent.TimingDisplayType)).ToList());

            editorOverlaysToggle.isOn = PersistentDataManager.gameSettings.overlaysInEditor;
            perfectChallengeToggle.isOn = PersistentDataManager.gameSettings.perfectChallengeType != PersistentDataManager.PerfectChallengeType.Off;
            sectionMedalsToggle.isOn = PersistentDataManager.gameSettings.isMedalOn;
            timingDispMinModeToggle.isOn = PersistentDataManager.gameSettings.timingDisplayMinMode;

            if (PersistentDataManager.gameSettings.timingDisplayComponents.Count == 0 &&
                PersistentDataManager.gameSettings.skillStarComponents.Count == 0 &&
                PersistentDataManager.gameSettings.sectionComponents.Count == 0)
            {
                CreateDefaultLayout();
            }

            lytElements = new List<OverlaysManager.OverlayOption>();
            foreach (var c in PersistentDataManager.gameSettings.timingDisplayComponents) { lytElements.Add(c); }
            foreach (var c in PersistentDataManager.gameSettings.skillStarComponents) { lytElements.Add(c); }
            foreach (var c in PersistentDataManager.gameSettings.sectionComponents) { lytElements.Add(c); }

            UpdateLayoutSettings();
        }

        public override void OnCloseTab()
        {
        }

        void UpdateLayoutSettings()
        {
            var element = lytElements[currentElementIdx];
            ElementToggle.isOn = element.enable;
            XPosInput.text = element.position.x.ToString();
            YPosInput.text = element.position.y.ToString();
            XPosSlider.value = element.position.x;
            YPosSlider.value = element.position.y;
            RotationInput.text = element.rotation.ToString();
            RotationSlider.value = element.rotation;
            ScaleInput.text = element.scale.ToString();
            ScaleSlider.value = element.scale;

            if (element is OverlaysManager.TimingDisplayComponent)
            {
                TimingDispTypeContainer.SetActive(true);
                TimingDispTypeDropdown.value = (int)(element as OverlaysManager.TimingDisplayComponent).tdType;
                ElementNameText.text = "Timing Display";
            }
            else
            {
                TimingDispTypeContainer.SetActive(false);
            }
            if (element is OverlaysManager.SkillStarComponent)
            { 
                ElementNameText.text = "Skill Star";
            }
            if (element is OverlaysManager.SectionComponent)
            {
                ElementNameText.text = "Section Progress";
            }
        }

        public void OnNextElementButtonClicked()
        {
            currentElementIdx = (currentElementIdx + 1) % lytElements.Count;
            UpdateLayoutSettings();
        }

        public void OnPrevElementButtonClicked()
        {
            currentElementIdx = (currentElementIdx - 1 + lytElements.Count) % lytElements.Count;
            UpdateLayoutSettings();
        }
    }
}