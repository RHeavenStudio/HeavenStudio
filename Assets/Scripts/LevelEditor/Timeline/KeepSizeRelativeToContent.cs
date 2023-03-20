using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.Editor.Track
{
    public class KeepSizeRelativeToContent : MonoBehaviour
    {
        private RectTransform rectTransform;

        public float sizeDeltaXInternal;

        public float sizeDeltaX = 1.0f;
        public float multiply;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            SetScale(Timeline.instance.TimelineContent.localScale.x);
        }

        private void LateUpdate()
        {
            SetScale(Timeline.instance.TimelineContent.localScale.x);
        }

        private void SetScale(float scale)
        {
            rectTransform.localScale = new Vector3((1.0f / scale) * multiply, transform.localScale.y, 1);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}
