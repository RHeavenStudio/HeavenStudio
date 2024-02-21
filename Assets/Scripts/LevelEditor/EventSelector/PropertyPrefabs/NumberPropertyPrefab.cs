using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

namespace HeavenStudio.Editor
{
    public class NumberPropertyPrefab : EventPropertyPrefab
    {
        [Header("Integer and Float")]
        [Space(10)]
        public Slider slider;
        public TMP_InputField inputField;

        private float _defaultValue;

        public override void SetProperties(string propertyName, object type, string caption)
        {
            base.SetProperties(propertyName, type, caption);

            switch (type)
            {
                case EntityTypes.Integer integer:
                    slider.minValue = integer.min;
                    slider.maxValue = integer.max;
                    _defaultValue = integer.val;

                    slider.wholeNumbers = true;
                    slider.value = Convert.ToSingle(parameterManager.entity[propertyName]);
                    inputField.text = slider.value.ToString();

                    slider.onValueChanged.AddListener(
                        _ =>
                        {
                            inputField.text = slider.value.ToString();
                            parameterManager.entity[propertyName] = (int) slider.value;
                            if (slider.value != _defaultValue)
                            {
                                this.caption.text = _captionText + "*";
                            }
                            else
                            {
                                this.caption.text = _captionText;
                            }
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
                            if (slider.value != _defaultValue)
                            {
                                this.caption.text = _captionText + "*";
                            }
                            else
                            {
                                this.caption.text = _captionText;
                            }
                        }
                    );
                    break;

                case EntityTypes.Float fl:
                    slider.minValue = fl.min;
                    slider.maxValue = fl.max;
                    _defaultValue = fl.val;

                    slider.value = Convert.ToSingle(parameterManager.entity[propertyName]);
                    inputField.text = slider.value.ToString("G");

                    slider.onValueChanged.AddListener(
                        _ =>
                        {
                            var newValue = (float) Math.Round(slider.value, 4);
                            inputField.text = newValue.ToString("G");
                            parameterManager.entity[propertyName] = newValue;
                            if (newValue != _defaultValue)
                            {
                                this.caption.text = _captionText + "*";
                            }
                            else
                            {
                                this.caption.text = _captionText;
                            }
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
                            if (slider.value != _defaultValue)
                            {
                                this.caption.text = _captionText + "*";
                            }
                            else
                            {
                                this.caption.text = _captionText;
                            }
                        }
                    );
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type), type, "I don't know how to make a property of this type!"
                    );
            }
        }

        public void ResetValue()
        {
            slider.value = _defaultValue;
        }

        public override void SetCollapses(object type)
        {
            switch (type)
            {
                case EntityTypes.Integer integer:
                    slider.onValueChanged.AddListener(_ => UpdateCollapse((int)slider.value));
                    inputField.onEndEdit.AddListener(_ => UpdateCollapse((int)slider.value));

                    UpdateCollapse((int)slider.value);

                    break;

                case EntityTypes.Float fl:
                    slider.onValueChanged.AddListener(newVal => UpdateCollapse((float)Math.Round(newVal, 4)));
                    inputField.onEndEdit.AddListener(_ => UpdateCollapse(slider.value));

                    UpdateCollapse((float)Math.Round(slider.value, 4));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type), type, "I don't know how to make a property of this type!"
                    );
            }
        }

        private void Update()
        {
        }
    }
}