using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Starpelly;
using DG.Tweening;
using Jukebox;
using Jukebox.Legacy;
using TMPro;

namespace HeavenStudio.Editor.Track
{
    public class TimelineEventObj : MonoBehaviour
    {
        private float startPosX;
        private float startPosY;

        private Vector3 lastPos;
        public Vector2 moveStartPos;
        private RectTransform rectTransform;

        [Header("Components")]
        [SerializeField] private RectTransform PosPreview;
        [SerializeField] private RectTransform PosPreviewRef;
        [SerializeField] public Image Icon;
        [SerializeField] private Image selectedImage;
        [SerializeField] public TMP_Text eventLabel;

        [SerializeField] private RectTransform resizeGraphicHolder;
        [SerializeField] private RectTransform resizeGraphicLeft;
        [SerializeField] private RectTransform resizeGraphicRight;
        [SerializeField] private RectTransform resizeGraphicLine;

        [SerializeField] private RectTransform leftDrag;
        [SerializeField] private RectTransform rightDrag;

        [SerializeField] private RectTransform outline;
        [SerializeField] private RectTransform outline1;
        [SerializeField] private RectTransform outline2;
        [SerializeField] private RectTransform outline3;
        [SerializeField] private RectTransform outline4;

        // private GameObject moveTemp;

        [Header("Properties")]
        public RiqEntity entity;
        public float length;
        public bool eligibleToMove = false;
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
        public int eventObjID;

        Timeline tl;
        float leftSide, rightSide;

        [Header("Colors")]
        public Color NormalCol;

        private void Start()
        {
            moveStartPos = transform.localPosition;

            rectTransform = GetComponent<RectTransform>();

            if (!resizable)
            {
                Destroy(resizeGraphicHolder.gameObject);
            }

            // what the fuck???? -- I wonder if I wrote this?
            // moveTemp = new GameObject();
            // moveTemp.transform.SetParent(this.transform.parent);

            bool visible = rectTransform.IsVisibleFrom(Editor.instance.EditorCamera);
            for (int i = 0; i < this.transform.childCount; i++)
            {
                if (i != 4)
                    this.transform.GetChild(i).gameObject.SetActive(visible);
            }

            tl = Timeline.instance;
        }

        private void Update()
        {
            if (tl == null)
                tl = Timeline.instance;

            selected = Selections.instance.eventsSelected.Contains(this);
            if (eventObjID != entity.uid)
            {
                eventObjID = GameManager.instance.Beatmap.Entities.Find(a => a == entity).uid;
                Debug.Log($"assigning uid {eventObjID}");
            }

            mouseHovering = tl.timelineState.selected && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Editor.instance.EditorCamera);

            #region Optimizations

            // thank you to @chrislo27 for suggesting the fix for this.
            // only renders blocks if they're in view of the timeline viewport
            leftSide = rectTransform.localPosition.x;
            rightSide = leftSide + rectTransform.sizeDelta.x;

            bool visible = rightSide >= tl.leftSide && leftSide <= tl.rightSide;

            if (visible != lastVisible)
            {
                for (int i = 0; i < this.transform.childCount; i++)
                {
                    // if (transform.GetChild(i).gameObject != selectedImage)
                    this.transform.GetChild(i).gameObject.SetActive(visible);
                }
            }

            lastVisible = visible;

            #endregion

            // We need a helper function for this
            // I'm aware how messy this is, but considering this is all going to be destroyed in a while and nobody
            // wants to touch it, I think it's fine.
            if (visible)
            {
                var timelineScale = 100.0f / Timeline.instance.TimelineContent.localScale.x;
                Icon.rectTransform.localScale = new Vector3(timelineScale, Icon.rectTransform.localScale.y, Icon.rectTransform.localScale.z);
                Icon.rectTransform.anchoredPosition = new Vector2(0.08f * timelineScale, Icon.rectTransform.anchoredPosition.y);

                if (resizeGraphicHolder != null)
                {
                    resizeGraphicHolder.offsetMin = new Vector2(0.04f * timelineScale, resizeGraphicHolder.offsetMin.y);
                    resizeGraphicHolder.offsetMax = new Vector2(0.04f * -timelineScale, resizeGraphicHolder.offsetMax.y);
                    resizeGraphicLeft.localScale = new Vector3(timelineScale, resizeGraphicLeft.localScale.y, resizeGraphicLeft.localScale.z);
                    resizeGraphicRight.localScale = new Vector3(-timelineScale, resizeGraphicRight.localScale.y, resizeGraphicRight.localScale.z);
                    resizeGraphicLine.offsetMin = new Vector2(0.132f * timelineScale, resizeGraphicLine.offsetMin.y);
                    resizeGraphicLine.offsetMax = new Vector2(0.132f * -timelineScale, resizeGraphicLine.offsetMax.y);
                }

                eventLabel.rectTransform.offsetMax = new Vector2(0.04f * -timelineScale, eventLabel.rectTransform.offsetMax.y);
                eventLabel.rectTransform.localScale = new Vector3(timelineScale * 0.01f, eventLabel.rectTransform.localScale.y, eventLabel.rectTransform.localScale.z);

                outline1.localScale = new Vector3(timelineScale, outline1.localScale.y, outline1.localScale.z);
                outline2.localScale = new Vector3(timelineScale, outline2.localScale.y, outline2.lossyScale.z);
                outline3.offsetMin = new Vector2(0.04f * timelineScale, outline3.offsetMin.y);
                outline3.offsetMax = new Vector2(0.04f * -timelineScale, outline3.offsetMax.y);
                outline4.offsetMin = new Vector2(0.04f * timelineScale, outline4.offsetMin.y);
                outline4.offsetMax = new Vector2(0.04f * -timelineScale, outline4.offsetMax.y);

                leftDrag.localScale = new Vector3(timelineScale, leftDrag.localScale.y, leftDrag.localScale.z);
                rightDrag.localScale = new Vector3(timelineScale, rightDrag.localScale.y, rightDrag.localScale.z);

                SetColor(entity["track"]);
            }

            if (selected)
            {
                /*
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    Selections.instance.Deselect(this);
                    tl.DestroyEventObject(entity);
                }
                */

                selectedImage.gameObject.SetActive(true);
                for (int i = 0; i < outline.childCount; i++)
                {
                    if (moving)
                        outline.GetChild(i).GetComponent<Image>().color = Color.magenta;
                    else
                        outline.GetChild(i).GetComponent<Image>().color = Color.cyan;
                }
            }
            else
            {
                selectedImage.gameObject.SetActive(false);

                for (int i = 0; i < outline.childCount; i++)
                    outline.GetChild(i).GetComponent<Image>().color = new Color32(0, 0, 0, 51);
            }

            if (Conductor.instance.NotStopped())
            {
                Cancel();

                if (moving)
                {
                    moving = false;
                }

                if (selected)
                {
                    selected = false;
                    selectedImage.gameObject.SetActive(false);
                    for (int i = 0; i < outline.childCount; i++)
                        outline.GetChild(i).GetComponent<Image>().color = new Color32(0, 0, 0, 51);
                }

                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, tl.LayerHeight());
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, -entity["track"] * tl.LayerHeight());
                return;
            }

            if (!resizing)
            {
                int count = 0;
                foreach (TimelineEventObj e in tl.eventObjs)
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

                        TimelineEventObj te = tl.CopyEventObject(this);
                        TimelineEventObj obj;

                        Selections.instance.DragSelect(te);

                        te.wasDuplicated = true;
                        te.transform.localPosition = transform.localPosition;
                        te.moveStartPos = transform.localPosition;

                        for (int i = 0; i < tl.eventObjs.Count; i++)
                        {
                            obj = tl.eventObjs[i];
                            obj.startPosX = mousePos.x - obj.transform.position.x;
                            obj.startPosY = mousePos.y - obj.transform.position.y;
                        }

                        te.moving = true;
                    }
                    else
                    {
                        this.transform.position = new Vector3(mousePos.x - startPosX, mousePos.y - startPosY - 0.40f, 0);
                        this.transform.localPosition = new Vector3(Mathf.Max(Mathp.Round2Nearest(this.transform.localPosition.x, Timeline.SnapInterval()), 0), tl.SnapToLayer(this.transform.localPosition.y));
                    }

                    if (lastPos != transform.localPosition)
                    {
                        OnMove();
                    }

                    lastPos = transform.localPosition;
                }
            }
            else
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
                OnComplete(false);
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
                    Cursor.SetCursor(tl.resizeCursor, new Vector2(14, 14), CursorMode.Auto);
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

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, tl.LayerHeight());
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, -entity["track"] * tl.LayerHeight());
        }

        #region ClickEvents

        public void OnClick()
        {
            if (Input.GetMouseButton(0) && tl.timelineState.selected)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Selections.instance.ShiftClickSelect(this);
                }
                else
                {
                    if (!selected)
                    {
                        Selections.instance.ClickSelect(this);
                    }
                }
            }
        }

        public void OnDown()
        {
            if (Input.GetMouseButton(0))
            {
                if (selected && tl.timelineState.selected)
                {
                    moveStartPos = transform.localPosition;

                    for (int i = 0; i < tl.eventObjs.Count; i++)
                    {
                        Vector3 mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);
                        tl.eventObjs[i].startPosX = mousePos.x - tl.eventObjs[i].transform.position.x;
                        tl.eventObjs[i].startPosY = mousePos.y - tl.eventObjs[i].transform.position.y;
                    }

                    moving = true;
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

        public void OnUp()
        {
            // lastPos_ = this.lastPos_;
            // previousPos = this.transform.localPosition;

            if (selected && tl.timelineState.selected)
            {
                if (wasDuplicated)
                {
                    tl.FinalizeDuplicateEventStack();
                    wasDuplicated = false;
                }
                if (eligibleToMove)
                {
                    OnComplete(true);
                    moveStartPos = transform.localPosition;
                }

                moving = false;

                Cancel();
                if (isCreating == true)
                {
                    isCreating = false;
                    CommandManager.instance.Execute(new Commands.Place(this));
                }
            }
        }

        private void Cancel()
        {
            eligibleToMove = false;
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

        #region OnEvents

        private void OnMove()
        {
            if (GameManager.instance.Beatmap.Entities.FindAll(c => c.beat == this.transform.localPosition.x && c["track"] == GetTrack() && c != this.entity).Count > 0)
            {
                eligibleToMove = false;
            }
            else
            {
                eligibleToMove = true;
            }

            OnComplete(true);
        }

        private void OnComplete(bool move)
        {
            entity.length = rectTransform.sizeDelta.x;
            entity.beat = this.transform.localPosition.x;
            entity["track"] = GetTrack();
            
            GameManager.instance.SortEventsList();
        }

        #endregion

        #region Selection

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
                transform.GetChild(1).GetChild(0).GetComponent<Image>().color = c;
                transform.GetChild(1).GetChild(1).GetComponent<Image>().color = c;
                transform.GetChild(1).GetChild(2).GetComponent<Image>().color = c;
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