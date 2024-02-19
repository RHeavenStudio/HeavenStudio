using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

using HeavenStudio.Common;

namespace HeavenStudio.Editor
{
    public class DropdownPropertyPrefab : EventPropertyPrefab
    {
        [Header("Dropdown")]
        [Space(10)]
        public LeftClickTMP_Dropdown dropdown;
        // private string[] values;
        private List<string> names;
        private List<(int, string)> values;

        private int _defaultValue;

        private bool openedDropdown = false;

        public override void SetProperties(string propertyName, object type, string caption)
        {
            base.SetProperties(propertyName, type, caption);

            // var enumType = type.GetType();
            // values = Enum.GetValues(enumType).Cast<string>().ToArray();
            // names = Enum.GetNames(enumType).ToList();
            // _defaultValue = (int)type;

            // Can we assume non-holey enum?
            // If we can we can simplify to dropdown.value = (int) parameterManager.entity[propertyName]
            // var currentlySelected = (int) parameterManager.entity[propertyName];
            // var selected = values
            //     .Cast<object>()
            //     .ToList()
            //     .FindIndex(val => (int) val == currentlySelected);

            if (type is not EntityTypes.Dropdown dropdownEntity) return;

            names = dropdownEntity.values;
            dropdown.AddOptions(names);
            dropdown.value = (int)parameterManager.entity[propertyName];

            dropdown.onValueChanged.AddListener(newValue =>
                {
                    parameterManager.entity[propertyName] = newValue;
                    Debug.Log("newValue : " + newValue);
                    this.caption.text = (newValue != _defaultValue) ? (_captionText + "*") : _captionText;
                }
            );
        }

        public void ResetValue()
        {
            dropdown.value = _defaultValue;
        }

        public override void SetCollapses(object type)
        {
            dropdown.onValueChanged.AddListener(_ => UpdateCollapse(type));
            UpdateCollapse(type);
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