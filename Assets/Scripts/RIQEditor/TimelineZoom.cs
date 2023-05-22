using System;
using Starpelly;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavenStudio.RIQEditor
{
    public class TimelineZoom : MonoBehaviour, IScrollHandler, IPointerDownHandler
    {
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 3f;
        [SerializeField] private float _scaleIncrement = .5f;
        [SerializeField] private Vector2 _initialScale = Vector2.one;

        private Vector3 _scale;
        private Vector2 relMousePos;
        
        RectTransform rectTransform;

        private bool zooming;

        private void Awake()
        {
            _initialScale = transform.localScale;
            rectTransform = transform as RectTransform;

            _scale.Set(_initialScale.x, _initialScale.y, 1);
            rectTransform.localScale = _scale;
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (!zooming)
                relMousePos = rectTransform.anchoredPosition;
            
            Vector2 relativeMousePosition;

            var cam = EditorMain.Instance.EditorCamera;
            if (cam == null) Debug.LogError("Camera not set!");
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, EditorMain.Instance.EditorCamera, out relativeMousePosition);

            float delta = eventData.scrollDelta.y;
            delta = Mathf.Clamp(delta, -1f, 1f);

            _scaleIncrement = 0.1f * _scale.x;

            if (delta > 0)
            {
                ZoomIn(delta, relativeMousePosition);
            }
            else if (delta < 0)
            {
                ZoomOut(delta, relativeMousePosition);
            }
            zooming = true;
        }

        public void ZoomIn(float delta, Vector2 relativeMousePosition)
        {
            if (!(_scale.x < maxScale)) return;

            float incre = _scaleIncrement * delta;

            var newScale = Mathf.Clamp(_scale.x + incre, minScale, maxScale);
            _scale.Set(newScale, 1, 1);
            relativeMousePosition = relativeMousePosition.ModifyY(0);
            relMousePos -= (relativeMousePosition * incre);
            
            EditorMain.Instance.Timeline.OnZoom(newScale);
        }

        public void ZoomOut(float delta, Vector2 relativeMousePosition)
        {
            if (!(_scale.x > minScale)) return;

            float incre = _scaleIncrement * -delta;

            var newScale = _scale.x - incre;
            _scale.Set(newScale, 1, 1);
            relativeMousePosition = relativeMousePosition.ModifyY(0);
            relMousePos += (relativeMousePosition * incre);
            
            EditorMain.Instance.Timeline.OnZoom(newScale);
        }

        private void Update()
        {
            if (zooming)
            {
                rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, _scale, Time.deltaTime * 20.0f);
                rectTransform.anchoredPosition = Vector3.Lerp(rectTransform.anchoredPosition, relMousePos, Time.deltaTime * 20.0f);
            }
            
            rectTransform.localScale =
                new Vector3(Mathf.Clamp(rectTransform.localScale.x, minScale, Mathf.Infinity),
                    rectTransform.localScale.y);
        }

        public void SetNewPos(float newX)
        {
            relMousePos = relMousePos.ModifyX(newX);
        }

        public void ResetZoom()
        {
            _scale.Set(100, 1, 1);
            rectTransform.localScale = _scale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            zooming = false;
        }
    }
}