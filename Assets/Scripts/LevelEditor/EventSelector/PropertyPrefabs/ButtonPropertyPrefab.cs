using System;
using UnityEngine;
using TMPro;

namespace HeavenStudio.Editor
{
    public class ButtonPropertyPrefab : EventPropertyPrefab
    {
        [Header("Boolean")]
        [Space(10)]
        // public Button buttonObj;
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
        }

        // OnClick() already handles this. 
        // if somebody wants to uncomment this for their thing feel free but it's unused for now -AJ
        // private void LateUpdate()
        // {
        //     if (entity[propertyName] != buttonText.text) {
        //         buttonText.text = entity[propertyName];
        //     }
        // }
    }
}