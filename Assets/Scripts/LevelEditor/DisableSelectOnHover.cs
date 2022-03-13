using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavenStudio.Editor
{
    public class DisableSelectOnHover : MonoBehaviour
    {
        public EventParameterManager eventParameterManager;

        // this is programmed on duct tape https://youtu.be/zMWA0ipQ94w?t=868
        private void LateUpdate()
        {
            eventParameterManager.canDisable = true;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (Editor.MouseInRectTransform(transform.GetChild(i).GetComponent<RectTransform>()))
                {
                    eventParameterManager.canDisable = false;
                }
            }
        }
    }
}