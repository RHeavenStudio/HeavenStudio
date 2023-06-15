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

        public static GridGameSelector instance;

        private void Start()
        {
            instance = this;
            GameEventSelectorRect = GameEventSelector.GetComponent<RectTransform>();
            selectorHeight = GameEventSelectorRect.rect.height;
            eventSize = EventRef.GetComponent<RectTransform>().rect.height;

            eventsParent = EventRef.transform.parent.GetChild(2).GetComponent<RectTransform>();
            SelectGame("Game Manager");

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
            eventSize = EventRef.GetComponent<RectTransform>().rect.height;
            // EventRef.transform.parent.DOKill();
            float lastLocalY = EventRef.transform.parent.transform.localPosition.y;

            if (currentEventIndex * eventSize >= selectorHeight/2 && eventsParent.childCount * eventSize >= selectorHeight)
            {
                if (currentEventIndex * eventSize < eventsParent.childCount * eventSize - selectorHeight/2)
                {
                    EventRef.transform.parent.transform.localPosition = new Vector3(
                        EventRef.transform.parent.transform.localPosition.x, 
                        Mathf.Lerp(lastLocalY, (currentEventIndex * eventSize) - selectorHeight/2, 12 * Time.deltaTime),
                        EventRef.transform.parent.transform.localPosition.z
                    );
                }
                else
                {
                    EventRef.transform.parent.transform.localPosition = new Vector3(
                        EventRef.transform.parent.transform.localPosition.x, 
                        Mathf.Lerp(lastLocalY, (eventsParent.childCount * eventSize) - selectorHeight + (eventSize*0.33f), 12 * Time.deltaTime),
                        EventRef.transform.parent.transform.localPosition.z
                    );
                }
            }
            else
            {
                EventRef.transform.parent.transform.localPosition = new Vector3(
                    EventRef.transform.parent.transform.localPosition.x, 
                    Mathf.Lerp(lastLocalY, 0, 12 * Time.deltaTime),
                    EventRef.transform.parent.transform.localPosition.z
                );
            }
            SetColors();
        }

        // will automatically select game + game icon
        // index is the event it will highlight (which was basically just added for pick block)
        // TODO: automatically scroll if the game is offscreen, because i can't figure it out rn. -AJ
        public void SelectGame(string gameName, int index = 0)
        {
            if (SelectedGameIcon != null)
            {
                SelectedGameIcon.GetComponent<GridGameSelectorGame>().UnClickIcon();
            }
            var mgs = EventCaller.instance.minigames;
            int mgIndex = 0, hidden = 0;
            for (int i = 0; i < mgs.Count; i++)
            {
                if (mgs[i].displayName == gameName && !mgs[i].hidden) {
                    mgIndex = i;
                    break;
                } else if (mgs[i].hidden) {
                    hidden++;
                }
            }
            
            mg = mgs[mgIndex];
            SelectedMinigame = gameName;
            gameOpen = true;

            DestroyEvents();
            AddEvents(index);

            // transform.GetChild(index).GetChild(0).gameObject.SetActive(true);
            mgIndex -= hidden;
            SelectedGameIcon = transform.GetChild(mgIndex+1).gameObject;
            SelectedGameIcon.GetComponent<GridGameSelectorGame>().ClickIcon();

            currentEventIndex = index;
            UpdateIndex(index, false);

            Editor.instance?.SetGameEventTitle($"Select game event for {gameName.Replace("\n", "")}");
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