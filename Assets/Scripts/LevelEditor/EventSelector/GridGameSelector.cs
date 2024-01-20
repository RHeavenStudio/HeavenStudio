using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;
using DG.Tweening;
using Starpelly;

using HeavenStudio.Editor.Track;
using System.Text;

namespace HeavenStudio.Editor
{
    // I hate the antichrist.
    public class GridGameSelector : MonoBehaviour
    {
        public Minigames.Minigame SelectedMinigame;

        [Header("Components")]
        public GameObject SelectedGameIcon;
        public GameObject GameEventSelector;
        public GameObject EventRef;
        public GameObject CurrentSelected;
        public Scrollbar Scrollbar;
        public RectTransform GameSelectionRect;
        public RectTransform GameEventSelectorCanScroll;
        public TMP_InputField SearchBar;
        private RectTransform GameEventSelectorRect;
        private RectTransform eventsParent;

        [Header("Properties")]
        [SerializeField] private int currentEventIndex;
        public List<RectTransform> mgsActive = new List<RectTransform>();
        public List<RectTransform> fxActive = new List<RectTransform>();
        public float posDif;
        public int ignoreSelectCount;
        private int dragTimes;
        private bool gameOpen;
        private float selectorHeight;
        private float eventSize;
        private float timeSinceUpdateIndex = 0.0f;

        public static GridGameSelector instance;

        private void Start()
        {
            instance = this;
            GameEventSelectorRect = GameEventSelector.GetComponent<RectTransform>();
            selectorHeight = GameEventSelectorRect.rect.height;
            eventSize = EventRef.GetComponent<RectTransform>().rect.height;

            eventsParent = EventRef.transform.parent.GetChild(2).GetComponent<RectTransform>();
            SelectGame(fxActive[0].name);
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
            timeSinceUpdateIndex = 0;
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

            timeSinceUpdateIndex += Time.deltaTime;

            CurrentSelected.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(
                    (Mathf.Cos(timeSinceUpdateIndex * 2.65f) * 12) + 12,
                    CurrentSelected.GetComponent<RectTransform>().anchoredPosition.y);
            SetColors();
        }

        // will automatically select game + game icon, and (eventually?) scroll to the game if it's offscreen
        // index is the event it will highlight (which was basically just added for pick block)
        // TODO: automatically scroll if the game is offscreen, because i can't figure out a good way to do it rn. -AJ
        public void SelectGame(string gameName, int index = 0)
        {
            if (SelectedGameIcon != null)
            {
                SelectedGameIcon.GetComponent<GridGameSelectorGame>().UnClickIcon();
            }

            SelectedMinigame = EventCaller.instance.GetMinigame(gameName);
            if (SelectedMinigame == null) {
                SelectGame("gameManager");
                Debug.LogWarning($"SelectGame() has failed, did you mean to input '{gameName}'?");
                return;
            }

            EventParameterManager.instance.Disable();

            gameOpen = true;

            DestroyEvents();
            AddEvents(index);

            SelectedGameIcon = transform.Find(gameName).gameObject;
            SelectedGameIcon.GetComponent<GridGameSelectorGame>().ClickIcon();

            currentEventIndex = index;
            UpdateIndex(index, false);

            Editor.instance?.SetGameEventTitle($"Select game event for {SelectedMinigame.displayName.Replace("\n", "")}");
        }

        private void AddEvents(int index = 0)
        {
            if (!EventCaller.FXOnlyGames().Contains(SelectedMinigame))
            {
                GameObject sg = Instantiate(EventRef, eventsParent);
                sg.GetComponentInChildren<TMP_Text>().text = "Switch Game";
                sg.SetActive(true);
                if (index == 0) sg.GetComponentInChildren<TMP_Text>().color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();
            } else {
                index++;
                if (SelectedMinigame.name == "gameManager") index++;
            }

            for (var i = 0; i < SelectedMinigame.actions.Count; i++)
            {
                var action = SelectedMinigame.actions[i];
                if (action.actionName == "switchGame" || action.hidden) continue;

                var g = Instantiate(EventRef, eventsParent);
                var label = g.GetComponentInChildren<TMP_Text>();

                label.text = action.displayName;
                if (action.parameters != null && action.parameters.Count > 0)
                    g.transform.GetChild(1).gameObject.SetActive(true);

                if (index - 1 == i)
                    label.color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();

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

        private void SetColors()
        {
            //CurrentSelected.GetComponent<Image>().color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();

            for (int i = 0; i < eventsParent.transform.childCount; i++)
            {
                var eventTxt = eventsParent.GetChild(i).GetChild(0).GetComponent<TMP_Text>();
                var goalX = -25;
                if (i == currentEventIndex)
                {
                    eventTxt.color = EditorTheme.theme.properties.EventSelectedCol.Hex2RGB();
                    goalX = 16;
                }
                else
                {
                    eventTxt.color = EditorTheme.theme.properties.EventNormalCol.Hex2RGB();
                }
                eventTxt.rectTransform.anchoredPosition =
                    new Vector2(
                        Mathf.Lerp(eventTxt.rectTransform.anchoredPosition.x, goalX, Time.deltaTime * 12f),
                        eventTxt.rectTransform.anchoredPosition.y);
            }
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
                //21.9f,
            };

            if (glg.constraintCount + 1 > sizes.Count && Input.GetAxisRaw("Mouse ScrollWheel") < 0) return;

            glg.constraintCount += (Input.GetAxisRaw("Mouse ScrollWheel") > 0) ? -1 : 1;
            glg.cellSize = Vector2.one * sizes[glg.constraintCount - 1];
        }

        // method called when clicking the sort button in the editor, skips sorting fx only "games"
        // sorts depending on which sorting button you click
        public void Sort(string type)
        {
            var mgsSort = mgsActive;
            mgsSort.Sort((x, y) => string.Compare(x.name, y.name));

            switch (type)
            {
                case "favorites":
                SortFavorites(mgsSort);
                break;
                case "chronologic":
                SortChronologic(mgsSort);
                break;
                default: // "alphabet"
                SortAlphabet(mgsSort);
                break;
            }
        }

        void SortAlphabet(List<RectTransform> mgs)
        {
            for (int i = 0; i < mgsActive.Count; i++) {
                mgs[i].SetSiblingIndex(i + fxActive.Count + 1);
            }
        }

        // if there are no favorites, the games will sort alphabetically
        void SortFavorites(List<RectTransform> allMgs)
        {
            var favs = allMgs.FindAll(mg => mg.GetComponent<GridGameSelectorGame>().StarActive);
            var mgs  = allMgs.FindAll(mg => !mg.GetComponent<GridGameSelectorGame>().StarActive);

            if (Input.GetKey(KeyCode.LeftShift)) {
                foreach (var fav in favs)
                    fav.GetComponent<GridGameSelectorGame>().Star();
                return;
            }

            for (int i = 0; i < favs.Count; i++) {
                favs[i].SetSiblingIndex(i + fxActive.Count + 1);
            }
            for (int i = 0; i < mgs.Count; i++) {
                mgs[i].SetSiblingIndex(i + fxActive.Count + favs.Count + 1);
            }
        }

        void SortChronologic(List<RectTransform> mgs)
        {
            var systems = new List<RectTransform>[] {
                new List<RectTransform>(),
                new List<RectTransform>(),
                new List<RectTransform>(),
                new List<RectTransform>(),
                new List<RectTransform>(),
                new List<RectTransform>(),
            };
            for (int i = 0; i < mgs.Count; i++)
            {
                var mg = EventCaller.instance.GetMinigame(mgs[i].name);
                var tags = mg.tags;
                if (tags.Count != 0) {
                    systems[tags[0] switch {
                        "agb" => 0,
                        "ntr" => 1,
                        "rvl" => 2,
                        "ctr" => 3,
                        "mob" => 4,
                        _     => 5,
                    }].Add(mgs[i]);
                } else if (mg.inferred) {
                    systems[^1].Add(mgs[i]);
                } else {
                    Debug.LogWarning($"Chronological sorting has failed, does \"{mg.displayName}\" ({mg.name}) have an asset bundle assigned to it?");
                }
            }
            int j = fxActive.Count + 1;
            foreach (var system in systems)
            {
                system.OrderBy(mg => mg.name);
                for (int i = 0; i < system.Count; i++)
                {
                    system[i].SetSiblingIndex(j);
                    j++;
                }
            }
        }

        public void Search()
        {
            for (int i = 0; i < mgsActive.Count; i++)
            {
                mgsActive[i].gameObject.SetActive(
                    System.Text.RegularExpressions.Regex.IsMatch(
                        EventCaller.instance.GetMinigame(mgsActive[i].name).displayName,
                        SearchBar.text,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    )
                );
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
            
            if (Timeline.instance.MouseInTimeline && dragTimes < 1)
            {
                Timeline.instance.timelineState.SetState(Timeline.CurrentTimelineState.State.Selection);
                dragTimes++;

                TimelineEventObj eventObj;

                if (currentEventIndex == 0)
                {
                    if (EventCaller.FXOnlyGames().Contains(SelectedMinigame))
                    {
                        int index = currentEventIndex + 1;
                        if (currentEventIndex - 1 > SelectedMinigame.actions.Count)
                        {
                            index = currentEventIndex;
                        }
                        else if (currentEventIndex - 1 < 0)
                        {
                            if (SelectedMinigame.actions[0].actionName == "switchGame")
                                index = 1;
                            else
                                index = 0;
                        }

                        eventObj = Timeline.instance.AddEventObject(SelectedMinigame.name + "/" + SelectedMinigame.actions[index].actionName, true, new Vector3(0, 0), null, true);
                    }
                    else
                        eventObj = Timeline.instance.AddEventObject($"gameManager/switchGame/{SelectedMinigame.name}", true, new Vector3(0, 0), null, true);
                }
                else
                {
                    int index = currentEventIndex - 1;
                    if (SelectedMinigame.actions[0].actionName == "switchGame")
                    {
                        index = currentEventIndex + 1;
                    }
                    else if (EventCaller.FXOnlyGames().Contains(SelectedMinigame) && SelectedMinigame.actions[0].actionName != "switchGame")
                    {
                        index = currentEventIndex;
                    }

                    eventObj = Timeline.instance.AddEventObject(SelectedMinigame.name + "/" + SelectedMinigame.actions[index].actionName, true, new Vector3(0, 0), null, true);
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