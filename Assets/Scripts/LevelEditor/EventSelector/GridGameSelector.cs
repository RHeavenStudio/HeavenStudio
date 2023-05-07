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
        public RectTransform GameEventSelectorCanScroll;
        private RectTransform GameEventSelectorRect;
        private RectTransform eventsParent;

        [Header("Properties")]
        [SerializeField] private int currentEventIndex;
        private Minigames.Minigame mg;
        private bool gameOpen;
        private int dragTimes;
        public float posDif;
        public int ignoreSelectCount;
        private float selectorHeight;
        private float eventSize;

        private void Start()
        {
            GameEventSelectorRect = GameEventSelector.GetComponent<RectTransform>();
            selectorHeight = GameEventSelectorRect.rect.height;
            eventSize = EventRef.GetComponent<RectTransform>().rect.height;

            eventsParent = EventRef.transform.parent.GetChild(2).GetComponent<RectTransform>();
            SelectGame("Game Manager", 1);

            SetColors();
        }

        private void Update()
        {
            if (!(EventParameterManager.instance.active || Conductor.instance.NotStopped()) && !IsPointerOverUIElement())
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

            EventRef.transform.parent.DOKill();
            CurrentSelected.transform.DOKill();

            if (currentEventIndex < 0)
                currentEventIndex = eventsParent.childCount - 1;
            else if (currentEventIndex > eventsParent.childCount - 1)
                currentEventIndex = 0;

            CurrentSelected.transform.DOLocalMoveY(eventsParent.transform.GetChild(currentEventIndex).localPosition.y + eventsParent.transform.localPosition.y, 0.35f).SetEase(Ease.OutExpo);

            if (updateCol)
            SetColors(currentEventIndex);
        }

        private void UpdateScrollPosition()
        {
            selectorHeight = GameEventSelectorRect.rect.height;
            eventSize = EventRef.GetComponent<RectTransform>().rect.height;

            if (currentEventIndex * eventSize >= selectorHeight/2 && eventsParent.childCount * eventSize >= selectorHeight)
            {
                if (currentEventIndex * eventSize < eventsParent.childCount * eventSize - selectorHeight/2)
                {
                    EventRef.transform.parent.DOLocalMoveY((currentEventIndex * eventSize) - selectorHeight/2, 0.35f).SetEase(Ease.OutExpo);
                }
                else
                {
                    EventRef.transform.parent.DOLocalMoveY((eventsParent.childCount * eventSize) - selectorHeight + (eventSize*0.33f), 0.35f).SetEase(Ease.OutExpo);
                }
            }
            else
                EventRef.transform.parent.DOLocalMoveY(0, 0.35f).SetEase(Ease.OutExpo);
        }

        public void SelectGame(string gameName, int index)
        {
            if (SelectedGameIcon != null)
            {
                SelectedGameIcon.GetComponent<GridGameSelectorGame>().UnClickIcon();
            }
            mg = EventCaller.instance.minigames.Find(c => c.displayName == gameName);
            SelectedMinigame = gameName;
            gameOpen = true;

            DestroyEvents();
            AddEvents();

            // transform.GetChild(index).GetChild(0).gameObject.SetActive(true);
            SelectedGameIcon = transform.GetChild(index).gameObject;
            SelectedGameIcon.GetComponent<GridGameSelectorGame>().ClickIcon();

            currentEventIndex = 0;
            UpdateIndex(0, false);

            Editor.instance.SetGameEventTitle($"Select game event for {gameName.Replace("\n", "")}");
        }

        private void AddEvents()
        {
            if (!EventCaller.FXOnlyGames().Contains(EventCaller.instance.GetMinigame(mg.name)))
            {
                GameObject sg = Instantiate(EventRef, eventsParent);
                sg.GetComponent<TMP_Text>().text = "Switch Game";
                sg.SetActive(true);
                sg.GetComponent<TMP_Text>().color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();
            }

            for (int i = 0; i < mg.actions.Count; i++)
            {
                if (mg.actions[i].actionName == "switchGame" || mg.actions[i].hidden) continue;
                GameObject g = Instantiate(EventRef, eventsParent);
                g.GetComponent<TMP_Text>().text = mg.actions[i].displayName;
                g.SetActive(true);
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

        private void SetColors(int index = 0)
        {
            CurrentSelected.GetComponent<Image>().color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();

            for (int i = 0; i < eventsParent.transform.childCount; i++)
                eventsParent.GetChild(i).GetComponent<TMP_Text>().color = EditorTheme.theme.properties.EventNormalCol.Hex2RGB();

            eventsParent.GetChild(index).GetComponent<TMP_Text>().color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();
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

                        eventObj = Timeline.instance.AddEventObject(mg.name + "/" + mg.actions[index].actionName, true, new Vector3(0, 0), null, true, Timeline.RandomID());
                    }
                    else
                        eventObj = Timeline.instance.AddEventObject($"gameManager/switchGame/{mg.name}", true, new Vector3(0, 0), null, true, Timeline.RandomID());
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

                    eventObj = Timeline.instance.AddEventObject(mg.name + "/" + mg.actions[index].actionName, true, new Vector3(0, 0), null, true, Timeline.RandomID());
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