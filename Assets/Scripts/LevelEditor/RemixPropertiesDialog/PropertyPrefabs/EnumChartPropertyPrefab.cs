using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

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

            Type enumType = type.GetType();
            Array enumVals = Enum.GetValues(enumType);
            List<string> enumNames = Enum.GetNames(enumType).ToList();

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
                parameterManager.chart[propertyName] = Enum.ToObject(enumType, (int) enumVals.GetValue(dropdown.value))
            );
        }

        private void Update()
        {
        }
    }
}