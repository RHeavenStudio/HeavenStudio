using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using Starpelly;

using HeavenStudio.Util;

namespace HeavenStudio.Editor
{
    public class EventPropertyPrefab : MonoBehaviour
    {
        public TMP_Text caption;
        public EventParameterManager parameterManager;
        public string propertyName;

        public void SetProperties(string propertyName, object type, string caption) {}

        public void InitProperties(string propertyName, string caption)
        {
            this.parameterManager = EventParameterManager.instance;
            this.propertyName = propertyName;
            this.caption.text = caption;
        }
    }
}