using UnityEngine;
using UnityEngine.UI;

using Starpelly;
using Jukebox;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Timeline;

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
        public bool moving;
        public bool wasDuplicated;

        public bool resizing => resizingLeft || resizingRight;
        private bool resizingLeft;
        private bool resizingRight;
        private bool inResizeRegion;
        private float resizingLeftBL;

        private double lastResizeBeat = 0;
        private float lastResizeLength = 0;

        public bool isCreating;

        private bool altWhenClicked = false;
        private bool dragging = false;

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
                this.resizable = gameAction.resizable;
                if (gameAction.resizable == false)
                {
                    rectTransform.sizeDelta = new Vector2(gameAction.defaultLength * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerHeight());
                    this.length = gameAction.defaultLength;
                }
                else
                {
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

            eventLabel.overflowMode = (mouseHovering || moving || resizing || inResizeRegion) ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;

            if (selected)
            {
                if (TimelineBlockManager.Instance.MovingAnyEvents)
                {
                    outline.color = Color.magenta;
                    SetColor((int)entity["track"]);
                }
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
                    Selections.instance.Deselect(this);
                    outline.color = new Color32(0, 0, 0, 51);
                }
                return;
            }

            if (resizingRight)
            {
                if (moving) moving = false;

                entity.length = Mathf.Max(Timeline.instance.MousePos2BeatSnap - (float)entity.beat, Timeline.instance.snapInterval);

                SetWidthHeight();
            }
            else if (resizingLeft)
            {
                if (moving) moving = false;

                entity.beat = Mathf.Min(Timeline.instance.MousePos2BeatSnap, resizingLeftBL - Timeline.instance.snapInterval);
                entity.length = Mathf.Max(resizingLeftBL - (float)entity.beat, Timeline.instance.snapInterval);

                SetWidthHeight();
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
                            // NOTE (PELLY): Replace with arrays soon
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

                    altWhenClicked = false;
                    dragging = false;
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

            // should consider adding this someday
            // else if (moving && selected || mouseHovering && selected)
            // {
            //     Cursor.SetCursor(Resources.Load<Texture2D>("Cursors/move"), new Vector2(8, 8), CursorMode.Auto);
            // }

            zPriority = entity.length;

            if (selected)
                zPriority += 10000;
        }

        public void LateUpdate()
        {
            rectTransform.anchoredPosition = new Vector2((float)entity.beat * Timeline.instance.PixelsPerBeat, -(int)entity["track"] * Timeline.instance.LayerHeight());
            SetWidthHeight();

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
            if (!selected && !TimelineBlockManager.Instance.MovingAnyEvents) selectedImage.gameObject.SetActive(true);
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
            if (Conductor.instance.NotStopped()) return;

            if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) return;
            if (!moving)
                if (!altWhenClicked)
                    altWhenClicked = Input.GetKey(KeyCode.LeftAlt);

            if (!altWhenClicked)
            {
                if (!selected)
                    Selections.instance.ClickSelect(this);
                if (!moving)
                    BeginMoving();

                return;
            }

            if (dragging) return;

            var entities = Selections.instance.eventsSelected;
            if (entities.Count == 0)
            {
                entities = new() { this };
            }
            CommandManager.Instance.AddCommand(new Commands.Duplicate(entities));

            dragging = true;
        }

        public void OnDown()
        {
            if (Conductor.instance.NotStopped()) return;
            if (moving) return;

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
                    else if (!selected)
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

        public void ResizeEnter()
        {
            if (Conductor.instance.NotStopped()) return;
            if (BoxSelection.instance.ActivelySelecting || !resizable || moving) return;

            inResizeRegion = true;

            Cursor.SetCursor(Timeline.instance.resizeCursor, new Vector2(14, 14), CursorMode.Auto);
        }

        public void ResizeExit()
        {
            inResizeRegion = false;

            if (!resizing)
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        public void OnLeftDown()
        {
            if (BoxSelection.instance.ActivelySelecting) return;

            if (resizable && selected)
            {
                ResetResize();

                resizingLeft = true;

                resizingLeftBL = (float)entity.beat + entity.length;

                lastResizeBeat = entity.beat;
                lastResizeLength = entity.length;
            }
        }

        public void OnLeftUp()
        {
            if (resizable && resizingLeft)
            {
                ResetResize();
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

                var b = entity.beat;
                var l = entity.length;

                entity.beat = lastResizeBeat;
                entity.length = lastResizeLength;

                CommandManager.Instance.AddCommand(new Commands.Resize(entity.guid, b, l));
            }
        }

        public void OnRightDown()
        {
            if (BoxSelection.instance.ActivelySelecting) return;

            if (resizable && selected)
            {
                ResetResize();

                resizingRight = true;

                lastResizeBeat = entity.beat;
                lastResizeLength = entity.length;
            }
        }

        public void OnRightUp()
        {
            if (resizable && resizingRight)
            {
                ResetResize();
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

                var b = entity.beat;
                var l = entity.length;

                entity.beat = lastResizeBeat;
                entity.length = lastResizeLength;

                CommandManager.Instance.AddCommand(new Commands.Resize(entity.guid, b, l));
            }
        }

        private void ResetResize()
        {
            resizingLeft = false;
            resizingRight = false;
        }

        #endregion

        #region Extra

        public void SetColor(int type)
        {
            var c = EditorTheme.theme.LayerGradientIndex(type);
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
        }

        #endregion

        public void OnSelect()
        {
            // selectedImage.gameObject.SetActive(true);
        }

        public void OnDeselect()
        {
            // selectedImage.gameObject.SetActive(false);
        }
    }
}