using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;


using HeavenStudio.Util;
using HeavenStudio.Editor;
using HeavenStudio.Common;

namespace HeavenStudio.Editor
{
    public class EnumPropertyPrefab : EventPropertyPrefab
    {
        [Header("Dropdown")]
        [Space(10)]
        public LeftClickTMP_Dropdown dropdown;
        private Array enumVals;

        private int _defaultValue;

        private bool openedDropdown = false;

        new public void SetProperties(string propertyName, object type, string caption)
        {
            InitProperties(propertyName, caption);

            var enumType = type.GetType();
            enumVals = Enum.GetValues(enumType);
            var enumNames = Enum.GetNames(enumType).ToList();
            _defaultValue = (int)type;

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
            {
                parameterManager.entity[propertyName] = (int)enumVals.GetValue(dropdown.value);
                if ((int)enumVals.GetValue(dropdown.value) != _defaultValue)
                {
                    this.caption.text = _captionText + "*";
                }
                else
                {
                    this.caption.text = _captionText;
                }
            }
            );
        }

        public void ResetValue()
        {
            dropdown.value = _defaultValue;
        }

        public override void SetCollapses(object type)
        {
            dropdown.onValueChanged.AddListener(_ => UpdateCollapse((int)enumVals.GetValue(dropdown.value)));
            UpdateCollapse((int)enumVals.GetValue(dropdown.value));
        }

        private void Update()
        {
            var scrollbar = GetComponentInChildren<ScrollRect>()?.verticalScrollbar;

            // This is bad but we'll fix it later.
            if (scrollbar != null)
            {
                if (openedDropdown == false)
                {
                    openedDropdown = true;

                    var valuePos = (float)dropdown.value / (dropdown.options.Count - 1);
                    var scrollVal = scrollbar.direction == Scrollbar.Direction.TopToBottom ? valuePos : 1.0f - valuePos;
                    scrollbar.value = Mathf.Max(0.001f, scrollVal);
                }
            }
            else
            {
                openedDropdown = false;
            }
        }
    }
}