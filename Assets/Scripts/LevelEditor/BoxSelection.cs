using UnityEngine;
using UnityEngine.UI;

using HeavenStudio.Editor.Track;

using DG.Tweening;
using Starpelly;

namespace HeavenStudio.Editor
{
    public class BoxSelection : MonoBehaviour
    {
        /// <summary>
        /// Are we currently drag selecting?
        /// </summary>
        public bool ActivelySelecting { get; private set; } = false;

        private Vector2 startPosition = Vector2.zero;
        private Vector2 endPosition = Vector2.zero;
        private bool validClick = false;

        [SerializeField] private RectTransform boxVisual;
        private CanvasGroup boxGroup;
        private TMPro.TMP_Text sizeText;

        public static BoxSelection instance { get; private set; }

        private void Start()
        {
            instance = this;

            Color boxCol = EditorTheme.theme.properties.BoxSelectionCol.Hex2RGB();
            boxVisual.GetChild(0).GetComponent<Image>().color = new Color(boxCol.r, boxCol.g, boxCol.b, 0.3f);
            boxVisual.GetChild(0).GetChild(0).GetComponent<Image>().color = EditorTheme.theme.properties.BoxSelectionOutlineCol.Hex2RGB();

            sizeText = boxVisual.GetChild(0).GetChild(1).GetComponent<TMPro.TMP_Text>();
            sizeText.text = string.Empty;

            boxGroup = boxVisual.GetComponent<CanvasGroup>();
        }

        public void LayerSelectUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!Timeline.instance.MouseInTimeline || TimelineBlockManager.Instance.InteractingWithEvents || Conductor.instance.NotStopped())
                {
                    return;
                }

                validClick = true;
                startPosition = new Vector2(Timeline.instance.MousePos2Beat, Timeline.instance.MousePos2Layer);

                boxGroup.DOKill();
                boxGroup.alpha = 1.0f;
            }

            if (!validClick) return;

            var startPos = startPosition;
            var endPos = endPosition;

            if (Input.GetMouseButton(0))
            {
                endPos = new Vector2(Timeline.instance.MousePos2Beat,
                    Timeline.instance.MousePos2Layer +
                    (Timeline.instance.MousePos2Layer < startPosition.y ? 0 : 1));
                startPos = new Vector2(startPos.x,
                    startPos.y + ((Timeline.instance.MousePos2Layer < startPosition.y) ? 1 : 0));

                startPos = new Vector2(startPos.x, Mathf.Clamp(startPos.y, 0, Timeline.instance.LayerCount));
                endPos = new Vector2(endPos.x, Mathf.Clamp(endPos.y, 0, Timeline.instance.LayerCount));

                ActivelySelecting = true;

                if (Conductor.instance.NotStopped())
                {
                    validClick = false;
                    boxGroup.DOFade(0.0f, 0.3f).SetEase(Ease.OutExpo);

                    ActivelySelecting = false;
                    return;
                }
            }

            var start = new Vector2(Mathf.Min(startPos.x, endPos.x),
                Mathf.Min(startPos.y, endPos.y));
            var end = new Vector2(Mathf.Max(startPos.x, endPos.x),
                Mathf.Max(startPos.y, endPos.y));

            if (Input.GetMouseButtonUp(0))
            {
                validClick = false;
                boxGroup.DOFade(0.0f, 0.3f).SetEase(Ease.OutExpo);

                ActivelySelecting = false;
                return;
            }

            boxVisual.anchoredPosition = new Vector2(start.x * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerToY(Mathf.FloorToInt(start.y)));
            boxVisual.sizeDelta = new Vector2((end.x - start.x) * Timeline.instance.PixelsPerBeat,
                (end.y - start.y) * Timeline.instance.LayerHeight());

            var boxLength = end.x - start.x;
            if (boxLength > 0.01f)
                sizeText.text = (boxLength).ToString("F");
            else
                sizeText.text = string.Empty;

            // Keeps the text always in view
            var sizeTextLeft = Timeline.instance.leftSide - start.x;
            sizeTextLeft = Mathf.Max(sizeTextLeft, 0);
            var sizeTextRight = -(Timeline.instance.rightSide - end.x);
            sizeTextRight = Mathf.Max(sizeTextRight, 0);

            var sizeTextTop = Timeline.instance.topSide - start.y;
            sizeTextTop = Mathf.Max(sizeTextTop, 0);
            var sizeTextBottom = -(Timeline.instance.bottomSide - end.y);
            sizeTextBottom = Mathf.Max(sizeTextBottom, 0);

            sizeText.rectTransform.offsetMin = new Vector2(sizeTextLeft * Timeline.instance.PixelsPerBeat, -sizeTextTop * Timeline.instance.LayerHeight());
            sizeText.rectTransform.offsetMax = new Vector2(-sizeTextRight * Timeline.instance.PixelsPerBeat, sizeTextBottom * Timeline.instance.LayerHeight());

            Select(start, end);
        }

        private void Select(Vector2 start, Vector2 end)
        {
            var boxRect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);

            // This doesn't take into account blocks the user cannot see, this is intentional.
            foreach (var marker in TimelineBlockManager.Instance.EntityMarkers.Values)
            {
                var markerRect = new Rect((float)marker.entity.beat, (int)marker.entity["track"], marker.entity.length, 1);

                var boxOverMarker = boxRect.Overlaps(markerRect);
                if (boxOverMarker)
                {
                    if (!marker.selected)
                        Selections.instance.DragSelect(marker);
                }
                else
                {
                    if (marker.selected && !Input.GetKey(KeyCode.LeftShift))
                        Selections.instance.Deselect(marker);
                }
            }
        }
    }
}