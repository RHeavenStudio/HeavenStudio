using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class DispAudioSettings : TabsContent
    {
        public TMP_Dropdown resolutionsDropdown;
        public GameObject customSetter;
        public TMP_InputField widthInputField, heightInputField;

        
        public Slider volSlider;
        public TMP_InputField volLabel;

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
            resolutionsDropdown.value = GlobalGameManager.ScreenSizeIndex;

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

            widthInputField.text = GlobalGameManager.CustomScreenWidth.ToString();
            heightInputField.text = GlobalGameManager.CustomScreenHeight.ToString();

            volSlider.value = GlobalGameManager.MasterVolume;
            volLabel.text = System.Math.Round(volSlider.value * 100, 2).ToString();
        }

        public void WindowFullScreen()
        {
            GlobalGameManager.WindowFullScreen();
        }

        public void WindowConfirmSize()
        {
            GlobalGameManager.ChangeScreenSize();
        }

        public void OnVolSliderChanged()
        {
            GlobalGameManager.ChangeMasterVolume(volSlider.value);
            volLabel.text = System.Math.Round(volSlider.value * 100, 2).ToString();
        }

        public void OnVolLabelChanged()
        {
            volSlider.value = (float)System.Math.Round(System.Convert.ToSingle(volLabel.text) / 100f, 2);
            GlobalGameManager.ChangeMasterVolume(volSlider.value);
        }

        public override void OnOpenTab()
        {
        }

        public override void OnCloseTab()
        {
        }
    }
}