using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;


using HeavenStudio.Util;
using HeavenStudio.Editor;
using UnityEngine.UIElements;

namespace HeavenStudio.Editor
{
    public class StringPropertyPrefab : EventPropertyPrefab
    {
        [Header("String")]  //why wasn't this a thing before
        [Space(10)]
        public TMP_InputField inputFieldString;

        private string _defaultValue;

        public override void SetProperties(string propertyName, object type, string caption)
        {
            base.SetProperties(propertyName, type, caption);

            _defaultValue = (string)type;

            inputFieldString.text = (string)entity[propertyName];

            inputFieldString.onSelect.AddListener(
                _ =>
                    Editor.instance.editingInputField = true
            );
            inputFieldString.onValueChanged.AddListener(
                _ =>
                {
                    entity[propertyName] = inputFieldString.text;
                    this.caption.text = (inputFieldString.text != _defaultValue) ? (_captionText + "*") : _captionText;
                }
            );

            inputFieldString.onEndEdit.AddListener(
                _ =>
                {
                    Editor.instance.editingInputField = false;
                }
            );
        }

        public void ResetValue()
        {
            inputFieldString.text = _defaultValue;
        }

        public override void SetCollapses(object type)
        {
            inputFieldString.onValueChanged.AddListener(newVal => UpdateCollapse(newVal));
            UpdateCollapse(inputFieldString.text);
        }

        private void LateUpdate()
        {
            if (entity[propertyName] != inputFieldString.text) {
                inputFieldString.text =entity[propertyName];
            }
        }
    }
}