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
    public class StringPropertyPrefab : EventPropertyPrefab
    {
        [Header("String")]  //why wasn't this a thing before
        [Space(10)]
        public TMP_InputField inputFieldString;

        new public void SetProperties(string propertyName, object type, string caption)
        {
            InitProperties(propertyName, caption);

            inputFieldString.text = (string) parameterManager.entity[propertyName];

            inputFieldString.onSelect.AddListener(
                _ =>
                    Editor.instance.editingInputField = true
            );
            inputFieldString.onValueChanged.AddListener(
                _ =>
                {;
                    parameterManager.entity[propertyName] = inputFieldString.text;
                }
            );
            inputFieldString.onEndEdit.AddListener(
                _ =>
                {;
                    Editor.instance.editingInputField = false;
                }
            );
        }

        public override void SetCollapses(object type)
        {
            inputFieldString.onValueChanged.AddListener(
                _ =>
                {
                    UpdateCollapse(inputFieldString.text);
                });
            UpdateCollapse(inputFieldString.text);
        }

        private void Update()
        {
        }
    }
}