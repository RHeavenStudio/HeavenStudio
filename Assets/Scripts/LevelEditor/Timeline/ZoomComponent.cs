using Starpelly;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeavenStudio.Editor.Track
{
    /// <summary>
    /// Zoom component which will handle the scroll wheel events and zooms in on the pointer
    /// </summary>
    public class ZoomComponent : MonoBehaviour, IScrollHandler
    {

        //Make sure these values are evenly divisible by scaleIncrement
        [SerializeField] float _minimumScale = 0.5f;
        [SerializeField] Vector2 _initialScale = Vector2.one;
        [SerializeField] float _maximumScale = 3f;
        /////////////////////////////////////////////
        [SerializeField] float _scaleIncrement = .5f;
        /////////////////////////////////////////////

        [HideInInspector] Vector3 _scale;

        RectTransform _thisTransform;

        private void Awake()
        {
            _initialScale = transform.localScale;
            _thisTransform = transform as RectTransform;

            _scale.Set(_initialScale.x, _initialScale.y, 1);
            _thisTransform.localScale = _scale;

        }

        private void Update()
        {
            // KeepScale();
        }

        private void KeepScale()
        {
            _thisTransform.localScale = new Vector3(Mathf.Clamp(_thisTransform.localScale.x, _minimumScale, _maximumScale), _thisTransform.localScale.y, 1);
        }

        public void OnScroll(PointerEventData eventData)
        {
            Vector2 relativeMousePosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_thisTransform, Input.mousePosition, Editor.instance.EditorCamera, out relativeMousePosition);

            float delta = eventData.scrollDelta.y;
            delta = Mathf.Clamp(delta, -6f, 6f);

            if (delta > 0 && _scale.x < _maximumScale)
            {   //zoom in
                float incre = _scaleIncrement * delta;

                _scale.Set(_scale.x + incre, 1, 1f);
                _thisTransform.localScale = _scale;
                relativeMousePosition = new Vector2(relativeMousePosition.x, 0);
                _thisTransform.anchoredPosition -= (relativeMousePosition * incre);
            }

            else if (delta < 0 && _scale.x > _minimumScale)
            {   //zoom out
                float incre = _scaleIncrement * -delta;

                _scale.Set(_scale.x - incre, 1, 1f);
                _thisTransform.localScale = _scale;
                relativeMousePosition = new Vector2(relativeMousePosition.x, 0);
                _thisTransform.anchoredPosition += (relativeMousePosition * incre);
            }

            // Timeline.Instance.UpdateScale(_thisTransform.localScale.x);

            // _thisTransform.localScale = new Vector3(Mathf.Clamp(_thisTransform.localScale.x, _minimumScale, Mathf.Infinity), _thisTransform.localScale.y, 1);
            // _thisTransform.anchoredPosition = new Vector2(Mathf.Clamp(_thisTransform.anchoredPosition.x, -_thisTransform.sizeDelta.x * _thisTransform.localScale.x, 0), _thisTransform.anchoredPosition.y);
        }
    }
}