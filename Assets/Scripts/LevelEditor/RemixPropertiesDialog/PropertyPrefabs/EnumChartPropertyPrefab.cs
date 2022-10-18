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
    public class EnumChartPropertyPrefab : RemixPropertyPrefab
    {
        [Header("Dropdown")]
        [Space(10)]
        public TMP_Dropdown dropdown;

        new public void SetProperties(RemixPropertiesDialog diag, string propertyName, object type, string caption)
        {
            InitProperties(diag, propertyName, caption);

            var enumType = type.GetType();
            var enumVals = Enum.GetValues(enumType);
            var enumNames = Enum.GetNames(enumType).ToList();

            // Can we assume non-holey enum?
            // If we can we can simplify to dropdown.value = (int) parameterManager.chart[propertyName]
            var currentlySelected = (int) parameterManager.chart[propertyName];
            var selected = enumVals
                .Cast<object>()
                .ToList()
                .FindIndex(val => (int) val == currentlySelected);

            dropdown.AddOptions(enumNames);
            dropdown.value = selected;

            dropdown.onValueChanged.AddListener(_ =>
                parameterManager.chart[propertyName] = (int) enumVals.GetValue(dropdown.value)
            );
        }

        private void Update()
        {
        }
    }
}