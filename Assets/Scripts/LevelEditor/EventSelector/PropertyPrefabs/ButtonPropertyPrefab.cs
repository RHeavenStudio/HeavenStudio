using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using Jukebox;

namespace HeavenStudio.Editor
{
    public class ButtonPropertyPrefab : EventPropertyPrefab
    {
        [Header("Boolean")]
        [Space(10)]
        // public Button buttonObj;
        public TMP_Text buttonText;
        public EntityTypes.Button button;
        public RiqEntity entity;

        public override void SetProperties(string propertyName, object type, string caption)
        {
            base.SetProperties(propertyName, type, caption);
            entity = parameterManager.entity;

            if (type is EntityTypes.Button button) {
                this.button = button;
                buttonText.text = button.defaultLabel;
            } else {
                Debug.LogError("ButtonPropertyPrefab was unable to use " + type.GetType() + " as a Button.");
                return;
            }
        }

        public void OnClick()
        {
            string text = button.onClick.Invoke(entity);
            if (text != null) {
                buttonText.text = text;
            }
        }
    }
}