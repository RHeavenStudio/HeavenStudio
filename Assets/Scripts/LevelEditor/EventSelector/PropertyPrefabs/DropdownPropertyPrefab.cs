using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Common;
using Jukebox;

namespace HeavenStudio.Editor
{
    public class DropdownPropertyPrefab : EventPropertyPrefab
    {
        [Header("Dropdown")]
        [Space(10)]
        public LeftClickTMP_Dropdown dropdown;
        private List<int> keys;

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
            // var currentlySelected = (int)parameterManager.entity[propertyName];
            RiqEntity entity = parameterManager.entity;
            int selected = 0;

            switch (type)
            {
                case EntityTypes.Dropdown dropdownEntity:
                    entity[propertyName].values = dropdownEntity.values;
                    selected = dropdownEntity.defaultValue;
                    break;
                case Enum enumEntity:
                    var enumType = enumEntity.GetType();
                    entity[propertyName] = Enum.GetNames(enumType).ToList();
                    keys = Enum.GetValues(enumType).Cast<int>().ToList();
                    for (int i = 0; i < keys.Count; i++) {
                        print("key " + (i + 1) + " : " + keys[i]);
                    }
                    selected = keys.FindIndex(val => val == (int)entity[propertyName]);
                    break;
                default:
                break;
            }
            dropdown.AddOptions(entity[propertyName].values);
            dropdown.value = selected;

            entity[propertyName].valueChanged = new Action<List<string>>(newValues =>
            {
                print(string.Join(", ", entity[propertyName].values));
                dropdown.ClearOptions();
                dropdown.AddOptions(newValues);
            });

            dropdown.onValueChanged.AddListener(newValue =>
                {
                    parameterManager.entity[propertyName].currentIndex = newValue;
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