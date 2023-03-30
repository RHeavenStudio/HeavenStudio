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
        private Graphic icon;
        private TMP_Text text;

        public void OnCreate()
        {
            rectTransform = GetComponent<RectTransform>();
            icon = GetComponentInChildren<Graphic>();
            text = GetComponentInChildren<TMP_Text>();
        }

        public void UpdateNode(Color color, Vector2 position)
        {
            nodeEntity.Intensity = Mathf.RoundToInt(nodeEntity.Intensity);
            /*if (nodeEntity.Type == NodeType.Pixelize)
                nodeEntity.Intensity = Mathp.Round2Nearest(nodeEntity.Intensity, )*/

            rectTransform.anchoredPosition = position;

            color.a = 1;
            icon.color = color;
            text.text = nodeEntity.Intensity.ToString();


            // var normalized = Mathp.Normalize(keyframeY, -_timelineRect.rect.height, 0);
            // transform.GetChild(0).GetChild(0).GetComponent<Image>().color = keyGradient.Evaluate(0);
            // transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = (nodeEntity.Intensity).ToString();
        }

        public void OnPointerClick(PointerEventData eventData)
        {

        }
    }
}