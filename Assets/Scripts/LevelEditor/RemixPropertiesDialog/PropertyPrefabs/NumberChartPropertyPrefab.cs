using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using Starpelly;

using HeavenStudio.Util;
using HeavenStudio.Editor;

namespace HeavenStudio.Editor
{
    public class NumberChartPropertyPrefab : RemixPropertyPrefab
    {
        [Header("Integer and Float")]
        [Space(10)]
        public Slider slider;
        public TMP_InputField inputField;

        new public void SetProperties(RemixPropertiesDialog diag, string propertyName, object type, string caption)
        {
            InitProperties(diag, propertyName, caption);

            switch (type)
            {
                case EntityTypes.Integer integer:
                    slider.minValue = integer.min;
                    slider.maxValue = integer.max;

                    slider.wholeNumbers = true;
                    slider.value = Convert.ToSingle(parameterManager.chart[propertyName]);
                    inputField.text = slider.value.ToString();

                    slider.onValueChanged.AddListener(
                        _ =>
                        {
                            inputField.text = slider.value.ToString();
                            parameterManager.chart[propertyName] = (int) slider.value;
                        }
                    );

                    inputField.onEndEdit.AddListener(
                        _ =>
                        {
                            slider.value = Convert.ToSingle(inputField.text);
                            parameterManager.chart[propertyName] = (int) slider.value;
                        }
                    );
                    break;

                case EntityTypes.Float fl:
                    slider.minValue = fl.min;
                    slider.maxValue = fl.max;

                    slider.value = Convert.ToSingle(parameterManager.chart[propertyName]);
                    inputField.text = slider.value.ToString("G");

                    slider.onValueChanged.AddListener(
                        _ =>
                        {
                            var newValue = (float) Math.Round(slider.value, 4);
                            inputField.text = newValue.ToString("G");
                            parameterManager.chart[propertyName] = newValue;
                        }
                    );

                    inputField.onEndEdit.AddListener(
                        _ =>
                        {
                            slider.value = (float) Math.Round(Convert.ToSingle(inputField.text), 4);
                            parameterManager.chart[propertyName] = slider.value;
                        }
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
        }
    }
}