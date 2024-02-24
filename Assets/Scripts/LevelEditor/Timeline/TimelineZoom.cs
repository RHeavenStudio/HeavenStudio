using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavenStudio.Editor.Track
{
    public class TimelineZoom : MonoBehaviour
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

        private void Update()
        {
            var scrollDeltaY = Input.mouseScrollDelta.y;
            if (scrollDeltaY != 0)
            {
                if (Timeline.instance.MouseInTimeline)
                    OnScroll(scrollDeltaY);
            }
        }

        public void OnScroll(float scrollDeltaY)
        {
            if (Conductor.instance.NotStopped()) return;

            relMousePos = rectTransform.anchoredPosition;

            Vector2 relativeMousePosition;

            var cam = Editor.instance.EditorCamera;
            if (cam == null)
            {
                Debug.LogError("Camera not set!");
                return;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, cam, out relativeMousePosition);

            if (scrollDeltaY > 0)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    Zoom(0.25f * _scale.y, relativeMousePosition, false);
                }
                else
                {
                    Zoom(0.25f * _scale.x, relativeMousePosition, true);
                }
            }
            else if (scrollDeltaY < 0)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    var incre = -0.2f * _scale.y;
                    if (_scale.y + incre > minScale - 0.1f)
                        Zoom(-0.2f * _scale.y, relativeMousePosition, false);
                }
                else
                {
                    var incre = -0.2f * _scale.x;
                    if (_scale.x + incre > minScale - 0.1f)
                        Zoom(-0.2f * _scale.x, relativeMousePosition, true);
                }
            }
        }

        public void Zoom(float incre, Vector2 relativeMousePosition, bool horiz)
        {
            if (!(_scale.x < maxScale)) return;

            if (horiz)
            {
                var newScale = Mathf.Clamp(_scale.x + incre, minScale, maxScale);
                _scale.Set(newScale, _scale.y, 1);
                relativeMousePosition = new Vector2(relativeMousePosition.x, 0);
                relMousePos -= (relativeMousePosition * incre);

                rectTransform.localScale = _scale;
                rectTransform.anchoredPosition = relMousePos;

                rectTransform.localScale =
                    new Vector3(Mathf.Clamp(rectTransform.localScale.x, minScale, Mathf.Infinity),
                        rectTransform.localScale.y);

                Timeline.instance.OnZoom(newScale);
            }
            else
            {
                var newScale = Mathf.Clamp(_scale.y + incre, 1.0f, maxScale);
                _scale.Set(_scale.x, newScale, 1);
                relativeMousePosition = new Vector2(0, relativeMousePosition.y);
                relMousePos -= (relativeMousePosition * incre);

                rectTransform.localScale = _scale;
                rectTransform.anchoredPosition = relMousePos;

                rectTransform.localScale =
                    new Vector3(rectTransform.localScale.x, 
                    Mathf.Clamp(rectTransform.localScale.y, 1.0f, Mathf.Infinity));

                Timeline.instance.OnZoomVertical(newScale);
            }
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