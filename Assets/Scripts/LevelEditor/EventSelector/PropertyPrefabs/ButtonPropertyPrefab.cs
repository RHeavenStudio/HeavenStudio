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
    public class ButtonPropertyPrefab : EventPropertyPrefab
    {
        [Header("Boolean")]
        [Space(10)]
        // public Button buttonObj;
        public EntityTypes.Button button;

        new public void SetProperties(string propertyName, object type, string caption)
        {
            InitProperties(propertyName, caption);

            if (type is EntityTypes.Button button) {
                this.button = button;
            } else {
                Debug.LogError("ButtonPropertyPrefab was unable to use " + type.GetType() + " as a Button.");
                return;
            }
        }

        public void OnClick()
        {
            button.onClick.Invoke();
        }
    }
}