using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RhythmHeavenMania.Editor
{
    public class DisableSelectOnHover : MonoBehaviour
    {
        public EventParameterManager eventParameterManager;

        // this is fucking r#####ded
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