using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Starpelly;
using DG.Tweening;
using Jukebox;
using Jukebox.Legacy;

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
        [SerializeField] private RectTransform outline;
        [SerializeField] private RectTransform resizeGraphic;
        [SerializeField] private RectTransform leftDrag;
        [SerializeField] private RectTransform rightDrag;
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

        [Header("Colors")]
        public Color NormalCol;

        private void Start()
        {
            moveStartPos = transform.localPosition;

            rectTransform = GetComponent<RectTransform>();

            if (!resizable)
            {
                Destroy(resizeGraphic.gameObject);
            }

            // what the fuck????
            // moveTemp = new GameObject();
            // moveTemp.transform.SetParent(this.transform.parent);

            bool visible = rectTransform.IsVisibleFrom(Editor.instance.EditorCamera);
            for (int i = 0; i < this.transform.childCount; i++)
            {
                if (i != 4)
                    this.transform.GetChild(i).gameObject.SetActive(visible);
            }
        }

        private void Update()
        {
            selected = Selections.instance.eventsSelected.Contains(this);
            if (eventObjID != entity.uid)
            {
                eventObjID = GameManager.instance.Beatmap.Entities.Find(a => a == entity).uid;
                Debug.Log($"assigning uid {eventObjID}");
            }

            mouseHovering = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Editor.instance.EditorCamera) && Timeline.instance.timelineState.selected;

            #region Optimizations

            // thank you to @chrislo27 for suggesting the fix for this.
            // only renders blocks if they're in view of the timeline viewport
            var leftSide = rectTransform.localPosition.x;
            var rightSide = leftSide + rectTransform.sizeDelta.x;

            bool visible = (rightSide >= Timeline.instance.leftSide && leftSide <= Timeline.instance.rightSide);

            if (visible != lastVisible)
            {
                for (int i = 0; i < this.transform.childCount; i++)
                {
                    if (transform.GetChild(i).gameObject != selectedImage)
                        this.transform.GetChild(i).gameObject.SetActive(visible);
                }
            }

            lastVisible = visible;

            #endregion

            SetColor(entity["track"]);

            if (selected)
            {
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    /*Selections.instance.Deselect(this);
                    Timeline.instance.DestroyEventObject(entity);*/
                }

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
                    moving = false;
            
                if (selected)
                {
                    selected = false;
                    selectedImage.gameObject.SetActive(false);
                    for (int i = 0; i < outline.childCount; i++)
                        outline.GetChild(i).GetComponent<Image>().color = new Color32(0, 0, 0, 51);
                }

                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Timeline.instance.LayerHeight());
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, -entity["track"] * Timeline.instance.LayerHeight());
                return;
            }

            if (!resizing)
            {
                if (Timeline.instance.eventObjs.FindAll(c => c.moving).Count > 0 && selected)
                {
                    Vector3 mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);
                    //duplicate the entity if holding alt or m-click
                    if ((!wasDuplicated) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetMouseButton(2)))
                    {
                        Selections.instance.Deselect(this);
                        this.wasDuplicated = false;
                        this.moving = false;

                        transform.localPosition = moveStartPos;
                        OnComplete(false);

                        var te = Timeline.instance.CopyEventObject(this);

                        Selections.instance.DragSelect(te);

                        te.wasDuplicated = true;
                        te.transform.localPosition = transform.localPosition;
                        te.moveStartPos = transform.localPosition;

                        for (int i = 0; i < Timeline.instance.eventObjs.Count; i++)
                        {
                            Timeline.instance.eventObjs[i].startPosX = mousePos.x - Timeline.instance.eventObjs[i].transform.position.x;
                            Timeline.instance.eventObjs[i].startPosY = mousePos.y - Timeline.instance.eventObjs[i].transform.position.y;
                        }

                        te.moving = true;
                    }
                    else
                    {
                        this.transform.position = new Vector3(mousePos.x - startPosX, mousePos.y - startPosY - 0.40f, 0);
                        this.transform.localPosition = new Vector3(Mathf.Max(Mathp.Round2Nearest(this.transform.localPosition.x, Timeline.SnapInterval()), 0), Timeline.instance.SnapToLayer(this.transform.localPosition.y));
                    }

                    if (lastPos != transform.localPosition)
                    {
                        OnMove();
                    }

                    lastPos = transform.localPosition;
                }
            }
            else if (resizingLeft)
            {
                if (moving)
                    moving = false;

                SetPivot(new Vector2(1, rectTransform.pivot.y));
                Vector2 sizeDelta = rectTransform.sizeDelta;

                Vector2 mousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, Editor.instance.EditorCamera, out mousePos);

                sizeDelta = new Vector2(-mousePos.x + 0.15f, sizeDelta.y);
                sizeDelta = new Vector2(Mathf.Clamp(sizeDelta.x, Timeline.SnapInterval(), rectTransform.localPosition.x), sizeDelta.y);

                rectTransform.sizeDelta = new Vector2(Mathp.Round2Nearest(sizeDelta.x, Timeline.SnapInterval()), sizeDelta.y);
                SetPivot(new Vector2(0, rectTransform.pivot.y));
                OnComplete(false);
            }
            else if (resizingRight)
            {
                if (moving)
                    moving = false;

                Vector2 sizeDelta = rectTransform.sizeDelta;

                Vector2 mousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, Editor.instance.EditorCamera, out mousePos);

                sizeDelta = new Vector2(mousePos.x + 0.15f, sizeDelta.y);
                sizeDelta = new Vector2(Mathf.Clamp(sizeDelta.x, Timeline.SnapInterval(), Mathf.Infinity), sizeDelta.y);

                rectTransform.sizeDelta = new Vector2(Mathp.Round2Nearest(sizeDelta.x, Timeline.SnapInterval()), sizeDelta.y);
                SetPivot(new Vector2(0, rectTransform.pivot.y));
                OnComplete(false);
            }

            if (Input.GetMouseButtonUp(0))
            {
                OnLeftUp();
                OnRightUp();
            }

            if (resizing && selected || inResizeRegion && selected)
            {
                if (resizable)
                Cursor.SetCursor(Resources.Load<Texture2D>("Cursors/horizontal_resize"), new Vector2(8, 8), CursorMode.Auto);
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
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, -entity["track"] * Timeline.instance.LayerHeight());
        }

        #region ClickEvents

        public void OnClick()
        {
            if (Input.GetMouseButton(0) && Timeline.instance.timelineState.selected)
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
                if (selected && Timeline.instance.timelineState.selected)
                {
                    moveStartPos = transform.localPosition;

                    for (int i = 0; i < Timeline.instance.eventObjs.Count; i++)
                    {
                        Vector3 mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);
                        Timeline.instance.eventObjs[i].startPosX = mousePos.x - Timeline.instance.eventObjs[i].transform.position.x;
                        Timeline.instance.eventObjs[i].startPosY = mousePos.y - Timeline.instance.eventObjs[i].transform.position.y;
                    }

                    moving = true;
                }
            }
            else if (Input.GetMouseButton(1))
            {
                EventParameterManager.instance.StartParams(entity);
            }
        }

        public void OnUp()
        {
            // lastPos_ = this.lastPos_;
            // previousPos = this.transform.localPosition;

            if (selected && Timeline.instance.timelineState.selected)
            {
                if (wasDuplicated)
                {
                    Timeline.instance.FinalizeDuplicateEventStack();
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
            GameManager.instance.SortEventsList();
            entity["track"] = GetTrack();
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