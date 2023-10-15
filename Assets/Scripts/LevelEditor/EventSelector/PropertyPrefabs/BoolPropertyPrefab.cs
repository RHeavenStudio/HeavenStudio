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
    public class BoolPropertyPrefab : EventPropertyPrefab
    {
        [Header("Boolean")]
        [Space(10)]
        public Toggle toggle;

        new public void SetProperties(string propertyName, object type, string caption)
        {
            InitProperties(propertyName, caption);

            // ' (bool)type ' always results in false
            toggle.isOn = Convert.ToBoolean(parameterManager.entity[propertyName]);

            toggle.onValueChanged.AddListener(
                _ => parameterManager.entity[propertyName] = toggle.isOn
            );
        }

        private void Update()
        {
        }
    }
}