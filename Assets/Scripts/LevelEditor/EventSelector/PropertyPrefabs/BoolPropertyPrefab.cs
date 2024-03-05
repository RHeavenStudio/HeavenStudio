using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

namespace HeavenStudio.Editor
{
    public class BoolPropertyPrefab : EventPropertyPrefab
    {
        [Header("Boolean")]
        [Space(10)]
        public Toggle toggle;

        private bool _defaultValue;

        public override void SetProperties(string propertyName, object type, string caption)
        {
            base.SetProperties(propertyName, type, caption);

            _defaultValue = (bool)type;
            toggle.isOn = Convert.ToBoolean(entity[propertyName]);

            toggle.onValueChanged.AddListener(_ =>
            {
                entity[propertyName] = toggle.isOn;
                this.caption.text = (toggle.isOn != _defaultValue) ? (_captionText + "*") : _captionText;
            });
        }

        public void ResetValue()
        {
            toggle.isOn = _defaultValue;
        }

        public override void SetCollapses(object type)
        {
            toggle.onValueChanged.AddListener(newVal => UpdateCollapse(newVal));
            UpdateCollapse(toggle.isOn);
        }
    }
}