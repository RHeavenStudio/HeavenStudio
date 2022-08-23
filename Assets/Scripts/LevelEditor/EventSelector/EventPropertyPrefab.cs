using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
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

            switch (type)
            {
                case EntityTypes.Integer integer:
                    slider.minValue = integer.min;
                    slider.maxValue = integer.max;

                    slider.wholeNumbers = true;
                    slider.value = Convert.ToSingle(parameterManager.entity[propertyName]);
                    inputField.text = slider.value.ToString();

                    slider.onValueChanged.AddListener(
                        _ =>
                        {
                            inputField.text = slider.value.ToString();
                            parameterManager.entity[propertyName] = (int) slider.value;
                        }
                    );

                    inputField.onSelect.AddListener(
                        _ =>
                            Editor.instance.editingInputField = true
                    );

                    inputField.onEndEdit.AddListener(
                        _ =>
                        {
                            slider.value = Convert.ToSingle(inputField.text);
                            parameterManager.entity[propertyName] = (int) slider.value;
                            Editor.instance.editingInputField = false;
                        }
                    );
                    break;

                case EntityTypes.Float fl:
                    slider.minValue = fl.min;
                    slider.maxValue = fl.max;

                    slider.value = Convert.ToSingle(parameterManager.entity[propertyName]);
                    inputField.text = slider.value.ToString("G");

                    slider.onValueChanged.AddListener(
                        _ =>
                        {
                            var newValue = (float) Math.Round(slider.value, 4);
                            inputField.text = newValue.ToString("G");
                            parameterManager.entity[propertyName] = newValue;
                        }
                    );

                    inputField.onSelect.AddListener(
                        _ =>
                            Editor.instance.editingInputField = true
                    );

                    inputField.onEndEdit.AddListener(
                        _ =>
                        {
                            slider.value = (float) Math.Round(Convert.ToSingle(inputField.text), 4);
                            parameterManager.entity[propertyName] = slider.value;
                            Editor.instance.editingInputField = false;
                        }
                    );
                    break;

                case bool _:
                    // ' (bool)type ' always results in false
                    toggle.isOn = Convert.ToBoolean(parameterManager.entity[propertyName]);

                    toggle.onValueChanged.AddListener(
                        _ => parameterManager.entity[propertyName] = toggle.isOn
                    );
                    break;

                case Color _:
                    colorPreview.colorPicker.onColorChanged += _ =>
                        parameterManager.entity[propertyName] = colorPreview.colorPicker.color;

                    var paramCol = (Color) parameterManager.entity[propertyName];

                    ColorBTN.onClick.AddListener(
                        () =>
                        {
                            ColorTable.gameObject.SetActive(true);
                            colorTableActive = true;
                            colorPreview.ChangeColor(paramCol);
                        }
                    );

                    colorPreview.ChangeColor(paramCol);
                    ColorTable.gameObject.SetActive(false);
                    break;

                case string _:
                    inputFieldString.text = (string) parameterManager.entity[propertyName];

                    inputFieldString.onSelect.AddListener(
                        _ =>
                            Editor.instance.editingInputField = true
                    );
                    inputFieldString.onEndEdit.AddListener(
                        _ =>
                        {;
                            parameterManager.entity[propertyName] = inputFieldString.text;
                            Editor.instance.editingInputField = false;
                        }
                    );
                    break;

                case Enum enumKind:
                    var enumType = enumKind.GetType();
                    var enumVals = Enum.GetValues(enumType);
                    var enumNames = Enum.GetNames(enumType).ToList();

                    // Can we assume non-holey enum?
                    // If we can we can simplify to dropdown.value = (int) parameterManager.entity[propertyName]
                    var currentlySelected = (int) parameterManager.entity[propertyName];
                    var selected = enumVals
                        .Cast<object>()
                        .ToList()
                        .FindIndex(val => (int) val == currentlySelected);

                    dropdown.AddOptions(enumNames);
                    dropdown.value = selected;

                    dropdown.onValueChanged.AddListener(_ =>
                        parameterManager.entity[propertyName] = (int) enumVals.GetValue(dropdown.value)
                    );
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type), type, "I don't know how to make a property of this type!"
                    );
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