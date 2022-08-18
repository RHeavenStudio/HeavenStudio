using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using TMPro;
using Starpelly;

using HeavenStudio.Util;

namespace HeavenStudio.Properties
{
    public class PropertyDefn : MonoBehaviour
    {

        public TMP_Text caption;
        [SerializeField] private PropManager parameterManager;

        [Header("Integer and Float")]
        [Space(10)]
        public Slider slider;
        public TMP_InputField inputField;

        [Header("Boolean")]
        [Space(10)]
        public Toggle toggle;

        [Header("Dropdown")]
        [Space(10)]
        public TMP_Dropdown dropdown;

        [Header("String")]  //why wasn't this a thing before
        [Space(10)]
        public TMP_InputField inputFieldString;

        private string propertyName;

        public void SetProperties(string propertyName, object type, string caption)
        {
            this.propertyName = propertyName;
            this.caption.text = caption;

            var objType = type.GetType();

            if (objType == typeof(EntityTypes.Integer))
            {
                var integer = ((EntityTypes.Integer)type);

                slider.minValue = integer.min;
                slider.maxValue = integer.max;

                slider.value = Mathf.RoundToInt(System.Convert.ToSingle(PropController.properties.instance[propertyName]));
                inputField.text = slider.value.ToString();

                slider.onValueChanged.AddListener(delegate
                {
                    inputField.text = slider.value.ToString();
                    PropController.properties.instance[propertyName] = (int)slider.value;
                });

                inputField.onSelect.AddListener(delegate
                {
                    Editor.instance.editingInputField = true;
                });

                inputField.onEndEdit.AddListener(delegate
                {
                    slider.value = Mathf.RoundToInt(System.Convert.ToSingle(System.Convert.ToSingle(inputField.text)));
                    PropController.properties.instance[propertyName] = (int)slider.value;
                    Editor.instance.editingInputField = false;
                });
            }
            else if (type is bool)
            {
                toggle.isOn = System.Convert.ToBoolean(PropController.properties.instance[propertyName]); // ' (bool)type ' always results in false

                toggle.onValueChanged.AddListener(delegate
                {
                    PropController.properties.instance[propertyName] = toggle.isOn;
                });
            }
            else if (objType.IsEnum)
            {
                List<TMP_Dropdown.OptionData> dropDownData = new List<TMP_Dropdown.OptionData>();
                var vals = Enum.GetValues(objType);
                var selected = 0;
                for (int i = 0; i < vals.Length; i++)
                {
                    string name = Enum.GetNames(objType)[i];
                    TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();

                    optionData.text = name;

                    dropDownData.Add(optionData);

                    if ((int)vals.GetValue(i) == (int)PropController.properties.instance[propertyName])
                        selected = i;
                }
                dropdown.AddOptions(dropDownData);
                dropdown.value = selected;

                dropdown.onValueChanged.AddListener(delegate
                {
                    PropController.properties.instance[propertyName] = (int)Enum.GetValues(objType).GetValue(dropdown.value);
                });
            }
            //why the FUCK wasn't this a thing before lmao
            else if (objType == typeof(string))
            {
                // Debug.Log("entity " + propertyName + " is: " + (string)(PropController.properties.instance[propertyName]));
                inputFieldString.text = (string)(PropController.properties.instance[propertyName]);

                inputFieldString.onSelect.AddListener(delegate
                {
                    Editor.instance.editingInputField = true;
                });

                inputFieldString.onEndEdit.AddListener(delegate
                {
                    // Debug.Log("setting " + propertyName + " to: " + inputFieldString.text);
                    PropController.properties.instance[propertyName] = inputFieldString.text;
                    Editor.instance.editingInputField = false;
                });
            }
        }
    }
}