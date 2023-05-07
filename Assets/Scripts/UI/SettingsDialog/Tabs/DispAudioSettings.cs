using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using HeavenStudio.Common;
namespace HeavenStudio.Editor 
{
    public class DispAudioSettings : TabsContent
    {
        [SerializeField] Toggle splashScreenToggle;
        public TMP_Dropdown resolutionsDropdown;
        public GameObject customSetter;
        public TMP_InputField widthInputField, heightInputField;

        
        public Slider volSlider;
        public TMP_InputField volLabel;
        public TMP_Dropdown dspSizeDropdown;
        public TMP_Dropdown sampleRateDropdown;

        private void Start() {
            List<TMP_Dropdown.OptionData> dropDownData = new List<TMP_Dropdown.OptionData>();
            var vals = GlobalGameManager.DEFAULT_SCREEN_SIZES_STRING;
            for (int i = 0; i < vals.Length; i++)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = vals[i];
                dropDownData.Add(optionData);
            }
            resolutionsDropdown.AddOptions(dropDownData);

            resolutionsDropdown.onValueChanged.AddListener(delegate 
            {
                GlobalGameManager.ScreenSizeIndex = resolutionsDropdown.value;

                customSetter.SetActive(resolutionsDropdown.value == GlobalGameManager.DEFAULT_SCREEN_SIZES_STRING.Length - 1);
            });

            widthInputField.onEndEdit.AddListener(delegate 
            {
                GlobalGameManager.CustomScreenWidth = System.Math.Max(int.Parse(widthInputField.text), 64);
                widthInputField.text = GlobalGameManager.CustomScreenWidth.ToString();
            });
            heightInputField.onEndEdit.AddListener(delegate 
            {
                GlobalGameManager.CustomScreenHeight = System.Math.Max(int.Parse(heightInputField.text), 64);
                heightInputField.text = GlobalGameManager.CustomScreenHeight.ToString();
            });

            resolutionsDropdown.value = GlobalGameManager.ScreenSizeIndex;

            widthInputField.text = GlobalGameManager.CustomScreenWidth.ToString();
            heightInputField.text = GlobalGameManager.CustomScreenHeight.ToString();

            volSlider.value = GlobalGameManager.MasterVolume;
            volLabel.text = System.Math.Round(volSlider.value * 100, 2).ToString();

            dspSizeDropdown.onValueChanged.AddListener(delegate 
            {
                GlobalGameManager.currentDspSize = GlobalGameManager.DSP_BUFFER_SIZES[dspSizeDropdown.value];
                GlobalGameManager.ChangeAudioSettings(GlobalGameManager.currentDspSize, GlobalGameManager.currentSampleRate);
            });

            sampleRateDropdown.onValueChanged.AddListener(delegate 
            {
                GlobalGameManager.currentSampleRate = GlobalGameManager.SAMPLE_RATES[sampleRateDropdown.value];
                GlobalGameManager.ChangeAudioSettings(GlobalGameManager.currentDspSize, GlobalGameManager.currentSampleRate);
            });
        }

        public void WindowFullScreen()
        {
            GlobalGameManager.WindowFullScreen();
            GlobalGameManager.ResetGameRenderTexture();
        }

        public void WindowConfirmSize()
        {
            GlobalGameManager.ChangeScreenSize();
            GlobalGameManager.ResetGameRenderTexture();
        }

        public void OnVolSliderChanged()
        {
            GlobalGameManager.ChangeMasterVolume(volSlider.value);
            volLabel.text = System.Math.Round(volSlider.value * 100, 2).ToString();
            PersistentDataManager.gameSettings.masterVolume = volSlider.value;
        }

        public void OnVolLabelChanged()
        {
            volSlider.value = (float)System.Math.Round(System.Convert.ToSingle(volLabel.text) / 100f, 2);
            GlobalGameManager.ChangeMasterVolume(volSlider.value);
            PersistentDataManager.gameSettings.masterVolume = volSlider.value;
        }

        public void OnSplashChanged()
        {
            PersistentDataManager.gameSettings.showSplash = splashScreenToggle.isOn;
        }

        public override void OnOpenTab()
        {
            splashScreenToggle.isOn = PersistentDataManager.gameSettings.showSplash;
            resolutionsDropdown.value = GlobalGameManager.ScreenSizeIndex;

            widthInputField.text = GlobalGameManager.CustomScreenWidth.ToString();
            heightInputField.text = GlobalGameManager.CustomScreenHeight.ToString();

            volSlider.value = GlobalGameManager.MasterVolume;
            volLabel.text = System.Math.Round(volSlider.value * 100, 2).ToString();

            dspSizeDropdown.ClearOptions();
            sampleRateDropdown.ClearOptions();
            dspSizeDropdown.AddOptions(GlobalGameManager.DSP_BUFFER_SIZES.Select(x => x.ToString()).ToList());
            sampleRateDropdown.AddOptions(GlobalGameManager.SAMPLE_RATES.Select(x => x.ToString()).ToList());
            dspSizeDropdown.value = GlobalGameManager.DSP_BUFFER_SIZES.ToList().IndexOf(GlobalGameManager.currentDspSize);
            sampleRateDropdown.value = GlobalGameManager.SAMPLE_RATES.ToList().IndexOf(GlobalGameManager.currentSampleRate);
        }

        public override void OnCloseTab()
        {
        }
    }
}