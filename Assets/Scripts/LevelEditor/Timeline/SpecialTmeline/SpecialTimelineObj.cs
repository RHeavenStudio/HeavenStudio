using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using DG.Tweening;

namespace HeavenStudio.Editor.Track
{
    public class SpecialTimelineObj : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform raycastRect;

        private float startPosX;
        private bool moving = false;

        public bool hovering;
        private float lastPosX;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!Conductor.instance.NotStopped())
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(raycastRect, Input.mousePosition, Editor.instance.EditorCamera))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Vector3 mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);
                        startPosX = mousePos.x - transform.position.x;
                        moving = true;
                        lastPosX = transform.localPosition.x;
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        // transform.parent.GetComponent<TempoTimeline>().tempoTimelineObjs.Remove(this);
                        Destroy(this.gameObject);
                    }
                }
            }
        }
    }
}