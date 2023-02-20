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
    public class StringChartPropertyPrefab : RemixPropertyPrefab
    {
        [Header("String")]  //why wasn't this a thing before
        [Space(10)]
        public TMP_InputField inputFieldString;

        new public void SetProperties(RemixPropertiesDialog diag, string propertyName, object type, string caption)
        {
            InitProperties(diag, propertyName, caption);

            inputFieldString.text = (string) parameterManager.chart[propertyName];

            inputFieldString.onSelect.AddListener(
                _ =>
                    Editor.instance.editingInputField = true
            );
            inputFieldString.onEndEdit.AddListener(
                _ =>
                {;
                    parameterManager.chart[propertyName] = inputFieldString.text;
                    Editor.instance.editingInputField = false;
                }
            );
        }

        private void Update()
        {
        }
    }
}