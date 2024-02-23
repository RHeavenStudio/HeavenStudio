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
        public Scrollbar scrollbar;

        private int _defaultValue;

        private bool openedDropdown = false;

        public override void SetProperties(string propertyName, object type, string caption)
        {
            base.SetProperties(propertyName, type, caption);

            int selected = 0;

            switch (type)
            {
                case EntityTypes.Dropdown dropdownEntity:
                    // entity[propertyName].ChangeValues(dropdownEntity.Values);
                    _defaultValue = dropdownEntity.defaultValue;
                    EntityTypes.DropdownObj dropdownObj = entity[propertyName];
                    selected = dropdownObj.value;
                    dropdown.AddOptions(dropdownObj.Values);
                    dropdown.onValueChanged.AddListener(newVal => dropdownObj.value = newVal);
                    dropdownObj.onValueChanged = new Action<List<string>>(newValues =>
                    {
                        if (dropdown == null) return;
                        dropdown.ClearOptions();
                        dropdown.AddOptions(newValues);
                        dropdown.enabled = newValues.Count > 0;
                        dropdownObj.value = _defaultValue;
                    });
                    break;
                case Enum enumEntity:
                    Type enumType = enumEntity.GetType();
                    _defaultValue = (int)type;
                    int[] keys = Enum.GetValues(enumType).Cast<int>().ToArray();
                    selected = Array.FindIndex(keys, val => val == (int)entity[propertyName]);

                    dropdown.AddOptions(Enum.GetNames(enumType).ToList());
                    dropdown.onValueChanged.AddListener(val => entity[propertyName] = keys[val]);
                    break;
                default:
                break;
            }
            dropdown.value = selected;
            dropdown.enabled = dropdown.options.Count > 0;

            dropdown.onValueChanged.AddListener(newValue => {
                this.caption.text = (newValue != _defaultValue) ? (_captionText + "*") : _captionText;
            });
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