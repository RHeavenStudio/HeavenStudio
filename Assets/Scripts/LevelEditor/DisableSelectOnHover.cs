using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavenStudio.Editor
{
    public class DisableSelectOnHover : MonoBehaviour
    {
        public EventParameterManager eventParameterManager;

        private void LateUpdate()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (Editor.MouseInRectTransform(transform.GetChild(i).GetComponent<RectTransform>()))
                {
                    if (eventParameterManager != null)
                        eventParameterManager.canDisable = false;
                }
            }
        }
    }
}