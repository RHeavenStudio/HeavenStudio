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

        private bool _defaultValue;

        new public void SetProperties(string propertyName, object type, string caption)
        {
            InitProperties(propertyName, caption);

            _defaultValue = (bool)type;
            toggle.isOn = Convert.ToBoolean(parameterManager.entity[propertyName]);

            toggle.onValueChanged.AddListener(
                _ =>
                {
                    parameterManager.entity[propertyName] = toggle.isOn;
                    if (toggle.isOn != _defaultValue)
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
            toggle.isOn = _defaultValue;
        }

        public override void SetCollapses(object type)
        {
            toggle.onValueChanged.AddListener(
             _ => UpdateCollapse(toggle.isOn)
            );
            UpdateCollapse(toggle.isOn);
        }
    }
}