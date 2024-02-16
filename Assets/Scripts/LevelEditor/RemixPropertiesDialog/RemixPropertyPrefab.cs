using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;


using HeavenStudio.Util;

namespace HeavenStudio.Editor
{
    public class RemixPropertyPrefab : MonoBehaviour
    {
        public TMP_Text caption;
        public RemixPropertiesDialog parameterManager;
        public string propertyName;

        public void SetProperties(RemixPropertiesDialog diag, string propertyName, object type, string caption) {}

        public void InitProperties(RemixPropertiesDialog diag, string propertyName, string caption)
        {
            this.parameterManager = diag;
            this.propertyName = propertyName;
            this.caption.text = caption;
        }
    }
}