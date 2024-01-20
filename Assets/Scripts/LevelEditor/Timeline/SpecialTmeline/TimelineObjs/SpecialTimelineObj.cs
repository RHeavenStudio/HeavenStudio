using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using DG.Tweening;
using Jukebox;

namespace HeavenStudio.Editor.Track
{
    public class SpecialTimelineObj : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform raycastRect;

        private float startPosX;
        private float lastPosX;

        public SpecialTimeline.HoveringTypes type;
        public bool moving = false;
        public bool hovering;
        public bool first = false;

        public RiqEntity chartEntity;

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
                    OnMove(Timeline.instance.MousePos2BeatSnap);

                    if (Input.GetMouseButtonUp(0))
                    {
                        /*if (!OnMove(transform.localPosition.x))
                            transform.localPosition = new Vector3(lastPosX, transform.localPosition.y);*/

                        moving = false;
                        OnMove(Timeline.instance.MousePos2BeatSnap, true);
                        lastPosX = transform.localPosition.x;
                    }
                }
            }
            else
            {
                /*
                if (moving)
                {
                    if (!OnMove(transform.localPosition.x))
                            transform.localPosition = new Vector3(lastPosX, transform.localPosition.y);
                    moving = false;
                    lastPosX = transform.localPosition.x;
                }
                */
                hovering = false;
            }
        }

        public void SetX(RiqEntity entity)
        {
            rectTransform.anchoredPosition = new Vector2((float)entity.beat * Timeline.instance.PixelsPerBeat, rectTransform.anchoredPosition.y);
        }

        public void SetX(float beat)
        {
            rectTransform.anchoredPosition = new Vector2(beat * Timeline.instance.PixelsPerBeat, rectTransform.anchoredPosition.y);
        }

        public void StartMove()
        {
            if (first) return;
            Vector3 mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);
            startPosX = mousePos.x - transform.position.x;
            moving = true;
            lastPosX = transform.localPosition.x;
        }

        public void DeleteObj()
        {
            if (first) return;
            CommandManager.Instance.AddCommand(new Commands.DeleteMarker(chartEntity.guid, type));
        }

        //events
        public virtual void Init() { }
        public virtual void OnLeftClick() { }
        public virtual void OnRightClick() { }
        public virtual bool OnMove(float beat, bool final = false)
        {
            return true;
        }
        public virtual void SetVisibility(Timeline.CurrentTimelineState.State state) { }
    }
}