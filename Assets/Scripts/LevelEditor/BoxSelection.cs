using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Starpelly;

using HeavenStudio.Editor.Track;

namespace HeavenStudio.Editor
{
    public class BoxSelection : MonoBehaviour
    {
        [SerializeField] private RectTransform boxVisual;
        [SerializeField] private RectTransform timelineContent;
        private Rect selectionBox;

        private Vector2 startPosition = Vector2.zero;
        private Vector2 endPosition = Vector2.zero;

        public bool selecting = false;

        /// <summary>
        /// Are we currently drag selecting?
        /// </summary>
        public bool ActivelySelecting = false;

        private bool clickedInTimeline = false;

        private TMPro.TMP_Text sizeText;
        private RectTransform text;

        private float timelineLastX;

        public static BoxSelection instance { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            DrawVisual();

            Color boxCol = EditorTheme.theme.properties.BoxSelectionCol.Hex2RGB();
            boxVisual.GetComponent<Image>().color = new Color(boxCol.r, boxCol.g, boxCol.b, 0.3f);
            boxVisual.transform.GetChild(0).GetComponent<Image>().color = EditorTheme.theme.properties.BoxSelectionOutlineCol.Hex2RGB();

            sizeText = boxVisual.transform.GetChild(1).GetComponent<TMPro.TMP_Text>();
            text = boxVisual.transform.GetChild(1).GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (Editor.instance == null) return;
            float deltaTimelineX = timelineContent.transform.localPosition.x - timelineLastX;

            Camera camera = Editor.instance.EditorCamera;
            Vector3 scale = Editor.instance.MainCanvas.transform.localScale;

            boxVisual.transform.localScale = new Vector2((1f / Timeline.instance.TimelineContent.localScale.x) / scale.x, 1f / scale.y);
            text.transform.localScale = scale;

            if (Selections.instance.eventsSelected.Count > 0 && Timeline.instance.InteractingWithEvents())
            {
                startPosition = Vector2.zero;
                endPosition = Vector2.zero;
                DrawVisual();
                return;
            }

            if (Conductor.instance.NotStopped() || !Timeline.instance.timelineState.selected)
            {
                startPosition = Vector2.zero;
                endPosition = Vector2.zero;
                DrawVisual();
                return;
            }

            float beatLen = boxVisual.rect.width * boxVisual.transform.localScale.x;
            if (beatLen >= 0.5f)
                sizeText.text = $"{string.Format("{0:0.000}", beatLen)}";
            else
                sizeText.text = string.Empty;

            // click
            if (Input.GetMouseButtonDown(0))
            {
                clickedInTimeline = Timeline.instance.CheckIfMouseInTimeline();

                startPosition = MousePosition();
                selectionBox = new Rect();

                ActivelySelecting = true;
            }

            // dragging
            if (Input.GetMouseButton(0) && clickedInTimeline)
            {
                startPosition.x += deltaTimelineX * scale.x;
                endPosition = MousePosition();
                DrawSelection();
                SelectEvents(); //kek
                DrawVisual();
            }

            // release click
            if (Input.GetMouseButtonUp(0))
            {
                startPosition = Vector2.zero;
                endPosition = Vector2.zero;
                SelectEvents();
                DrawVisual();

                ActivelySelecting = false;
            }

            timelineLastX = timelineContent.transform.localPosition.x;
        }

        private void DrawVisual()
        {
            Vector2 boxStart = startPosition;
            Vector2 boxEnd = endPosition;

            Vector2 boxCenter = (boxStart + boxEnd) / 2;
            boxVisual.position = boxCenter;

            Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));
            boxVisual.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boxSize.x);
            boxVisual.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, boxSize.y);
        }

        private void DrawSelection()
        {
            // X
            if (MousePosition().x < startPosition.x)
            {
                // dragging left
                selectionBox.xMin = MousePosition().x;
                selectionBox.xMax = startPosition.x;
            }
            else
            {
                // dragging right
                selectionBox.xMin = startPosition.x;
                selectionBox.xMax = MousePosition().x;
            }

            // Y
            if (MousePosition().y < startPosition.y)
            {
                // dragging down
                selectionBox.yMin = MousePosition().y;
                selectionBox.yMax = startPosition.y;
            }
            else
            {
                // dragging up
                selectionBox.yMin = startPosition.y;
                selectionBox.yMax = MousePosition().y;
            }
        }

        private void SelectEvents()
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Timeline.instance.InteractingWithEvents() && Editor.instance.canSelect) Selections.instance.DeselectAll();

            int selected = 0;

            for (int i = 0; i < Timeline.instance.eventObjs.Count; i++)
            {
                TimelineEventObj e = Timeline.instance.eventObjs[i];

                if (selectionBox.Overlaps(GetWorldRect(e.GetComponent<RectTransform>())))
                {
                    Selections.instance.DragSelect(e);
                    selected++;
                }
            }

            selecting = selected > 0;
        }

        public Vector3 MousePosition()
        {
            var mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);
            return new Vector3(mousePos.x, mousePos.y, 0);
        }

        public Rect GetWorldRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            // Get the bottom left corner.
            Vector3 position = corners[0];

            Vector2 size = new Vector2(
                rectTransform.lossyScale.x * rectTransform.rect.size.x,
                rectTransform.lossyScale.y * rectTransform.rect.size.y);

            return new Rect(position, size);
        }
    }
}