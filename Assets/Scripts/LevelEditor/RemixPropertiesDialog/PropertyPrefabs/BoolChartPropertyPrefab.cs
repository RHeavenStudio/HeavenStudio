using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;


using HeavenStudio.Util;
using HeavenStudio.Editor;

namespace HeavenStudio.Editor
{
    public class BoolChartPropertyPrefab : RemixPropertyPrefab
    {
        [Header("Boolean")]
        [Space(10)]
        public Toggle toggle;

        new public void SetProperties(RemixPropertiesDialog diag, string propertyName, object type, string caption)
        {
            InitProperties(diag, propertyName, caption);

            // ' (bool)type ' always results in false
            toggle.isOn = Convert.ToBoolean(parameterManager.chart[propertyName]);

            toggle.onValueChanged.AddListener(
                _ => parameterManager.chart[propertyName] = toggle.isOn
            );
        }

        private void Update()
        {
        }
    }
}