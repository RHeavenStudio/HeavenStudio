using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;


namespace HeavenStudio.Editor.Track
{
    public class LayerLabel : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;

        // Update is called once per frame
        void Update()
        {
            //convert timeline layer scale to screen space
            Camera cam;
            //"your program can't crash if you put everything in a try block"
            try
            {
                cam = Editor.instance.EditorCamera;
                float layerScaleDist = cam.WorldToScreenPoint(Timeline.instance.LayerCorners[1]).y - Camera.main.WorldToScreenPoint(Timeline.instance.LayerCorners[0]).y;
                float modScale = Timeline.GetScaleModifier();
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layerScaleDist/5 * (1/modScale));
            }
            catch (System.NullReferenceException)
            {
                return;
            }
        }

    }
}