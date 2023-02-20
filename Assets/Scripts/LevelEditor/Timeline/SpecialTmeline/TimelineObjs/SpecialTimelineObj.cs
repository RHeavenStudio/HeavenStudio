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
        private float lastPosX;

        public bool moving = false;
        public bool hovering;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        protected void Update()
        {
            if (!Conductor.instance.NotStopped())
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(raycastRect, Input.mousePosition, Editor.instance.EditorCamera))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        OnLeftClick();
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        OnRightClick();
                    }
                    hovering = true;
                }
                else
                {
                    hovering = false;
                }

                if (moving)
                {
                    Vector3 mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);

                    transform.position = new Vector3(mousePos.x - startPosX, transform.position.y, 0);
                    transform.localPosition = new Vector3(Mathf.Clamp(Starpelly.Mathp.Round2Nearest(transform.localPosition.x, Timeline.SnapInterval()), 0, Mathf.Infinity), transform.localPosition.y);

                    if (Input.GetMouseButtonUp(0))
                    {
                        if (!OnMove(transform.localPosition.x))
                            transform.localPosition = new Vector3(lastPosX, transform.localPosition.y);

                        moving = false;
                        lastPosX = transform.localPosition.x;
                    }
                }
            }
            else
            {
                if (moving)
                {
                    if (!OnMove(transform.localPosition.x))
                            transform.localPosition = new Vector3(lastPosX, transform.localPosition.y);
                    moving = false;
                    lastPosX = transform.localPosition.x;
                }
                hovering = false;
            }
        }

        public void StartMove()
        {
            Vector3 mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);
            startPosX = mousePos.x - transform.position.x;
            moving = true;
            lastPosX = transform.localPosition.x;
        }

        public void DeleteObj()
        {
            transform.parent.GetComponent<SpecialTimeline>().specialTimelineObjs.Remove(this);
            Destroy(this.gameObject);
        }

        //events
        public virtual void Init() {}
        public virtual void OnLeftClick() {}
        public virtual void OnRightClick() {}
        public virtual bool OnMove(float beat)
        {
            return true;
        }
        public virtual void SetVisibility(Timeline.CurrentTimelineState.State state) {}
    }
}