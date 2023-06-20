using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;
using DG.Tweening;
using Starpelly;

using HeavenStudio.Editor.Track;


namespace HeavenStudio.Editor
{
    public class GridGameSelector : MonoBehaviour
    {
        public string SelectedMinigame;

        [Header("Components")]
        public GameObject SelectedGameIcon;
        public GameObject GameEventSelector;
        public GameObject EventRef;
        public GameObject CurrentSelected;
        public GameObject Scrollbar;
        public RectTransform GameSelectionRect;
        public RectTransform GameEventSelectorCanScroll;
        private RectTransform GameEventSelectorRect;
        private RectTransform eventsParent;

        [Header("Properties")]
        [SerializeField] private int currentEventIndex;
        private Minigames.Minigame mg;
        public float posDif;
        public int ignoreSelectCount;
        private int sortStatus;
        private int dragTimes;
        private bool gameOpen;
        private float selectorHeight;
        private float eventSize;

        public static GridGameSelector instance;

        private void Start()
        {
            instance = this;
            GameEventSelectorRect = GameEventSelector.GetComponent<RectTransform>();
            selectorHeight = GameEventSelectorRect.rect.height;
            eventSize = EventRef.GetComponent<RectTransform>().rect.height;

            eventsParent = EventRef.transform.parent.GetChild(2).GetComponent<RectTransform>();
            SelectGame("gameManager");

            //SetColors();
        }

        private void Update()
        {
            if (!EventParameterManager.instance.active && !IsPointerOverUIElement())
            {
                if (gameOpen)
                {
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        UpdateIndex(currentEventIndex + 1);
                    }
                    else if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        UpdateIndex(currentEventIndex - 1);
                    }
                }

                if (RectTransformUtility.RectangleContainsScreenPoint(GameEventSelectorCanScroll, Input.mousePosition, Editor.instance.EditorCamera) && Input.mouseScrollDelta.y != 0)
                {
                    UpdateIndex(currentEventIndex - Mathf.RoundToInt(Input.mouseScrollDelta.y));
                }
            }

            //moved here so this updates dynamically with window scale
            UpdateScrollPosition();
        }

        #region Functions

        public void UpdateIndex(int amount, bool updateCol = true)
        {
            currentEventIndex = amount;

            CurrentSelected.transform.DOKill();

            if (currentEventIndex < 0)
                currentEventIndex = eventsParent.childCount - 1;
            else if (currentEventIndex > eventsParent.childCount - 1)
                currentEventIndex = 0;

            CurrentSelected.transform.DOLocalMoveY(eventsParent.transform.GetChild(currentEventIndex).localPosition.y + eventsParent.transform.localPosition.y, 0.35f).SetEase(Ease.OutExpo);
        }

        private void UpdateScrollPosition()
        {
            selectorHeight = GameEventSelectorRect.rect.height;
            //eventSize = EventRef.GetComponent<RectTransform>().rect.height;
            
            Vector3 lastPos = EventRef.transform.parent.transform.localPosition;
            float end = 0;

            if ((currentEventIndex * eventSize >= selectorHeight/2) && (eventsParent.childCount * eventSize >= selectorHeight))
            {
                if (currentEventIndex * eventSize < eventsParent.childCount * eventSize - selectorHeight/2)
                    end = (currentEventIndex * eventSize) - selectorHeight/2;
                else
                    end = (eventsParent.childCount * eventSize) - selectorHeight + (eventSize * 0.33f);
            }
            EventRef.transform.parent.transform.localPosition = new Vector3(
                lastPos.x, 
                Mathf.Lerp(lastPos.y, end, 12 * Time.deltaTime),
                lastPos.z
            );
            SetColors();
        }

        // will automatically select game + game icon, and scroll to the game if it's offscreen
        // index is the event it will highlight (which was basically just added for pick block)
        // TODO: automatically scroll if the game is offscreen, because i can't figure it out/can't figure out a good way to do it rn. -AJ
        public void SelectGame(string gameName, int index = 0)
        {
            EventParameterManager.instance.Disable();
            if (SelectedGameIcon != null)
            {
                SelectedGameIcon.GetComponent<GridGameSelectorGame>().UnClickIcon();
            }

            mg = EventCaller.instance.GetMinigame(gameName);
            if (mg == null) {
                SelectGame("gameManager");
                Debug.LogWarning($"SelectGame() has failed, did you mean to input '{gameName}'?");
                return;
            }
            
            SelectedMinigame = gameName;
            gameOpen = true;

            DestroyEvents();
            AddEvents(index);

            SelectedGameIcon = transform.Find(gameName).gameObject;
            SelectedGameIcon.GetComponent<GridGameSelectorGame>().ClickIcon();

            currentEventIndex = index;
            UpdateIndex(index, false);

            Editor.instance?.SetGameEventTitle($"Select game event for {mg.displayName.Replace("\n", "")}");

            // should auto scroll if it's offscreen
            // just barely doesn't work, and works even less with zooming. im sure there's a much better way to do it
            /*
            var pos = 1 - ((mgIndex / 4)-0.02f) / (Mathf.Ceil(mgsActive / 4) - 3);
            var posMin = (pos + Viewport.rect.height); //+ (pos * 0.01f);
            if (Scrollbar.value < pos) Scrollbar.value = pos;
            else if (Scrollbar.value > posMin) Scrollbar.value = posMin;
            */
        }

        private void AddEvents(int index = 0)
        {
            if (!EventCaller.FXOnlyGames().Contains(EventCaller.instance.GetMinigame(mg.name)))
            {
                GameObject sg = Instantiate(EventRef, eventsParent);
                sg.GetComponent<TMP_Text>().text = "Switch Game";
                sg.SetActive(true);
                if (index == 0) sg.GetComponent<TMP_Text>().color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();
            } else {
                index++;
                if (mg.name == "gameManager") index++;
            }

            for (int i = 0; i < mg.actions.Count; i++)
            {
                if (mg.actions[i].actionName == "switchGame" || mg.actions[i].hidden) continue;
                GameObject g = Instantiate(EventRef, eventsParent);
                g.GetComponent<TMP_Text>().text = mg.actions[i].displayName;
                g.SetActive(true);
                if (index - 1 == i) g.GetComponent<TMP_Text>().color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();
            }
        }

        private void DestroyEvents()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
            }

            for (int i = 0; i < eventsParent.childCount; i++)
            {
                Destroy(eventsParent.GetChild(i).gameObject);
            }
        }

        private void SetColors()
        {
            //CurrentSelected.GetComponent<Image>().color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();

            for (int i = 0; i < eventsParent.transform.childCount; i++)
                            eventsParent.GetChild(i).GetComponent<TMP_Text>().color = EditorTheme.theme.properties.EventNormalCol.Hex2RGB();

            eventsParent.GetChild(currentEventIndex).GetComponent<TMP_Text>().color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();
        }

        // TODO: find the equation to get the sizes automatically, nobody's been able to figure one out yet (might have to be manual?)
        public void Zoom()
        {
            if (!Input.GetKey(KeyCode.LeftControl)) return;
            var glg = GetComponent<GridLayoutGroup>();
            var sizes = new List<float>() {
                209.5f,
                102.3f,
                66.6f,
                48.6f,
                37.9f,
                30.8f,
                25.7f,
                24.3f,
            };

            if (glg.constraintCount + 1 > sizes.Count && Input.GetAxisRaw("Mouse ScrollWheel") < 0) return;

            glg.constraintCount += (Input.GetAxisRaw("Mouse ScrollWheel") > 0) ? -1 : 1;
            glg.cellSize = new Vector2(sizes[glg.constraintCount - 1], sizes[glg.constraintCount - 1]);
        }

        // method called when clicking the sort button in the editor, skips sorting first three "games"
        // sorts by favorites if there are any, and sorts alphabetically if there aren't.
        public void Sort()
        {
            var mgs = EventCaller.instance.minigames;
            var mgsActive = new List<string>();
            for (int i = 3; i < mgs.Count; i++)
            {
                if (!mgs[i].hidden) mgsActive.Add(mgs[i].name);
            }
            mgsActive.Sort();
            var favs = new List<Transform>();
            bool fav = false;
            for (int i = 0; i < mgsActive.Count; i++)
            {
                var mg = transform.Find(mgsActive[i]);
                if (mg.GetComponent<GridGameSelectorGame>().StarActive) {
                    favs.Add(mg);
                    fav = true;
                }
            }
            if (!fav) {
                for (int i = 0; i < mgsActive.Count; i++)
                    transform.Find(mgsActive[i]).SetSiblingIndex(i+4);
            } else {
                for (int i = 0; i < favs.Count; i++)
                    favs[i].SetSiblingIndex(i+4);
            }
        }

        public bool IsPointerOverUIElement()
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults());
        }

        private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
        {
            for (int index = 0; index < eventSystemRaysastResults.Count; index++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[index];
                if (curRaysastResult.gameObject.layer == 5)
                    return true;
            }
            return false;
        }

        static List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            return raysastResults;
        }

        #endregion

        #region Events

        public void Drag()
        {
            if (Conductor.instance.NotStopped() || Editor.instance.inAuthorativeMenu) return;
            
            if (Timeline.instance.CheckIfMouseInTimeline() && dragTimes < 1)
            {
                Timeline.instance.timelineState.SetState(Timeline.CurrentTimelineState.State.Selection);
                dragTimes++;

                TimelineEventObj eventObj;

                if (currentEventIndex == 0)
                {
                    if (EventCaller.FXOnlyGames().Contains(EventCaller.instance.GetMinigame(mg.name)))
                    {
                        int index = currentEventIndex + 1;
                        if (currentEventIndex - 1 > mg.actions.Count)
                        {
                            index = currentEventIndex;
                        }
                        else if (currentEventIndex - 1 < 0)
                        {
                            if (mg.actions[0].actionName == "switchGame")
                                index = 1;
                            else
                                index = 0;
                        }

                        eventObj = Timeline.instance.AddEventObject(mg.name + "/" + mg.actions[index].actionName, true, new Vector3(0, 0), null, true);
                    }
                    else
                        eventObj = Timeline.instance.AddEventObject($"gameManager/switchGame/{mg.name}", true, new Vector3(0, 0), null, true);
                }
                else
                {
                    int index = currentEventIndex - 1;
                    if (mg.actions[0].actionName == "switchGame")
                    {
                        index = currentEventIndex + 1;
                    }
                    else if (EventCaller.FXOnlyGames().Contains(EventCaller.instance.GetMinigame(mg.name)) && mg.actions[0].actionName != "switchGame")
                    {
                        index = currentEventIndex;
                    }

                    eventObj = Timeline.instance.AddEventObject(mg.name + "/" + mg.actions[index].actionName, true, new Vector3(0, 0), null, true);
                }

                eventObj.isCreating = true;

                // CommandManager.instance.Execute(new Commands.Place(eventObj));
            }
        }

        public void Drop()
        {
            if (Conductor.instance.NotStopped()) return;

            dragTimes = 0;
        }

        #endregion
    }
}