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
        public static bool InPreview;
        [SerializeField] Toggle editorOverlaysToggle;
        [SerializeField] Toggle perfectChallengeToggle;
        [SerializeField] Toggle sectionMedalsToggle;
        [SerializeField] Toggle timingDispMinModeToggle;
        [SerializeField] Toggle letterboxBgEnable;
        [SerializeField] Toggle letterboxFxEnable;

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

        const string fFormat = "0.000";

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

        public void OnLetterboxBgToggleChanged()
        {
            PersistentDataManager.gameSettings.letterboxBgEnable = letterboxBgEnable.isOn;
            StaticCamera.instance.ToggleLetterboxBg(PersistentDataManager.gameSettings.letterboxBgEnable);
        }

        public void OnLetterboxFxToggleChanged()
        {
            PersistentDataManager.gameSettings.letterboxFxEnable = letterboxFxEnable.isOn;
            StaticCamera.instance.ToggleLetterboxGlow(PersistentDataManager.gameSettings.letterboxFxEnable);
        }

        public override void OnOpenTab()
        {
            TimingDispTypeDropdown.ClearOptions();
            TimingDispTypeDropdown.AddOptions(Enum.GetNames(typeof(OverlaysManager.TimingDisplayComponent.TimingDisplayType)).ToList());

            editorOverlaysToggle.isOn = PersistentDataManager.gameSettings.overlaysInEditor;
            perfectChallengeToggle.isOn = PersistentDataManager.gameSettings.perfectChallengeType != PersistentDataManager.PerfectChallengeType.Off;
            sectionMedalsToggle.isOn = PersistentDataManager.gameSettings.isMedalOn;
            timingDispMinModeToggle.isOn = PersistentDataManager.gameSettings.timingDisplayMinMode;
            letterboxBgEnable.isOn = PersistentDataManager.gameSettings.letterboxBgEnable;
            letterboxFxEnable.isOn = PersistentDataManager.gameSettings.letterboxFxEnable;

            if (PersistentDataManager.gameSettings.timingDisplayComponents.Count == 0 &&
                PersistentDataManager.gameSettings.skillStarComponents.Count == 0 &&
                PersistentDataManager.gameSettings.sectionComponents.Count == 0)
            {
                CreateDefaultLayout();
            }

            lytElements = new List<OverlaysManager.OverlayOption>();
            foreach (var c in PersistentDataManager.gameSettings.timingDisplayComponents) { lytElements.Add(c); c.EnablePreview();}
            foreach (var c in PersistentDataManager.gameSettings.skillStarComponents) { lytElements.Add(c); c.EnablePreview();}
            foreach (var c in PersistentDataManager.gameSettings.sectionComponents) { lytElements.Add(c); c.EnablePreview();}

            UpdateLayoutSettings();
            InPreview = true;
        }

        public override void OnCloseTab()
        {
            foreach (var e in lytElements)
            {
                e.DisablePreview();
            }
            lytElements.Clear();
            InPreview = false;
        }

        void UpdateLayoutSettings()
        {
            var element = lytElements[currentElementIdx];
            element.EnablePreview();

            ElementToggle.isOn = element.enable;
            XPosInput.text = element.position.x.ToString(fFormat);
            YPosInput.text = element.position.y.ToString(fFormat);
            XPosSlider.value = element.position.x;
            YPosSlider.value = element.position.y;
            RotationInput.text = element.rotation.ToString(fFormat);
            RotationSlider.value = element.rotation;
            ScaleInput.text = element.scale.ToString(fFormat);
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

        public void OnElementToggled()
        {
            var element = lytElements[currentElementIdx];
            element.enable = ElementToggle.isOn;
            element.PositionElement();
        }

        public void OnXPosInputChanged()
        {
            var element = lytElements[currentElementIdx];
            XPosSlider.value = float.Parse(XPosInput.text);
            element.position.x = XPosSlider.value;
            element.PositionElement();
        }

        public void OnXPosSliderChanged()
        {
            var element = lytElements[currentElementIdx];
            XPosInput.text = XPosSlider.value.ToString(fFormat);
            element.position.x = XPosSlider.value;
            element.PositionElement();
        }

        public void OnYPosInputChanged()
        {
            var element = lytElements[currentElementIdx];
            YPosSlider.value = float.Parse(YPosInput.text);
            element.position.y = YPosSlider.value;
            element.PositionElement();
        }

        public void OnYPosSliderChanged()
        {
            var element = lytElements[currentElementIdx];
            YPosInput.text = YPosSlider.value.ToString(fFormat);
            element.position.y = YPosSlider.value;
            element.PositionElement();
        }

        public void OnRotationInputChanged()
        {
            var element = lytElements[currentElementIdx];
            RotationSlider.value = float.Parse(RotationInput.text);
            element.rotation = RotationSlider.value;
            element.PositionElement();
        }

        public void OnRotationSliderChanged()
        {
            var element = lytElements[currentElementIdx];
            RotationInput.text = RotationSlider.value.ToString(fFormat);
            element.rotation = RotationSlider.value;
            element.PositionElement();
        }

        public void OnScaleInputChanged()
        {
            var element = lytElements[currentElementIdx];
            ScaleSlider.value = float.Parse(ScaleInput.text);
            element.scale = ScaleSlider.value;
            element.PositionElement();
        }

        public void OnScaleSliderChanged()
        {
            var element = lytElements[currentElementIdx];
            ScaleInput.text = ScaleSlider.value.ToString(fFormat);
            element.scale = ScaleSlider.value;
            element.PositionElement();
        }

        public void OnTimingDispTypeDropdownChanged()
        {
            var element = lytElements[currentElementIdx] as OverlaysManager.TimingDisplayComponent;
            if (element == null) return;
            element.tdType = (OverlaysManager.TimingDisplayComponent.TimingDisplayType)TimingDispTypeDropdown.value;
            bool elHide = element.enable;
            switch (element.tdType)
            {
                case OverlaysManager.TimingDisplayComponent.TimingDisplayType.Dual:
                    element.position = new Vector2(-0.84f, 0);
                    element.rotation = 0f;
                    break;
                default:
                    element.position = new Vector2(0, -0.8f);
                    element.rotation = 90f;
                    break;
            }
            element.scale = 1f;
            element.enable = elHide;
            element.PositionElement();
            UpdateLayoutSettings();
        }
    }
}