using UnityEngine;
using UnityEngine.EventSystems;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace HeavenStudio.Editor.Track
{
    public class ZoomComponent : MonoBehaviour, IScrollHandler
    {
        [SerializeField] float _minimumScale = 0.5f;
        [SerializeField] Vector2 _initialScale = Vector2.one;
        [SerializeField] float _maximumScale = 3f;
        [SerializeField] float _scaleIncrement = .5f;

        [HideInInspector] Vector3 _scale;

        RectTransform _thisTransform;

        private void Awake()
        {
            _initialScale = transform.localScale;
            _thisTransform = transform as RectTransform;

            _scale.Set(_initialScale.x, _initialScale.y, 1);
            _thisTransform.localScale = _scale;
        }

        public void OnScroll(PointerEventData eventData)
        {
            Vector2 relativeMousePosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_thisTransform, Input.mousePosition, Editor.instance.EditorCamera, out relativeMousePosition);

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
        }

        public void ZoomIn(float delta, Vector2 relativeMousePosition)
        {
            if (!(_scale.x < _maximumScale)) return;
            if (Timeline.instance.InteractingWithEvents() || SpecialTimeline.instance.InteractingWithEvents()) return;

            float incre = _scaleIncrement * delta;

            var newScale = Mathf.Clamp(_scale.x + incre, _minimumScale, _maximumScale);
            _scale.Set(newScale, 1, 1);
            _thisTransform.localScale = _scale;
            relativeMousePosition = new Vector2(relativeMousePosition.x, 0);
            _thisTransform.anchoredPosition -= (relativeMousePosition * incre);
        }

        public void ZoomOut(float delta, Vector2 relativeMousePosition)
        {
            if (!(_scale.x > _minimumScale)) return;
            if (Timeline.instance.InteractingWithEvents() || SpecialTimeline.instance.InteractingWithEvents()) return;

            float incre = _scaleIncrement * -delta;

            var newScale = _scale.x - incre;
            _scale.Set(newScale, 1, 1);
            _thisTransform.localScale = _scale;
            relativeMousePosition = new Vector2(relativeMousePosition.x, 0);
            _thisTransform.anchoredPosition += (relativeMousePosition * incre);
        }

        public void ResetZoom()
        {
            _scale.Set(100, 1, 1);
            _thisTransform.localScale = _scale;
        }
    }
}