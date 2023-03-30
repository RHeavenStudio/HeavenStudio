using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using HeavenStudio;

using Starpelly;
using TMPro;

namespace HeavenStudio.Editor.Track
{
    public class NodeEventObj : MonoBehaviour, IPointerClickHandler
    {
        public Node nodeEntity;

        private RectTransform rectTransform;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void UpdateNode(Gradient keyGradient, RectTransform _timelineRect)
        {
            rectTransform.anchoredPosition = new Vector2(
                nodeEntity.Beat,
                Mathf.Lerp(-_timelineRect.rect.height, 0.0f, nodeEntity.Intensity / 100.0f)
                );

            var keyframeY = rectTransform.anchoredPosition.y;

            var normalized = Mathp.Normalize(keyframeY, -_timelineRect.rect.height, 0);
            transform.GetChild(0).GetChild(0).GetComponent<Image>().color = keyGradient.Evaluate(0);
            transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = (nodeEntity.Intensity).ToString();
        }

        public void OnPointerClick(PointerEventData eventData)
        {

        }
    }
}