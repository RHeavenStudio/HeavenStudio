using UnityEngine;
using UnityEngine.UI;

using Starpelly;
using Jukebox;
using TMPro;
using UnityEngine.Timeline;

namespace HeavenStudio.Editor.Track
{
    public class TimelineEventObj : MonoBehaviour
    {
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

        float leftSide, rightSide;

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

            rectTransform.anchoredPosition = new Vector2((float)entity.beat * Timeline.instance.PixelsPerBeat, -entity["track"] * Timeline.instance.LayerHeight());
            resizeGraphic.gameObject.SetActive(resizable);
            eventLabel.text = action.displayName;

            SetColor((int)entity["track"]);
        }

        public void SetEntity(RiqEntity entity)
        {
            this.entity = entity;
        }

        public void UpdateMarker()
        {
            mouseHovering = Timeline.instance.timelineState.selected && Timeline.instance.MousePos2Beat.IsWithin((float)entity.beat, (float)entity.beat + entity.length);

            Icon.rectTransform.sizeDelta = new Vector2(Timeline.instance.LayerHeight() - 8, Timeline.instance.LayerHeight() - 8);
            eventLabel.rectTransform.offsetMin = new Vector2(Icon.rectTransform.anchoredPosition.x + Icon.rectTransform.sizeDelta.x + 4, eventLabel.rectTransform.offsetMin.y);
            SetColor((int)entity["track"]);

            if (selected)
            {
                /*
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    Selections.instance.Deselect(this);
                    Timeline.instance.DestroyEventObject(entity);
                }
                */

                selectedImage.gameObject.SetActive(true);
                if (moving)
                    outline.color = Color.magenta;
                else
                    outline.color = Color.cyan;
            }
            else
            {
                selectedImage.gameObject.SetActive(false);

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


                // rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Timeline.instance.LayerHeight());
                // this.transform.localPosition = new Vector3(this.transform.localPosition.x, -entity["track"] * Timeline.instance.LayerHeight());
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

                        GameManager.instance.SortEventsList();
                    }
                }

                if (moving)
                {
                    foreach (var marker in Selections.instance.eventsSelected)
                    {
                        marker.entity.beat = Mathf.Max(Timeline.instance.MousePos2BeatSnap - marker.initMoveX, 0);
                        marker.entity["track"] = Mathf.Clamp(Timeline.instance.MousePos2Layer - marker.initMoveY, 0, Timeline.instance.LayerCount - 1);

                    }
                }

                /*
                int count = 0;
                foreach (TimelineEventObj e in Timeline.instance.eventObjs)
                {
                    if (e.moving)
                    {
                        count++;
                    }
                }

                if (count > 0 && selected)
                {
                    Vector3 mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);
                    // duplicate the entity if holding alt
                    if ((!wasDuplicated) && Input.GetKey(KeyCode.LeftAlt))
                    {
                        Selections.instance.Deselect(this);
                        this.wasDuplicated = false;
                        this.moving = false;

                        transform.localPosition = moveStartPos;
                        OnComplete(false);

                        TimelineEventObj te = Timeline.instance.CopyEventObject(this);
                        TimelineEventObj obj;

                        Selections.instance.DragSelect(te);

                        te.wasDuplicated = true;
                        te.transform.localPosition = transform.localPosition;
                        te.moveStartPos = transform.localPosition;

                        for (int i = 0; i < Timeline.instance.eventObjs.Count; i++)
                        {
                            obj = Timeline.instance.eventObjs[i];
                            obj.startPosX = mousePos.x - obj.transform.position.x;
                            obj.startPosY = mousePos.y - obj.transform.position.y;
                        }

                        te.moving = true;
                    }
                    else
                    {
                        this.transform.position = new Vector3(mousePos.x - startPosX, mousePos.y - startPosY - 0.40f, 0);
                        this.transform.localPosition = new Vector3(Mathf.Max(Mathp.Round2Nearest(this.transform.localPosition.x * Timeline.instance.PixelsPerBeat, Timeline.SnapInterval()), 0), Timeline.instance.SnapToLayer(this.transform.localPosition.y));
                    }

                    if (lastPos != transform.localPosition)
                    {
                        OnMove();
                    }

                    lastPos = transform.localPosition;
                }
                */
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

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Timeline.instance.LayerHeight());
            rectTransform.anchoredPosition = new Vector2((float)entity.beat * Timeline.instance.PixelsPerBeat, -entity["track"] * Timeline.instance.LayerHeight());
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
            Color c = Color.white;
            switch (type)
            {
                case 0:
                    c = EditorTheme.theme.properties.Layer1Col.Hex2RGB();
                    break;
                case 1:
                    c = EditorTheme.theme.properties.Layer2Col.Hex2RGB();
                    break;
                case 2:
                    c = EditorTheme.theme.properties.Layer3Col.Hex2RGB();
                    break;
                case 3:
                    c = EditorTheme.theme.properties.Layer4Col.Hex2RGB();
                    break;
                case 4:
                    c = EditorTheme.theme.properties.Layer5Col.Hex2RGB();
                    break;
            }
            // c = new Color(c.r, c.g, c.b, 0.85f);
            transform.GetChild(0).GetComponent<Image>().color = c;

            if (resizable)
            {
                c = new Color(0, 0, 0, 0.35f);
                resizeGraphic.color = c;
            }
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
    }
}