using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;
using Starpelly;

namespace HeavenStudio.Editor.Track
{
    public class Timeline : MonoBehaviour
    {
        [Header("Song Positions")]
        [SerializeField] private TMP_Text SongBeat;
        [SerializeField] private TMP_Text SongPos;
        [SerializeField] private TMP_Text CurrentTempo;

        [Header("Timeline Properties")]
        private float lastBeatPos = 0;
        private Vector2 lastMousePos;
        public List<TimelineEventObj> eventObjs = new List<TimelineEventObj>();
        private bool lastFrameDrag;
        public int LayerCount = 4;
        public bool metronomeEnabled;
        public bool resizable;
        private bool movingPlayback;
        public CurrentTimelineState timelineState = new CurrentTimelineState();
        public float snapInterval = 0.25f; // 4/4

        public static float SnapInterval() { return instance.snapInterval; }

        public class CurrentTimelineState
        {
            public bool selected;
            public bool tempoChange;
            public bool musicVolume;

            public void SetState(bool selected, bool tempoChange, bool musicVolume)
            {
                if (Conductor.instance.NotStopped()) return;

                this.selected = selected;
                this.tempoChange = tempoChange;
                this.musicVolume = musicVolume;

                if (selected)
                    instance.SelectionsBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                else
                    instance.SelectionsBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                if (tempoChange)
                    instance.TempoChangeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                else
                    instance.TempoChangeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                if (musicVolume)
                    instance.MusicVolumeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                else
                    instance.MusicVolumeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;

            }
        }

        [Header("Timeline Components")]
        [SerializeField] private RectTransform TimelineSlider;
        [SerializeField] private RectTransform TimelineGridSelect;
        [SerializeField] private TMP_Text TimelinePlaybackBeat;
        public RectTransform TimelineContent;
        [SerializeField] private RectTransform TimelineSongPosLineRef;
        [SerializeField] private RectTransform TimelineEventObjRef;
        [SerializeField] private RectTransform LayersRect;
        public TempoTimeline TempoInfo;
        private RectTransform TimelineSongPosLine;

        [Header("Timeline Playbar")]
        public Button PlayBTN;
        public Button PauseBTN;
        public Button StopBTN;
        public Button MetronomeBTN;
        public Button AutoplayBTN;
        public Button SelectionsBTN;
        public Button TempoChangeBTN;
        public Button MusicVolumeBTN;
        public Slider PlaybackSpeed;

        public static Timeline instance { get; private set; }

        public bool userIsEditingInputField
        {
            get
            {
                var selectedGO = EventSystem.current.currentSelectedGameObject;
                return selectedGO != null && (selectedGO.GetComponent<InputField>() != null || selectedGO.GetComponent<TMP_InputField>() != null);
            }
        }

        #region Initializers

        public void LoadRemix()
        {
            for (int i = 0; i < eventObjs.Count; i++)
            {
                Destroy(eventObjs[i].gameObject);
            }
            eventObjs.Clear();

            for (int i = 0; i < GameManager.instance.Beatmap.entities.Count; i++)
            {
                var entity = GameManager.instance.Beatmap.entities[i];
                var e = GameManager.instance.Beatmap.entities[i];

                AddEventObject(e.datamodel, false, new Vector3(e.beat, -e.track * LayerHeight()), e, false, RandomID());
            }
        }

        public void Init()
        {
            instance = this;

            LoadRemix();

            TimelineSlider.GetChild(0).GetComponent<Image>().color = EditorTheme.theme.properties.BeatMarkerCol.Hex2RGB();
            TimelineSlider.GetChild(1).GetComponent<Image>().color = EditorTheme.theme.properties.BeatMarkerCol.Hex2RGB();
            TimelineSlider.GetChild(2).GetComponent<TMP_Text>().color = EditorTheme.theme.properties.BeatMarkerCol.Hex2RGB();
            TimelineSlider.GetChild(3).GetComponent<TMP_Text>().color = EditorTheme.theme.properties.BeatMarkerCol.Hex2RGB();
            TimelineSongPosLineRef.GetComponent<Image>().color = EditorTheme.theme.properties.CurrentTimeMarkerCol.Hex2RGB();

            PlayBTN.onClick.AddListener(delegate 
            {
                if (Conductor.instance.isPaused)
                    PlayCheck(false);
                else
                    PlayCheck(true); 
            });
            PauseBTN.onClick.AddListener(delegate 
            {
                if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
                PlayCheck(false); 
            });
            StopBTN.onClick.AddListener(delegate 
            {
                if (Conductor.instance.isPlaying || Conductor.instance.isPaused)
                PlayCheck(true); 
            });

            MetronomeBTN.onClick.AddListener(delegate 
            {
                MetronomeToggle();
            });
            AutoplayBTN.onClick.AddListener(delegate
            {
                AutoPlayToggle();
            });

            SelectionsBTN.onClick.AddListener(delegate
            {
                timelineState.SetState(true, false, false);
            });
            TempoChangeBTN.onClick.AddListener(delegate
            {
                timelineState.SetState(false, true, false);
            });
            MusicVolumeBTN.onClick.AddListener(delegate
            {
                timelineState.SetState(false, false, true);
            });

            Tooltip.AddTooltip(SongBeat.gameObject, "Current Beat");
            Tooltip.AddTooltip(SongPos.gameObject, "Current Time");
            Tooltip.AddTooltip(CurrentTempo.gameObject, "Current Tempo (BPM)");

            Tooltip.AddTooltip(PlayBTN.gameObject, "Play <color=#adadad>[Space]</color>");
            Tooltip.AddTooltip(PauseBTN.gameObject, "Pause <color=#adadad>[Shift + Space]</color>");
            Tooltip.AddTooltip(StopBTN.gameObject, "Stop <color=#adadad>[Space]</color>");

            Tooltip.AddTooltip(MetronomeBTN.gameObject, "Metronome <color=#adadad>[M]</color>");
            Tooltip.AddTooltip(AutoplayBTN.gameObject, "Autoplay <color=#adadad>[P]</color>");

            Tooltip.AddTooltip(SelectionsBTN.gameObject, "Tool: Selection <color=#adadad>[1]</color>");
            Tooltip.AddTooltip(TempoChangeBTN.gameObject, "Tool: Tempo Change <color=#adadad>[2]</color>");
            Tooltip.AddTooltip(MusicVolumeBTN.gameObject, "Tool: Music Volume <color=#adadad>[3]</color>");

            Tooltip.AddTooltip(PlaybackSpeed.gameObject, "The preview's playback speed. Right click to reset to 1.0");

            SetTimeButtonColors(true, false, false);
            MetronomeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;

            timelineState.SetState(true, false, false);

            AutoBtnUpdate();
        }

        public void FitToSong()
        {
            var currentSizeDelta = TimelineContent.sizeDelta;
            float songBeats = Conductor.instance.SongLengthInBeats();
            if (songBeats == 0) songBeats = 320;
            else songBeats += 10;
            TimelineContent.sizeDelta = new Vector2(songBeats, currentSizeDelta.y);
        }

        public void AutoBtnUpdate()
        {
            var animName = GameManager.instance.autoplay ? "Idle" : "Disabled";
            AutoplayBTN.GetComponent<Animator>().Play(animName, 0, 0);
        }

        public void AutoPlayToggle()
        {
            if (!GameManager.instance.autoplay)
            {
                AutoplayBTN.GetComponent<Animator>().Play("Idle", 0, 0);
                GameManager.instance.autoplay = true;
            }
            else
            {
                AutoplayBTN.GetComponent<Animator>().Play("Disabled", 0, 0);
                GameManager.instance.autoplay = false;
            }
        }


        public void MetronomeToggle()
        {
            if (!Conductor.instance.metronome)
            {
                MetronomeBTN.transform.GetChild(0).GetComponent<Image>().color = "009FC6".Hex2RGB();
                Conductor.instance.metronome = true;
            }
            else
            {
                MetronomeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                Conductor.instance.metronome = false;
            }
        }

        public static string RandomID()
        {
            return Starpelly.Random.Strings.RandomString(Starpelly.Enums.Strings.StringType.Alphanumeric, 128);
        }

        #endregion

        private void Update()
        {
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                SongBeat.text = $"Beat {string.Format("{0:0.000}", TimelineSlider.localPosition.x)}";
                SongPos.text = FormatTime(Conductor.instance.GetSongPosFromBeat(TimelineSlider.localPosition.x));
            }
            else
            {
                SongBeat.text = $"Beat {string.Format("{0:0.000}", Conductor.instance.songPositionInBeats)}";
                SongPos.text = FormatTime(Conductor.instance.songPosition);
            }

            SliderControl();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    PlayCheck(false);
                }
                else
                {
                    PlayCheck(true);
                }
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                AutoPlayToggle();
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                MetronomeToggle();
            }


            if (Input.GetMouseButton(1) && !Conductor.instance.isPlaying && Editor.MouseInRectTransform(TimelineGridSelect))
            {
                movingPlayback = true;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                movingPlayback = false;
            }

            if (movingPlayback)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(TimelineContent, Input.mousePosition, Editor.instance.EditorCamera, out lastMousePos);
                TimelineSlider.localPosition = new Vector3(Mathf.Clamp(Mathp.Round2Nearest(lastMousePos.x + 0.12f, Timeline.SnapInterval()), 0, Mathf.Infinity), TimelineSlider.transform.localPosition.y);

                if (TimelineSlider.localPosition.x != lastBeatPos)
                    Conductor.instance.SetBeat(TimelineSlider.transform.localPosition.x);

                lastBeatPos = TimelineSlider.localPosition.x;
            }

            float moveSpeed = 750;
            if (Input.GetKey(KeyCode.LeftShift)) moveSpeed *= 2;

            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                TimelineContent.transform.localPosition += new Vector3(moveSpeed * Time.deltaTime, 0);
            }
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                TimelineContent.transform.localPosition += new Vector3(-moveSpeed * Time.deltaTime, 0);
            }

            if (Conductor.instance.isPlaying)
                TimelineContent.transform.localPosition = new Vector3((-Conductor.instance.songPositionInBeats * 100) + 200, TimelineContent.transform.localPosition.y);

            TimelineContent.transform.localPosition = new Vector3(Mathf.Clamp(TimelineContent.transform.localPosition.x, Mathf.NegativeInfinity, 0), TimelineContent.transform.localPosition.y);

            CurrentTempo.text = $"            = {Conductor.instance.songBpm}";

            if (Input.GetKeyDown(KeyCode.Alpha1) && !userIsEditingInputField)
            {
                timelineState.SetState(true, false, false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && !userIsEditingInputField)
            {
                timelineState.SetState(false, true, false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) && !userIsEditingInputField)
            {
                timelineState.SetState(false, false, true);
            }
        }

        private void SliderControl()
        {
            TimelinePlaybackBeat.text = $"Beat {string.Format("{0:0.000}", TimelineSlider.localPosition.x)}";

            if (TimelineSongPosLine != null)
            {
                TimelineSongPosLine.transform.localPosition = new Vector3(Conductor.instance.songPositionInBeats, TimelineSongPosLine.transform.localPosition.y);
                TimelineSongPosLine.transform.localScale = new Vector3(1f / TimelineContent.transform.localScale.x, TimelineSongPosLine.transform.localScale.y, 1);
            }
        }

        #region PlayChecks
        public void PlayCheck(bool fromStart)
        {
            if (fromStart)
            {
                if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused)
                {
                    Play(false, TimelineSlider.transform.localPosition.x);
                }
                else
                {
                    Stop(TimelineSlider.transform.localPosition.x);
                }
                    
            }
            else
            {
                if (!Conductor.instance.isPlaying)
                {
                    if (TimelineSongPosLine == null)
                    {
                        Play(false, TimelineSlider.transform.localPosition.x);
                    }
                    else
                    {
                        Play(false, TimelineSongPosLine.transform.localPosition.x);
                    }
                }
                else if (!Conductor.instance.isPaused)
                {
                    Pause();
                }
            }
        }

        public void Play(bool fromStart, float time)
        {
            // if (fromStart) Stop();

            if (!Conductor.instance.isPaused)
            {
                TimelineSongPosLine = Instantiate(TimelineSongPosLineRef, TimelineSongPosLineRef.parent).GetComponent<RectTransform>();
                TimelineSongPosLine.gameObject.SetActive(true);
            }

            GameManager.instance.Play(time);

            SetTimeButtonColors(false, true, true);
        }

        public void Pause()
        {
            // isPaused = true;
            GameManager.instance.Pause();

            SetTimeButtonColors(true, false, true);
        }

        public void Stop(float time)
        {
            // isPaused = true;
            // timelineSlider.value = 0;

            if (TimelineSongPosLine != null)
            Destroy(TimelineSongPosLine.gameObject);

            GameManager.instance.Stop(time);

            SetTimeButtonColors(true, false, false);
        }

        public void SetTimeButtonColors(bool playEnabled, bool pauseEnabled, bool stopEnabled)
        {
            if (playEnabled)
            {
                PlayBTN.transform.GetChild(0).GetComponent<Image>().color = Color.green;
                PlayBTN.enabled = true;
            }
            else
            {
                PlayBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                PlayBTN.enabled = false;
            }

            if (pauseEnabled)
            {
                PauseBTN.enabled = true;
                PauseBTN.transform.GetChild(0).GetComponent<Image>().color = Color.blue;
            }
            else
            {   PauseBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                PauseBTN.enabled = false;
            }
            
            if (stopEnabled)
            {
                StopBTN.enabled = true;
                StopBTN.transform.GetChild(0).GetComponent<Image>().color = Color.red;
            }
            else
            {
                StopBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                StopBTN.enabled = false;
            }
        }
        #endregion


        #region Extras
        private string FormatTime(float time)
        {
            int minutes = (int)time / 60;
            int seconds = (int)time - 60 * minutes;
            int milliseconds = (int)(1000 * (time - minutes * 60 - seconds));
            return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }

        public bool CheckIfMouseInTimeline()
        {
            return (this.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(TimelineContent.transform.parent.gameObject.GetComponent<RectTransform>(), Input.mousePosition, Editor.instance.EditorCamera));
        }
        #endregion

        #region Functions

        public TimelineEventObj AddEventObject(string eventName, bool dragNDrop = false, Vector3 pos = new Vector3(), Beatmap.Entity entity = null, bool addEvent = false, string eventId = "")
        {
            GameObject g = Instantiate(TimelineEventObjRef.gameObject, TimelineEventObjRef.parent);
            g.transform.localPosition = pos;
            g.transform.GetChild(3).GetComponent<TMP_Text>().text = eventName.Split('/')[1];

            TimelineEventObj eventObj = g.GetComponent<TimelineEventObj>();

            if (eventName.Split(1) == "switchGame")
            eventObj.Icon.sprite = Editor.GameIcon(eventName.Split(2));
                else
            eventObj.Icon.sprite = Editor.GameIcon(eventName.Split(0));

            Minigames.GameAction gameAction = EventCaller.instance.GetGameAction(EventCaller.instance.GetMinigame(eventName.Split(0)), eventName.Split(1));

            if (gameAction != null)
            {
                if (gameAction.resizable == false)
                {
                    g.GetComponent<RectTransform>().sizeDelta = new Vector2(gameAction.defaultLength, LayerHeight());
                    float length = gameAction.defaultLength;
                    eventObj.length = length;
                }
                else
                {
                    eventObj.resizable = true;
                    if (entity != null && gameAction.defaultLength != entity.length && dragNDrop == false)
                    {
                        g.GetComponent<RectTransform>().sizeDelta = new Vector2(entity.length, LayerHeight());
                    }
                    else
                    {
                        g.GetComponent<RectTransform>().sizeDelta = new Vector2(gameAction.defaultLength, LayerHeight());
                    }
                }
            }

            g.SetActive(true);

            if (dragNDrop)
            {
                var mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);
                g.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

                Selections.instance.ClickSelect(eventObj);
                eventObj.moving = true;
            }
            else
            {
                entity.eventObj = g.GetComponent<TimelineEventObj>();
                entity.track = (int)(g.transform.localPosition.y / LayerHeight() * -1);
            }


            if (addEvent)
            {
                Beatmap.Entity tempEntity = entity;

                if (entity == null)
                {
                    Beatmap.Entity en = new Beatmap.Entity();
                    en.datamodel = eventName;
                    en.eventObj = eventObj;

                    GameManager.instance.Beatmap.entities.Add(en);
                    GameManager.instance.SortEventsList();

                    tempEntity = en;
                }
                else
                {
                    GameManager.instance.Beatmap.entities.Add(entity);
                    GameManager.instance.SortEventsList();
                }

                // default param value
                var game = EventCaller.instance.GetMinigame(eventName.Split(0));
                var ep = EventCaller.instance.GetGameAction(game, eventName.Split(1)).parameters;

                if (ep != null)
                {
                    for (int i = 0; i < ep.Count; i++)
                    {
                        object returnVal = ep[i].parameter;

                        var propertyType = returnVal.GetType();
                        if (propertyType == typeof(EntityTypes.Integer))
                        {
                            returnVal = ((EntityTypes.Integer)ep[i].parameter).val;
                        }
                        else if (propertyType == typeof(EntityTypes.Float))
                        {
                            returnVal = ((EntityTypes.Float)ep[i].parameter).val;
                        }

                        tempEntity[ep[i].propertyName] = returnVal;
                    }
                }
            }

            eventObjs.Add(eventObj);

            eventObj.eventObjID = eventId;

            return eventObj;
        }

        public void DestroyEventObject(Beatmap.Entity entity)
        {
            if (EventParameterManager.instance.entity == entity)
                EventParameterManager.instance.Disable();

            eventObjs.Remove(entity.eventObj);
            GameManager.instance.Beatmap.entities.Remove(entity);
            Timeline.instance.eventObjs.Remove(entity.eventObj);

            Destroy(entity.eventObj.gameObject);
            GameManager.instance.SortEventsList();
        }

        public bool IsMouseAboveEvents()
        {
            return Timeline.instance.eventObjs.FindAll(c => c.mouseHovering == true).Count > 0;
        }

        public bool InteractingWithEvents()
        {
            return eventObjs.FindAll(c => c.moving == true).Count > 0 || eventObjs.FindAll(c => c.resizing == true).Count > 0;
        }

        public float SnapToLayer(float y)
        {
            float size = LayerHeight();
            return Mathf.Clamp(Mathp.Round2Nearest(y, size), -size * 3, 0);
        }

        public float LayerHeight()
        {
            return LayersRect.rect.height / 4;
        }

        public void SetPlaybackSpeed(float speed)
        {
            float spd = Mathp.Round2Nearest(speed, Timeline.SnapInterval());
            PlaybackSpeed.transform.GetChild(3).GetComponent<TMP_Text>().text = $"Playback Speed: {spd}x";
            Conductor.instance.musicSource.pitch = spd;
            PlaybackSpeed.value = spd;
        }

        public void ResetPlaybackSpeed()
        {
            if (Input.GetMouseButton(1))
            {
                PlaybackSpeed.transform.GetChild(3).GetComponent<TMP_Text>().text = $"Playback Speed: 1x";
                PlaybackSpeed.value = 1f;
            }
        }

        #endregion

        #region Commands

        public void Move()
        { 
        }

        public void Undo()
        {

        }

        #endregion
    }
}