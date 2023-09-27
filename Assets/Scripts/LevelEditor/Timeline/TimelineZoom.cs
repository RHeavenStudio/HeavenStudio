using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavenStudio.Editor.Track
{
    public class TimelineZoom : MonoBehaviour, IScrollHandler
    {
        public Vector3 Scale => _scale;

        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale => float.MaxValue;
        [SerializeField] private Vector2 initialScale = Vector2.one;

        private Vector3 _scale;
        private Vector2 relMousePos;

        private RectTransform rectTransform;

        private void Awake()
        {
            initialScale = transform.localScale;
            rectTransform = transform as RectTransform;

            _scale.Set(initialScale.x, initialScale.y, 1);
            rectTransform.localScale = _scale;
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (Conductor.instance.NotStopped()) return;

            relMousePos = rectTransform.anchoredPosition;

            Vector2 relativeMousePosition;

            var cam = Editor.instance.EditorCamera;
            if (cam == null) Debug.LogError("Camera not set!");
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, cam, out relativeMousePosition);

            float delta = eventData.scrollDelta.y;

            if (delta > 0)
            {
                Zoom(0.25f * _scale.x, relativeMousePosition);
            }
            else if (delta < 0)
            {
                var incre = -0.2f * _scale.x;
                if (_scale.x + incre > minScale - 0.1f)
                    Zoom(-0.2f * _scale.x, relativeMousePosition);
            }
        }

        public void Zoom(float incre, Vector2 relativeMousePosition)
        {
            if (!(_scale.x < maxScale)) return;

            var newScale = Mathf.Clamp(_scale.x + incre, minScale, maxScale);
            _scale.Set(newScale, 1, 1);
            relativeMousePosition = new Vector2(relativeMousePosition.x, 0);
            relMousePos -= (relativeMousePosition * incre);

            rectTransform.localScale = _scale;
            rectTransform.anchoredPosition = relMousePos;

            rectTransform.localScale =
                new Vector3(Mathf.Clamp(rectTransform.localScale.x, minScale, Mathf.Infinity),
                    rectTransform.localScale.y);

            Timeline.instance.OnZoom(newScale);
        }

        public void SetNewPos(float newX)
        {
            relMousePos = new Vector2(newX, relMousePos.y);
        }

        public void ResetZoom()
        {
            _scale.Set(1, 1, 1);
            rectTransform.localScale = _scale;
            Timeline.instance.OnZoom(_scale.x);
        }
    }
}