using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using TMPro;
using Starpelly;

using HeavenStudio.Util;

namespace HeavenStudio.Editor
{
    public class EventPropertyPrefab : MonoBehaviour
    {
        public TMP_Text caption;
        [SerializeField] private EventParameterManager parameterManager;

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

        [Header("Color")]
        [Space(10)]
        public Button ColorBTN;
        public RectTransform ColorTable;
        public bool colorTableActive;
        public ColorPreview colorPreview;

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

                slider.value = Mathf.RoundToInt(System.Convert.ToSingle(parameterManager.entity[propertyName]));
                inputField.text = slider.value.ToString();

                slider.onValueChanged.AddListener(delegate 
                {
                    inputField.text = slider.value.ToString();
                    parameterManager.entity[propertyName] = (int)slider.value;
                });

                inputField.onSelect.AddListener(delegate
                {
                    Editor.instance.editingInputField = true;
                });

                inputField.onEndEdit.AddListener(delegate
                {
                    slider.value = Mathf.RoundToInt(System.Convert.ToSingle(System.Convert.ToSingle(inputField.text)));
                    parameterManager.entity[propertyName] = (int)slider.value;
                    Editor.instance.editingInputField = false;
                });
            }
            else if (objType == typeof(EntityTypes.Float))
            {
                var fl = ((EntityTypes.Float)type);

                slider.minValue = fl.min;
                slider.maxValue = fl.max;

                slider.value = System.Convert.ToSingle(parameterManager.entity[propertyName]);
                inputField.text = slider.value.ToString("G");

                slider.onValueChanged.AddListener(delegate 
                {
                    var newValue = (float)System.Math.Round(slider.value, 4);
                    inputField.text = newValue.ToString("G");
                    parameterManager.entity[propertyName] = newValue;
                });

                inputField.onSelect.AddListener(delegate
                {
                    Editor.instance.editingInputField = true;
                });

                inputField.onEndEdit.AddListener(delegate 
                {
                    slider.value = (float)System.Math.Round(System.Convert.ToSingle(inputField.text), 4);
                    parameterManager.entity[propertyName] = slider.value;
                    Editor.instance.editingInputField = false;
                });
            }
            else if(type is bool)
            {
                toggle.isOn = System.Convert.ToBoolean(parameterManager.entity[propertyName]); // ' (bool)type ' always results in false

                toggle.onValueChanged.AddListener(delegate
                {
                    parameterManager.entity[propertyName] = toggle.isOn;
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

                    if ((int)vals.GetValue(i) == (int)parameterManager.entity[propertyName])
                        selected = i;
                }
                dropdown.AddOptions(dropDownData);
                dropdown.value = selected;
                
                dropdown.onValueChanged.AddListener(delegate 
                {
                    parameterManager.entity[propertyName] = (int)Enum.GetValues(objType).GetValue(dropdown.value);
                });
            }
            else if (objType == typeof(Color))
            {
                colorPreview.colorPicker.onColorChanged += delegate
                {
                    parameterManager.entity[propertyName] = (Color)colorPreview.colorPicker.color;
                };

                Color paramCol = (Color)parameterManager.entity[propertyName];

                ColorBTN.onClick.AddListener(delegate
                {
                    ColorTable.gameObject.SetActive(true);
                    colorTableActive = true;
                    colorPreview.ChangeColor(paramCol);
                });

                colorPreview.ChangeColor(paramCol);
                ColorTable.gameObject.SetActive(false);
            }
            //why the FUCK wasn't this a thing before lmao
            else if(objType == typeof(string))
            {
                // Debug.Log("entity " + propertyName + " is: " + (string)(parameterManager.entity[propertyName]));
                inputFieldString.text = (string)(parameterManager.entity[propertyName]);

                inputFieldString.onSelect.AddListener(delegate
                {
                    Editor.instance.editingInputField = true;
                });

                inputFieldString.onEndEdit.AddListener(delegate
                {
                    // Debug.Log("setting " + propertyName + " to: " + inputFieldString.text);
                    parameterManager.entity[propertyName] = inputFieldString.text;
                    Editor.instance.editingInputField = false;
                });
            }
        }

        private void Update()
        {
            if (colorTableActive)
            {
                if (!Editor.MouseInRectTransform(ColorTable))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        ColorTable.gameObject.SetActive(false);
                        colorTableActive = false;
                    }
                }
            }
        }
    }
}