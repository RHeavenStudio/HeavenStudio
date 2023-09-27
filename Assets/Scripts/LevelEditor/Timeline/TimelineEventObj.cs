using UnityEngine;
using UnityEngine.UI;

using Starpelly;
using Jukebox;
using TMPro;
using UnityEngine.Timeline;
using System.Linq;
using System.Collections.Generic;

namespace HeavenStudio.Editor.Track
{
    public class TimelineEventObj : MonoBehaviour
    {
        public bool interacting;

        private Vector3 lastPos;
        public Vector2 moveStartPos;
        private RectTransform rectTransform;

        [Header("Components")]
        [SerializeField] private RectTransform PosPreview;
        [SerializeField] private RectTransform PosPreviewRef;
        [SerializeField] public Image Icon;
        [SerializeField] private Image selectedImage;
        [SerializeField] public TMP_Text eventLabel;

        [SerializeField] private Image resizeGraphic;

        [SerializeField] private RectTransform leftDrag;
        [SerializeField] private RectTransform rightDrag;

        [SerializeField] private Image outline;
        [SerializeField] private Image hasPropertiesIcon;

        [SerializeField] private RectTransform visibleRegion;

        // private GameObject moveTemp;

        [Header("Properties")]
        public RiqEntity entity;
        public float length;
        private bool lastVisible;
        public bool selected;
        public bool mouseHovering;
        public bool resizable;
        public bool resizing;
        public bool moving;
        public bool wasDuplicated;
        private bool resizingLeft;
        private bool resizingRight;
        private bool inResizeRegion;
        public bool isCreating;

        private bool altWhenClicked = false;

        private float initMoveX = 0.0f;
        private float initMoveY = 0.0f;

        private bool movedEntity = false;
        private float lastBeat = 0.0f;
        private int lastLayer = 0;

        private int lastSiblingIndex;
        private bool changedSiblingIndex;

        public float zPriority { get; private set; }

        // Difference between mouseHovering is this is regardless if the user can see it.
        private bool mouseOver;

        private float clickTimer = 0.0f;

        [Header("Colors")]
        public Color NormalCol;

        public void SetMarkerInfo()
        {
            moveStartPos = transform.localPosition;
            rectTransform = GetComponent<RectTransform>();

            var eventName = entity.datamodel;

            var game = EventCaller.instance.GetMinigame(eventName.Split(0));
            var action = EventCaller.instance.GetGameAction(game, eventName.Split(1));
            var gameAction = EventCaller.instance.GetGameAction(EventCaller.instance.GetMinigame(eventName.Split(0)), eventName.Split(1));

            if (eventName.Split(1) == "switchGame")
                Icon.sprite = Editor.GameIcon(eventName.Split(2));
            else
                Icon.sprite = Editor.GameIcon(eventName.Split(0));

            if (gameAction != null)
            {
                if (gameAction.resizable == false)
                {
                    rectTransform.sizeDelta = new Vector2(gameAction.defaultLength * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerHeight());
                    this.length = gameAction.defaultLength;
                }
                else
                {
                    this.resizable = true;
                    if (entity != null && gameAction.defaultLength != entity.length)
                    {
                        rectTransform.sizeDelta = new Vector2(entity.length * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerHeight());
                    }
                    else
                    {
                        rectTransform.sizeDelta = new Vector2(gameAction.defaultLength * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerHeight());
                    }
                }
            }

            rectTransform.anchoredPosition = new Vector2((float)entity.beat * Timeline.instance.PixelsPerBeat, (int)-entity["track"] * Timeline.instance.LayerHeight());
            resizeGraphic.gameObject.SetActive(resizable);
            eventLabel.text = action.displayName;

            hasPropertiesIcon.enabled = action.actionName != "switchGame" && action.parameters != null && action.parameters.Count > 0;

            SetColor((int)entity["track"]);
            SetWidthHeight();
            selectedImage.gameObject.SetActive(false);
        }

        public void SetEntity(RiqEntity entity)
        {
            this.entity = entity;
        }

        public void Update()
        {
            clickTimer += Time.deltaTime;
        }

        public void UpdateMarker()
        {
            mouseOver = Timeline.instance.timelineState.selected && Timeline.instance.MouseInTimeline &&
                Timeline.instance.MousePos2Beat.IsWithin((float)entity.beat, (float)entity.beat + entity.length) &&
                Timeline.instance.MousePos2Layer == (int)entity["track"];

            eventLabel.overflowMode = (mouseHovering || moving) ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;

            if (selected)
            {
                if (moving)
                    outline.color = Color.magenta;
                else
                    outline.color = Color.cyan;
            }
            else
            {
                outline.color = new Color32(0, 0, 0, 51);
            }

            if (Conductor.instance.NotStopped())
            {
                if (moving)
                {
                    moving = false;
                }

                if (selected)
                {
                    selected = false;
                    selectedImage.gameObject.SetActive(false);
                    outline.color = new Color32(0, 0, 0, 51);
                }
                return;
            }

            if (resizing)
            {
                if (moving) moving = false;
                if (resizingLeft) SetPivot(new Vector2(1, rectTransform.pivot.y));

                Vector2 sizeDelta = rectTransform.sizeDelta;
                Vector2 mousePos;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, Editor.instance.EditorCamera, out mousePos);

                sizeDelta = new Vector2((resizingLeft ? -mousePos.x : mousePos.x) + 0.15f, sizeDelta.y);
                sizeDelta = new Vector2(Mathf.Clamp(sizeDelta.x, Timeline.SnapInterval(), resizingLeft ? rectTransform.localPosition.x : Mathf.Infinity), sizeDelta.y);

                rectTransform.sizeDelta = new Vector2(Mathp.Round2Nearest(sizeDelta.x, Timeline.SnapInterval()), sizeDelta.y);
                SetPivot(new Vector2(0, rectTransform.pivot.y));
            }
            else
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (moving)
                    {
                        moving = false;

                        if (!isCreating && movedEntity)
                        {
                            List<double> lastBeats = new();
                            List<int> lastLayers = new();
                            foreach (var marker in Selections.instance.eventsSelected)
                            {
                                var entity = marker.entity;

                                lastBeats.Add(marker.entity.beat);
                                lastLayers.Add((int)marker.entity["track"]);

                                entity.beat = marker.lastBeat;
                                entity["track"] = marker.lastLayer;
                            }
                            CommandManager.Instance.AddCommand(new Commands.Move(Selections.instance.eventsSelected.Select(c => c.entity).ToList(), lastBeats, lastLayers));
                        }

                        isCreating = false;

                        GameManager.instance.SortEventsList();
                        TimelineBlockManager.Instance.SortMarkers();
                    }
                }

                if (moving)
                {
                    foreach (var marker in Selections.instance.eventsSelected)
                    {
                        marker.entity.beat = Mathf.Max(Timeline.instance.MousePos2BeatSnap - marker.initMoveX, 0);
                        marker.entity["track"] = Mathf.Clamp(Timeline.instance.MousePos2Layer - marker.initMoveY, 0, Timeline.instance.LayerCount - 1);
                        marker.SetColor((int)entity["track"]);
                        marker.SetWidthHeight();
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                OnLeftUp();
                OnRightUp();
            }

            if (!BoxSelection.instance.ActivelySelecting)
            if (resizing && selected || inResizeRegion && selected)
            {
                if (resizable)
                    Cursor.SetCursor(Timeline.instance.resizeCursor, new Vector2(14, 14), CursorMode.Auto);
            }
            // should consider adding this someday
            // else if (moving && selected || mouseHovering && selected)
            // {
            //     Cursor.SetCursor(Resources.Load<Texture2D>("Cursors/move"), new Vector2(8, 8), CursorMode.Auto);
            // }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }

            rectTransform.anchoredPosition = new Vector2((float)entity.beat * Timeline.instance.PixelsPerBeat, -(int)entity["track"] * Timeline.instance.LayerHeight());

            zPriority = entity.length;

            if (selected)
                zPriority += 10000;
        }

        public void LateUpdate()
        {
            var followXL = (Timeline.instance.leftSide - (float)entity.beat) * Timeline.instance.PixelsPerBeat;
            visibleRegion.offsetMin = new Vector2(
                Mathf.Clamp(followXL - 2, 0, (entity.length * Timeline.instance.PixelsPerBeat) - Timeline.instance.LayerHeight()),
                visibleRegion.offsetMin.y);

            var followXR = (Timeline.instance.rightSide - ((float)entity.beat + entity.length)) * Timeline.instance.PixelsPerBeat;
            visibleRegion.offsetMax = new Vector2(
                Mathf.Clamp(followXR, -(entity.length * Timeline.instance.PixelsPerBeat) + 8, 0),
                visibleRegion.offsetMax.y);
        }

        public void BeginMoving(bool setMovedEntity = true)
        {
            moving = true;

            foreach (var marker in Selections.instance.eventsSelected)
            {
                if (setMovedEntity) marker.movedEntity = true;
                marker.lastBeat = (float)marker.entity.beat;
                marker.lastLayer = (int)marker.entity["track"];

                marker.initMoveX = Timeline.instance.MousePos2BeatSnap - (float)marker.entity.beat;
                marker.initMoveY = Timeline.instance.MousePos2Layer - (int)marker.entity["track"];
            }
        }

        #region ClickEvents

        public void HoverEnter()
        {
            if (!TimelineBlockManager.Instance.MovingAnyEvents)
            {
                lastSiblingIndex = gameObject.transform.GetSiblingIndex();
                gameObject.transform.SetAsLastSibling();
                changedSiblingIndex = true;
            }
            else
                changedSiblingIndex = false;

            mouseHovering = true;
            selectedImage.gameObject.SetActive(true);
        }

        public void HoverExit()
        {
            if (changedSiblingIndex)
                gameObject.transform.SetSiblingIndex(lastSiblingIndex);
            mouseHovering = false;
            selectedImage.gameObject.SetActive(false);
        }

        public void OnDragMain()
        {
            if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) return;
            if (!moving)
                altWhenClicked = Input.GetKey(KeyCode.LeftAlt);

            if (!altWhenClicked)
            {
                if (!selected)
                    Selections.instance.ClickSelect(this);
                if (!moving)
                    BeginMoving();

                return;
            }

            var entities = Selections.instance.eventsSelected;
            if (entities.Count == 0)
            {
                entities = new() { this };
            }

        }

        public void OnDown()
        {
            if (Input.GetMouseButton(0) && Timeline.instance.timelineState.selected)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Selections.instance.ShiftClickSelect(this);
                }
                else
                {
                    if (selected && clickTimer < 0.315f)
                    {
                        foreach (var marker in TimelineBlockManager.Instance.EntityMarkers.Values)
                        {
                            if (marker == this) continue;
                            if (marker.mouseOver)
                            {
                                Selections.instance.ClickSelect(marker);
                                marker.clickTimer = 0;
                                break;
                            }
                        }
                    }
                    else
                        Selections.instance.ClickSelect(this);
                }
            }
            else if (Input.GetMouseButton(1))
            {
                EventParameterManager.instance.StartParams(entity);
            }
            else if (Input.GetMouseButton(2))
            {
                var mgs = EventCaller.instance.minigames;
                string[] datamodels = entity.datamodel.Split('/');
                Debug.Log("Selected entity's datamodel : " + entity.datamodel);

                bool isSwitchGame = datamodels[1] == "switchGame";
                int gameIndex = mgs.FindIndex(c => c.name == datamodels[isSwitchGame ? 2 : 0]);
                int block = isSwitchGame ? 0 : mgs[gameIndex].actions.FindIndex(c => c.actionName == datamodels[1]) + 1;

                if (!isSwitchGame)
                {
                    // hardcoded stuff
                    // needs to happen because hidden blocks technically change the event index
                    if (datamodels[0] == "gameManager") block -= 2;
                    else if (datamodels[0] is "countIn" or "vfx") block -= 1;
                }

                GridGameSelector.instance.SelectGame(datamodels[isSwitchGame ? 2 : 0], block);
            }

            clickTimer = 0;
        }

        #endregion

        #region ResizeEvents

        public void DragEnter()
        {
            inResizeRegion = true;
        }

        public void DragExit()
        {
            inResizeRegion = false;
        }

        public void OnLeftDown()
        {
            if (resizable && selected)
            {
                ResetResize();
                resizing = true;
                resizingLeft = true;
            }
        }

        public void OnLeftUp()
        {
            if (resizable && selected)
            {
                ResetResize();
            }
        }

        public void OnRightDown()
        {
            if (resizable && selected)
            {
                ResetResize();
                resizing = true;
                resizingRight = true;
            }
        }

        public void OnRightUp()
        {
            if (resizable && selected)
            {
                ResetResize();
            }
        }

        private void ResetResize()
        {
            resizingLeft = false;
            resizingRight = false;
            resizing = false;
        }

        private void SetPivot(Vector2 pivot)
        {
            if (rectTransform == null) return;

            Vector2 size = rectTransform.rect.size;
            Vector2 deltaPivot = rectTransform.pivot - pivot;
            Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            rectTransform.pivot = pivot;
            rectTransform.localPosition -= deltaPosition;
        }

        #endregion

        #region Extra

        public void SetColor(int type)
        {
            var c = EditorTheme.theme.LayersGradient.Evaluate(type / (float)(Timeline.instance.LayerCount - 1));
            transform.GetChild(0).GetComponent<Image>().color = c;

            if (resizable)
            {
                c = new Color(0, 0, 0, 0.35f);
                resizeGraphic.color = c;
            }
        }

        public void SetWidthHeight()
        {
            rectTransform.sizeDelta = new Vector2(entity.length * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerHeight());
            Icon.rectTransform.sizeDelta = new Vector2(Timeline.instance.LayerHeight() - 8, Timeline.instance.LayerHeight() - 8);
            eventLabel.rectTransform.offsetMin = new Vector2(Icon.rectTransform.anchoredPosition.x + Icon.rectTransform.sizeDelta.x + 4, eventLabel.rectTransform.offsetMin.y);
        }

        public int GetTrack()
        {
            return (int)Mathf.Round(this.transform.localPosition.y / Timeline.instance.LayerHeight()) * -1;
        }

        private void OnDestroy()
        {
            // better safety net than canada's healthcare system
            // this is still hilarious
            // GameManager.instance.Beatmap.Entities.Remove(GameManager.instance.Beatmap.Entities.Find(c => c.eventObj = this));

            // FINALLY, A USE CASE FOR THIS FUNCTION
            if (Timeline.instance.eventObjs.Contains(this))
                Timeline.instance.eventObjs.Remove(this);
        }

        #endregion

        public void OnSelect()
        {
            selectedImage.gameObject.SetActive(true);
        }

        public void OnDeselect()
        {
            selectedImage.gameObject.SetActive(false);
        }
    }
}