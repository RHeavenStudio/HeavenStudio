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
    public class EnumPropertyPrefab : EventPropertyPrefab
    {
        [Header("Dropdown")]
        [Space(10)]
        public TMP_Dropdown dropdown;
        private Array enumVals;

        new public void SetProperties(string propertyName, object type, string caption)
        {
            InitProperties(propertyName, caption);

            var enumType = type.GetType();
            enumVals = Enum.GetValues(enumType);
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
        }

        public override void SetCollapses(object type)
        {
            dropdown.onValueChanged.AddListener(_ => UpdateCollapse((int)enumVals.GetValue(dropdown.value)));
            UpdateCollapse((int)enumVals.GetValue(dropdown.value));
        }

        private void Update()
        {
        }
    }
}