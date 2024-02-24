using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace HeavenStudio.Editor
{
    public class ButtonPropertyPrefab : EventPropertyPrefab
    {
        [Header("Boolean")]
        [Space(10)]
        public float minButtonSize;
        public RectTransform buttonTextRect;
        public RectTransform buttonRect;
        public TMP_Text buttonText;
        public EntityTypes.Button button;

        public override void SetProperties(string propertyName, object type, string caption)
        {
            base.SetProperties(propertyName, type, caption);

            if (type is EntityTypes.Button button) {
                this.button = button;
                buttonText.text = entity[propertyName];
            } else {
                Debug.LogError("ButtonPropertyPrefab was unable to use " + type.GetType() + " as a Button.");
                return;
            }
        }

        public void OnClick()
        {
            string text = button.onClick.Invoke(entity);
            if (text != null) {
                buttonText.text = entity[propertyName] = text;
            }
            UpdateCollapse(entity[propertyName]);
        }

        private void LateUpdate()
        {
            // OnClick() already handles this. 
            // if somebody wants to uncomment this for their thing feel free but it's unused for now -AJ
            // if (entity[propertyName] != buttonText.text) {
            //     buttonText.text = entity[propertyName];
            // }

            buttonRect.sizeDelta = new(Mathf.Max(buttonTextRect.sizeDelta.x, minButtonSize), buttonRect.sizeDelta.y);
        }

        public void ResetValue()
        {
            buttonText.text = entity[propertyName] = button.defaultLabel;
        }

        public override void SetCollapses(object type)
        {
            UpdateCollapse(entity[propertyName]);
        }
    }
}