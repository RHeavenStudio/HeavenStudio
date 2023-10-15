using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;
using Starpelly;
using Jukebox;
using Newtonsoft.Json;
using System.Linq;

namespace HeavenStudio.Editor.Track
{
    public class Timeline : MonoBehaviour
    {
        [Header("Song Positions")]
        [SerializeField] private TMP_Text SongBeat;
        [SerializeField] private TMP_Text SongPos;
        [SerializeField] private TMP_Text CurrentTempo;

        [Header("Timeline Properties")]
        private bool lastFrameDrag;
        public int LayerCount = 5;
        public bool metronomeEnabled;
        public bool resizable;
        private bool movingPlayback;
        public CurrentTimelineState timelineState = new CurrentTimelineState();
        public float snapInterval = 0.25f; // 4/4

        [Header("Components")]
        [SerializeField] private RawImage waveform;
        [SerializeField] private Image layerBG;
        [SerializeField] private TimelineZoom zoomComponent;
        public Texture2D resizeCursor;

        public float WidthPerBeat { get; private set; } = 100.0f;
        public float PixelsPerBeat => WidthPerBeat * Zoom;
        public float Zoom { get; private set; } = 1.0f;
        public float MousePos2Beat { get; private set; }
        public float MousePos2Layer { get; private set; }
        public float MousePos2BeatSnap => Mathp.Round2Nearest(MousePos2Beat + (SnapInterval() * 0.5f), SnapInterval());
        public bool MouseInTimeline { get; private set; }

        private Vector2 relativeMousePos;
        public Vector2 RelativeMousePos => relativeMousePos;
        public float PlaybackBeat = 0.0f;

        public static float SnapInterval() { return instance.snapInterval; }

        public void SetSnap(float snap) { snapInterval = snap; }

        public class CurrentTimelineState
        {
            public enum State
            {
                Selection,
                TempoChange,
                MusicVolume,
                ChartSection
            }

            public State currentState = State.Selection;

            public bool selected { get { return currentState == State.Selection; } }
            public bool tempoChange { get { return currentState == State.TempoChange; } }
            public bool musicVolume { get { return currentState == State.MusicVolume; } }
            public bool chartSection { get { return currentState == State.ChartSection; } }

            public void SetState(bool selected, bool tempoChange, bool musicVolume)
            {
                if (Conductor.instance.NotStopped()) return;

                if (selected)
                {
                    currentState = State.Selection;
                    instance.SelectionsBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                    instance.SelectionsBTN.GetComponent<TabButton>().Invoke("OnClick", 0);
                }
                else
                    instance.SelectionsBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                if (tempoChange)
                {
                    currentState = State.TempoChange;
                    instance.TempoChangeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                    instance.TempoChangeBTN.GetComponent<TabButton>().Invoke("OnClick", 0);
                }
                else
                    instance.TempoChangeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                if (musicVolume)
                {
                    currentState = State.MusicVolume;
                    instance.MusicVolumeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                    instance.MusicVolumeBTN.GetComponent<TabButton>().Invoke("OnClick", 0);
                }
                else
                    instance.MusicVolumeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;

            }

            public void SetState(State state)
            {
                if (Conductor.instance.NotStopped()) return;

                currentState = state;
                if (selected)
                {
                    instance.SelectionsBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                    instance.SelectionsBTN.GetComponent<TabButton>().Invoke("OnClick", 0);
                }
                else
                    instance.SelectionsBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                if (tempoChange)
                {
                    instance.TempoChangeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                    instance.TempoChangeBTN.GetComponent<TabButton>().Invoke("OnClick", 0);
                }
                else
                    instance.TempoChangeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                if (musicVolume)
                {
                    instance.MusicVolumeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                    instance.MusicVolumeBTN.GetComponent<TabButton>().Invoke("OnClick", 0);
                }
                else
                    instance.MusicVolumeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                if (chartSection)
                {
                    instance.ChartSectionBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                    instance.ChartSectionBTN.GetComponent<TabButton>().Invoke("OnClick", 0);
                }
                else
                    instance.ChartSectionBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
            }
        }

        [Header("Panels")]
        [SerializeField] private RectTransform TopPanel;
        [SerializeField] private RectTransform TopPanelContent;
        [SerializeField] private RectTransform ContentPanel;
        [SerializeField] private RectTransform OverlayPanel, OverlayPanelContent;

        [Header("Timeline Components")]
        [SerializeField] private RectTransform TimelineSlider;
        [SerializeField] private RectTransform TimelineGridSelect;
        [SerializeField] private RectTransform TimelineEventGrid;
        [SerializeField] private TMP_Text TimelinePlaybackBeat;
        public ScrollRect TimelineScroll;
        [SerializeField] private RectTransform RealTimelineContent;
        [SerializeField] private RectTransform TimelineContent;
        public RectTransform EventContent;
        [SerializeField] private RectTransform TimelineSongPosLineRef;
        [SerializeField] private RectTransform TimelineEventObjRef;
        public RectTransform TimelineEventsHolder;
        [SerializeField] private RectTransform LayersRect;

        [Header("Timeline Inputs")]
        public TMP_InputField FirstBeatOffset;
        public TMP_InputField StartingTempoSpecialAll;
        public TMP_InputField StartingTempoSpecialTempo;
        public TMP_InputField StartingVolumeSpecialVolume;

        public SpecialTimeline SpecialInfo;
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
        public Button ChartSectionBTN;
        public Button ZoomInBTN;
        public Button ZoomOutBTN;
        public Button ZoomResetBTN;
        public Button WaveformBTN;
        public Slider PlaybackSpeed;

        public Vector3[] LayerCorners = new Vector3[4];

        // Beats
        public float leftSide => (RealTimelineContent.anchoredPosition.x / PixelsPerBeat) * -1;
        public float rightSide => (TimelineScroll.viewport.rect.width / PixelsPerBeat) + leftSide;

        // Layer Height
        public float topSide => RealTimelineContent.anchoredPosition.y / LayerHeight();
        public float bottomSide => topSide + (TimelineScroll.viewport.rect.height / LayerHeight());

        private Vector2 lastScreenSize;

        public static Timeline instance { get; private set; }

        [HideInInspector]
        public List<RiqEntity> CopiedEntities = new();

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
            /*
            for (int i = 0; i < GameManager.instance.Beatmap.Entities.Count; i++)
            {
                var e = GameManager.instance.Beatmap.Entities[i];

                AddEventObject(e.datamodel, false, new Vector3((float)e.beat, -e["track"] * LayerHeight()), e, false);
            }
            */

            TimelineBlockManager.Instance.Load();

            SpecialInfo.Setup();
            UpdateOffsetText();
            UpdateStartingBPMText();
            UpdateStartingVolText();

            GameManager.instance.SortEventsList();
        }

        public void Init()
        {
            instance = this;

            for (var i = 0; i < LayerCount; i++)
            {
                var bg = Instantiate(layerBG.gameObject, layerBG.rectTransform.parent).GetComponent<Image>();
                if (i % 2 == 0) bg.enabled = false;
            }
            layerBG.gameObject.SetActive(false);

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
                timelineState.SetState(CurrentTimelineState.State.Selection);
            });
            TempoChangeBTN.onClick.AddListener(delegate
            {
                timelineState.SetState(CurrentTimelineState.State.TempoChange);
            });
            MusicVolumeBTN.onClick.AddListener(delegate
            {
                timelineState.SetState(CurrentTimelineState.State.MusicVolume);
            });
            ChartSectionBTN.onClick.AddListener(delegate
            {
                timelineState.SetState(CurrentTimelineState.State.ChartSection);
            });

            ZoomInBTN.onClick.AddListener(delegate
            {
                zoomComponent.Zoom(0.25f * zoomComponent.Scale.x, Vector2.zero);
            });
            ZoomOutBTN.onClick.AddListener(delegate
            {
                zoomComponent.Zoom(-0.2f * zoomComponent.Scale.x, Vector2.zero);
            });
            ZoomResetBTN.onClick.AddListener(delegate
            {
                zoomComponent.ResetZoom();
            });
            WaveformBTN.onClick.AddListener(delegate
            {
                WaveformToggle();
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
            Tooltip.AddTooltip(ChartSectionBTN.gameObject, "Tool: Beatmap Sections <color=#adadad>[4]</color>");

            Tooltip.AddTooltip(StartingTempoSpecialAll.gameObject, "Starting Tempo (BPM)");
            Tooltip.AddTooltip(StartingTempoSpecialTempo.gameObject, "Starting Tempo (BPM)");
            Tooltip.AddTooltip(StartingVolumeSpecialVolume.gameObject, "Starting Volume (%)");

            Tooltip.AddTooltip(ZoomInBTN.gameObject, "Zoom In");
            Tooltip.AddTooltip(ZoomOutBTN.gameObject, "Zoom Out");
            Tooltip.AddTooltip(ZoomResetBTN.gameObject, "Zoom Reset");
            Tooltip.AddTooltip(WaveformBTN.gameObject, "Waveform Toggle");

            Tooltip.AddTooltip(PlaybackSpeed.gameObject, "The preview's playback speed. Right click to reset to 1.0");

            SetTimeButtonColors(true, false, false);
            MetronomeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
            MetronomeBTN.transform.GetChild(1).GetComponent<Image>().color = Color.gray;
            MetronomeBTN.transform.GetChild(2).GetComponent<Image>().color = Color.gray;

            timelineState.SetState(CurrentTimelineState.State.Selection);

            AutoBtnUpdate();

            resizeCursor = Resources.Load<Texture2D>("Cursors/horizontal_resize");
        }

        public void FitToSong()
        {
            var currentSizeDelta = RealTimelineContent.sizeDelta;
            float songBeats = Conductor.instance.SongLengthInBeats();
            if (songBeats == 0) songBeats = 320;
            else songBeats += 10;

            TimelineContent.sizeDelta = new Vector2(songBeats * PixelsPerBeat, currentSizeDelta.y);
            // TimelineEventGrid.sizeDelta = new Vector2(songBeats * PixelsPerBeat, currentSizeDelta.y);
            RealTimelineContent.sizeDelta = new Vector2(TimelineContent.sizeDelta.x / Zoom, RealTimelineContent.sizeDelta.y);
        }

        public void CreateWaveform()
        {
            Debug.Log("what");
            // DrawWaveform();
            // StartCoroutine(DrawWaveformRealtime());
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
                MetronomeBTN.transform.GetChild(1).GetComponent<Image>().color = "009FC6".Hex2RGB();
                MetronomeBTN.transform.GetChild(2).GetComponent<Image>().color = "009FC6".Hex2RGB();
                Conductor.instance.metronome = true;
            }
            else
            {
                MetronomeBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                MetronomeBTN.transform.GetChild(1).GetComponent<Image>().color = Color.gray;
                MetronomeBTN.transform.GetChild(2).GetComponent<Image>().color = Color.gray;
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
            RectTransformUtility.ScreenPointToLocalPointInRectangle(EventContent, Input.mousePosition, Editor.instance.EditorCamera, out relativeMousePos);

            MouseInTimeline = this.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(EventContent, Input.mousePosition, Editor.instance.EditorCamera);


            /*
            if (MouseInTimeline)
            {
                var wheel = Input.mouseScrollDelta.y;

                if (wheel != 0)
                {
                    var incre = 0.0f;
                    if (wheel > 0)
                    {
                        incre += wheel * (Zoom * 0.25f);
                    }
                    else
                    {
                        incre += wheel * (Zoom * 0.2f);
                    }

                    var v = new Vector3[4];
                    TimelineScroll.viewport.GetWorldCorners(v);
                    var viewportPos = Editor.instance.EditorCamera.WorldToScreenPoint(v[0]);

                    var left = leftSide;
                    var viewportWidth = TimelineScroll.viewport.rect.width;
                    var localPointRec = Mathf.Lerp(left, left + (viewportWidth / Zoom),
                        Mathp.Normalize(Input.mousePosition.x, viewportPos.x, viewportPos.x + viewportWidth));

                    var xPixels = (localPointRec * incre);
                    xPixels *= (Zoom);

                    TimelineContent.anchoredPosition = new Vector2(
                        TimelineContent.anchoredPosition.x - xPixels,
                        TimelineContent.anchoredPosition.y
                        );

                    Zoom += incre;
                    Zoom = Mathf.Clamp(Zoom, 1, 1000);
                }
            }
            */

            MousePos2Beat = relativeMousePos.x / PixelsPerBeat;
            MousePos2Layer = Mathf.Clamp(Mathf.FloorToInt(-(relativeMousePos.y) / LayerHeight()), 0, LayerCount - 1);

            Conductor cond = Conductor.instance;
            // waveform.rectTransform.anchoredPosition = new Vector2(
            //     -(GameManager.instance.Beatmap.data.offset / (60.0f / GameManager.instance.Beatmap.bpm)), 
            //     waveform.rectTransform.anchoredPosition.y);

            // WaveformBTN.transform.GetChild(0).GetComponent<Image>().color = (cond.musicSource.clip != null && waveform.gameObject.activeInHierarchy) ? Color.white : Color.gray;

            if (!cond.isPlaying && !cond.isPaused)
            {
                SongBeat.text = $"Beat {string.Format("{0:0.000}", PlaybackBeat)}";
                SongPos.text = FormatTime(cond.GetSongPosFromBeat(PlaybackBeat));
            }
            else
            {
                SongBeat.text = $"Beat {string.Format("{0:0.000}", cond.songPositionInBeats)}";
                SongPos.text = FormatTime(cond.songPositionAsDouble);
            }

            // Metronome animation
            {
                var rectTransform = MetronomeBTN.transform.GetChild(1).GetComponent<RectTransform>();
                var rot = 0.0f;
                if (Conductor.instance.metronome)
                {
                    var startBeat = Mathf.FloorToInt(Conductor.instance.songPositionInBeats - 0.5f);
                    var nm = Conductor.instance.GetLoopPositionFromBeat(0.5f, 1f);
                    var loop = (startBeat % 2 == 0) ? Mathf.SmoothStep(-1.1f, 1f, nm) : Mathf.SmoothStep(1f, -1f, nm);

                    rot = loop * 45f;
                }
                else
                {
                    rot = Mathf.LerpAngle(rectTransform.localEulerAngles.z, 0.0f, Time.deltaTime * 16);
                }

                rectTransform.localEulerAngles =
                    new Vector3(rectTransform.localEulerAngles.x, rectTransform.localEulerAngles.y,
                    rot);
            }


            SliderControl();

            #region Keyboard Shortcuts
            if ((!userIsEditingInputField) && Editor.instance.isShortcutsEnabled)
            {

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

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    timelineState.SetState(CurrentTimelineState.State.Selection);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    timelineState.SetState(CurrentTimelineState.State.TempoChange);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    timelineState.SetState(CurrentTimelineState.State.MusicVolume);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    timelineState.SetState(CurrentTimelineState.State.ChartSection);
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    PlaybackFocus(false);
                }

                float moveSpeed = 750;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) moveSpeed *= 6;

                if (Input.GetKey(KeyCode.LeftArrow) || (!Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.A)))
                {
                    RealTimelineContent.transform.localPosition += new Vector3(moveSpeed * Time.deltaTime, 0);
                }
                else if (Input.GetKey(KeyCode.RightArrow) || (!Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.D)))
                {
                    RealTimelineContent.transform.localPosition += new Vector3(-moveSpeed * Time.deltaTime, 0);
                }
            }
            #endregion

            if (Input.GetMouseButton(1) && !cond.isPlaying && Editor.MouseInRectTransform(TimelineGridSelect))
            {
                movingPlayback = true;
            }
            else if (Input.GetMouseButtonUp(1) || cond.isPlaying)
            {
                movingPlayback = false;
            }
            if (movingPlayback)
            {
                var playbackBeat = Mathf.Max(MousePos2BeatSnap, 0);
                TimelineSlider.localPosition = new Vector3(playbackBeat * PixelsPerBeat, TimelineSlider.transform.localPosition.y);

                cond.SetBeat(playbackBeat);
                this.PlaybackBeat = playbackBeat;
            }

            if (cond.isPlaying)
                PlaybackFocus(true);

            RealTimelineContent.transform.localPosition = new Vector3(Mathf.Clamp(RealTimelineContent.transform.localPosition.x, Mathf.NegativeInfinity, 0), RealTimelineContent.transform.localPosition.y);

            CurrentTempo.text = $"    = {cond.songBpm}";

            LayersRect.GetWorldCorners(LayerCorners);

            for (var i = 0; i < LayersRect.childCount; i++)
            {
                var layerRect = LayersRect.GetChild(i).GetComponent<RectTransform>();
                layerRect.anchoredPosition = new Vector2(layerRect.anchoredPosition.x, -LayerHeight() * (i - 1));
                layerRect.sizeDelta = new Vector2(layerRect.sizeDelta.x, LayerHeight());
            }

            for (var i = 0; i < layerBG.rectTransform.parent.childCount; i++)
            {
                var layerRect = layerBG.rectTransform.parent.GetChild(i).GetComponent<RectTransform>();
                layerRect.anchoredPosition = new Vector2(layerRect.anchoredPosition.x, -LayerHeight() * (i - 1));
                layerRect.sizeDelta = new Vector2(layerRect.sizeDelta.x, LayerHeight());
            }

            RealTimelineContent.sizeDelta = new Vector2(RealTimelineContent.sizeDelta.x, LayerHeight() * LayerCount);
        }

        public void LateUpdate()
        {
            TimelineBlockManager.Instance.UpdateMarkers();
            BoxSelection.instance.LayerSelectUpdate();

            TimelineContent.anchoredPosition = RealTimelineContent.anchoredPosition;

            TopPanelContent.anchoredPosition = new Vector2(TimelineContent.anchoredPosition.x, TopPanelContent.anchoredPosition.y);
            TopPanelContent.sizeDelta = new Vector2(TimelineContent.sizeDelta.x, TopPanelContent.sizeDelta.y);

            OverlayPanelContent.anchoredPosition = new Vector2(TimelineContent.anchoredPosition.x, OverlayPanelContent.anchoredPosition.y);
            OverlayPanelContent.sizeDelta = new Vector2(TimelineContent.sizeDelta.x, OverlayPanelContent.sizeDelta.y);

            LayersRect.anchoredPosition = new Vector2(LayersRect.anchoredPosition.x, TimelineContent.anchoredPosition.y);
        }

        public static float GetScaleModifier()
        {
            Camera cam = Editor.instance.EditorCamera;
            return Mathf.Pow(cam.pixelWidth / 1280f, 1f) * Mathf.Pow(cam.pixelHeight / 720f, 0f);
        }

        public Vector2 LayerCornersToDist()
        {
            Vector3[] v = LayerCorners;
            return new Vector2(Mathf.Abs(v[1].x - v[2].x), Mathf.Abs(v[3].y - v[1].y));
        }

        private void SliderControl()
        {
            TimelinePlaybackBeat.text = $"Beat {string.Format("{0:0.000}", PlaybackBeat)}";

            if (TimelineSongPosLine != null)
            {
                TimelineSongPosLine.transform.localPosition = new Vector3(Conductor.instance.songPositionInBeats * PixelsPerBeat, TimelineSongPosLine.transform.localPosition.y);
            }
        }

        #region PlayChecks
        public void PlayCheck(bool fromStart)
        {
            if (fromStart)
            {
                if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused)
                {
                    Play(false, PlaybackBeat);
                }
                else
                {
                    Stop(PlaybackBeat);
                }

            }
            else
            {
                if (!Conductor.instance.isPlaying)
                {
                    if (TimelineSongPosLine == null)
                    {
                        Play(false, PlaybackBeat);
                    }
                    else
                    {
                        Play(false, Conductor.instance.songPositionInBeats);
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
                PlayBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
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
                PauseBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            }
            else
            {
                PauseBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                PauseBTN.enabled = false;
            }

            if (stopEnabled)
            {
                StopBTN.enabled = true;
                StopBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            }
            else
            {
                StopBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                StopBTN.enabled = false;
            }
        }
        #endregion


        #region Extras
        private string FormatTime(double time)
        {
            int minutes = (int)time / 60;
            int seconds = (int)time - 60 * minutes;
            int milliseconds = (int)(1000 * (time - minutes * 60 - seconds));
            return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }

        #endregion

        #region Functions

        public void PlaybackFocus(bool lerp)
        {
            var lerpSpd = (lerp) ? 12f : 10000000; // im lazy

            var newPos = new Vector3((-Conductor.instance.songPositionInBeats * PixelsPerBeat) + 200, RealTimelineContent.transform.localPosition.y);
            newPos.x = Mathf.Max(newPos.x, -(TimelineContent.rect.width - TimelineScroll.viewport.rect.width));
            RealTimelineContent.transform.localPosition =
                Vector3.Lerp(RealTimelineContent.transform.localPosition, newPos, Time.deltaTime * lerpSpd);
        }

        public void WaveformToggle()
        {
            if (Conductor.instance.musicSource.clip == null) return;

            // waveform.gameObject.SetActive(!waveform.gameObject.activeInHierarchy);
            waveform.gameObject.SetActive(false);
        }

        public IEnumerator DrawWaveformRealtime()
        {
            var clip = Conductor.instance.musicSource.clip;

            if (!clip)
                yield break;

            waveform.rectTransform.sizeDelta = new Vector2(Conductor.instance.SongLengthInBeats() + 0.15f, waveform.rectTransform.sizeDelta.y);
            waveform.color = Color.white;

            var num = 12000;
            var num2 = 1f;
            var num3 = clip.samples * clip.channels;
            var array = new float[num3];
            var wave = new float[num];
            clip.GetData(array, 0);
            int packsize = num3 / num;
            Debug.Log($"drawing waveform. samples:, {clip.samples}, packsize: {packsize}");
            float num5 = 0f;
            int num6 = 0;
            int num7 = 0;
            for (int i = 0; i < num3; i++)
            {
                wave[num6] += Mathf.Abs(array[i]);
                num7++;
                if (num7 > packsize)
                {
                    if (num5 < wave[num6])
                    {
                        num5 = wave[num6];
                    }
                    num6++;
                    num7 = 0;
                }
            }
            for (int j = 0; j < num; j++)
            {
                wave[j] /= num5 * num2;
                if (wave[j] > 1f)
                {
                    wave[j] = 1f;
                }
            }
            int height = 200;
            Color col = Colors.Hex2RGB("727272");
            col.a = 0.25f;

            Texture2D tex = new Texture2D(wave.Length, height, TextureFormat.RGBA32, false);

            FillColorAlpha(tex, new Color32(0, 0, 0, 0));

            this.waveform.texture = tex;
            int waveI = 0;
            while (waveI < wave.Length)
            {
                int num8 = waveI;
                while (num8 < waveI + 100 && waveI < wave.Length)
                {
                    int num9 = 0;
                    while ((float)num9 <= wave[num8] * (float)height / 2f)
                    {
                        tex.SetPixel(num8, height / 2 + num9, col);
                        tex.SetPixel(num8, height / 2 - num9, col);
                        num9++;
                    }
                    num8++;
                }
                waveI += 100;
                tex.Apply();
                yield return null;
            }
            tex.Apply();
            yield break;
        }

        public Texture2D FillColorAlpha(Texture2D tex2D, Color32? fillColor = null)
        {
            if (fillColor == null)
            {
                fillColor = Color.clear;
            }
            Color32[] fillPixels = new Color32[tex2D.width * tex2D.height];
            for (int i = 0; i < fillPixels.Length; i++)
            {
                fillPixels[i] = (Color32)fillColor;
            }
            tex2D.SetPixels32(fillPixels);
            return tex2D;
        }

        public TimelineEventObj AddEventObject(string eventName, bool dragNDrop = false, Vector3 pos = new Vector3(), RiqEntity entity = null, bool addEvent = false)
        {
            var game = EventCaller.instance.GetMinigame(eventName.Split(0));
            var action = EventCaller.instance.GetGameAction(game, eventName.Split(1));

            var gameAction = EventCaller.instance.GetGameAction(EventCaller.instance.GetMinigame(eventName.Split(0)), eventName.Split(1));

            if (addEvent)
            {
                RiqEntity tempEntity = entity;

                if (entity == null)
                {
                    RiqEntity en = GameManager.instance.Beatmap.AddNewEntity(eventName, 0, gameAction.defaultLength);

                    tempEntity = en;

                    // default param values
                    var ep = action.parameters;

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
                            else if (propertyType.IsEnum)
                            {
                                returnVal = (int)ep[i].parameter;
                            }

                            //tempEntity[ep[i].propertyName] = returnVal;
                            tempEntity.CreateProperty(ep[i].propertyName, returnVal);
                        }
                    }
                }
                else
                {
                    // GameManager.instance.Beatmap.Entities.Add(tempEntity);
                    Debug.LogWarning("Weird as fuck case called?");
                }

                GameManager.instance.SortEventsList();
                TimelineBlockManager.Instance.SetEntityToSet(tempEntity);
            }
            else
            {
                TimelineBlockManager.Instance.SetEntityToSet(entity);
            }

            var marker = TimelineBlockManager.Instance.Pool.Get();

            marker.SetMarkerInfo();

            if (dragNDrop)
            {
                Selections.instance.ClickSelect(marker);
                marker.moving = true;
            }
            else
            {
                entity["track"] = marker.GetTrack();
            }

            CommandManager.Instance.AddCommand(new Commands.Place(marker.entity, marker.entity.guid));

            return marker;
        }

        public void CopySelected()
        {
            CopyEntities(Selections.instance.eventsSelected.Select(c => c.entity).ToList());
        }

        public void CopyEntities(List<RiqEntity> original)
        {
            if (original.Count <= 0) return;

            CopiedEntities.Clear();

            foreach (RiqEntity entity in original)
            {
                CopiedEntities.Add(entity.DeepCopy());
            }
        }

        public void Paste()
        {
            CommandManager.Instance.AddCommand(new Commands.Paste(CopiedEntities));
        }

        /*
        public TimelineEventObj AddEventObject(string eventName, bool dragNDrop = false, Vector3 pos = new Vector3(), RiqEntity entity = null, bool addEvent = false)
        {
            var game = EventCaller.instance.GetMinigame(eventName.Split(0));
            var action = EventCaller.instance.GetGameAction(game, eventName.Split(1));
            GameObject g = Instantiate(TimelineEventObjRef.gameObject, TimelineEventObjRef.parent);
            g.transform.localPosition = pos;

            TimelineEventObj eventObj = g.GetComponent<TimelineEventObj>();
            eventObj.eventLabel.text = action.displayName;

            if (eventName.Split(1) == "switchGame")
                eventObj.Icon.sprite = Editor.GameIcon(eventName.Split(2));
            else
                eventObj.Icon.sprite = Editor.GameIcon(eventName.Split(0));

            Minigames.GameAction gameAction = EventCaller.instance.GetGameAction(EventCaller.instance.GetMinigame(eventName.Split(0)), eventName.Split(1));

            if (gameAction != null)
            {
                if (gameAction.resizable == false)
                {
                    g.GetComponent<RectTransform>().sizeDelta = new Vector2(gameAction.defaultLength * Timeline.instance.PixelsPerBeat, LayerHeight());
                    float length = gameAction.defaultLength;
                    eventObj.length = length;
                }
                else
                {
                    eventObj.resizable = true;
                    if (entity != null && gameAction.defaultLength != entity.length && dragNDrop == false)
                    {
                        g.GetComponent<RectTransform>().sizeDelta = new Vector2(entity.length * Timeline.instance.PixelsPerBeat, LayerHeight());
                    }
                    else
                    {
                        g.GetComponent<RectTransform>().sizeDelta = new Vector2(gameAction.defaultLength * Timeline.instance.PixelsPerBeat, LayerHeight());
                    }
                }
            }

            if (dragNDrop)
            {
                var mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);
                g.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

                Selections.instance.ClickSelect(eventObj);
                eventObj.moving = true;
            }
            else
            {
                entity["track"] = eventObj.GetTrack();
            }

            if (addEvent)
            {
                RiqEntity tempEntity = entity;

                if (entity == null)
                {
                    RiqEntity en = GameManager.instance.Beatmap.AddNewEntity(eventName, g.transform.localPosition.x, gameAction.defaultLength);

                    tempEntity = en;

                    // default param values
                    var ep = action.parameters;

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
                            else if (propertyType.IsEnum)
                            {
                                returnVal = (int) ep[i].parameter;
                            }

                            //tempEntity[ep[i].propertyName] = returnVal;
                            tempEntity.CreateProperty(ep[i].propertyName, returnVal);
                        }
                    }
                }
                else
                {
                    GameManager.instance.Beatmap.Entities.Add(tempEntity);
                }

                GameManager.instance.SortEventsList();
                eventObj.entity = tempEntity;
            }
            else
            {
                eventObj.entity = entity;
            }

            eventObjs.Add(eventObj);
            eventObj.eventObjID = eventObj.entity.uid;

            g.SetActive(true);

            return eventObj;
        }
        */

        private List<TimelineEventObj> duplicatedEventObjs = new List<TimelineEventObj>();
        public TimelineEventObj CopyEventObject(TimelineEventObj e)
        {
            RiqEntity clone = e.entity.DeepCopy();
            TimelineEventObj dup = AddEventObject(clone.datamodel, false, new Vector3((float)clone.beat, -clone["track"] * Timeline.instance.LayerHeight()), clone, true);
            duplicatedEventObjs.Add(dup);

            return dup;
        }

        public float SnapToLayer(float y)
        {
            float size = LayerHeight();
            return Mathf.Clamp(Mathp.Round2Nearest(y, size), -size * (LayerCount - 1), 0f);
        }

        public float LayerHeight()
        {
            var defaultHeight = 32;
            return Mathf.Max(defaultHeight, (LayersRect.rect.height / LayerCount));
            // return LayersRect.rect.height / LayerCount;
        }

        public float LayerToY(int layer)
        {
            return (-layer * LayerHeight());
        }

        const float SpeedSnap = 0.25f;
        public void SetPlaybackSpeed(float speed)
        {
            float spd = Mathp.Round2Nearest(speed, SpeedSnap);
            PlaybackSpeed.transform.GetChild(3).GetComponent<TMP_Text>().text = $"Playback Speed: {spd}x";
            Conductor.instance.SetTimelinePitch(spd);
            PlaybackSpeed.value = spd;
        }

        public void ResetPlaybackSpeed()
        {
            if (Input.GetMouseButton(1))
            {
                PlaybackSpeed.transform.GetChild(3).GetComponent<TMP_Text>().text = $"Playback Speed: 1x";
                PlaybackSpeed.value = 1f;
                Conductor.instance.SetTimelinePitch(PlaybackSpeed.value);
            }
        }

        public void OnZoom(float zoom)
        {
            Zoom = zoom;

            TimelineSlider.localPosition = new Vector3(Conductor.instance.songPositionInBeats * PixelsPerBeat, TimelineSlider.transform.localPosition.y);

            FitToSong();
            TimelineBlockManager.Instance.OnZoom();
        }

        public void UpdateOffsetText()
        {
            // show up to 4 decimal places
            FirstBeatOffset.text = (GameManager.instance.Beatmap.data.offset * 1000f).ToString("F0");
        }

        public void UpdateOffsetFromText()
        {
            // Failsafe against empty string.
            if (String.IsNullOrEmpty(FirstBeatOffset.text))
                FirstBeatOffset.text = "0";

            // Convert ms to s.
            double newOffset = Convert.ToDouble(FirstBeatOffset.text) / 1000f;

            GameManager.instance.Beatmap.data.offset = newOffset;

            UpdateOffsetText();
            FitToSong();
        }

        public void UpdateStartingBPMText()
        {
            StartingTempoSpecialAll.text = GameManager.instance.Beatmap.TempoChanges[0]["tempo"].ToString("G");
            StartingTempoSpecialTempo.text = StartingTempoSpecialAll.text;
        }

        public void UpdateStartingBPMFromText(bool all)
        {
            string text = all ? StartingTempoSpecialAll.text : StartingTempoSpecialTempo.text;
            // Failsafe against empty string.
            if (String.IsNullOrEmpty(text))
                text = "120";

            var newBPM = Convert.ToDouble(text);

            // Failsafe against negative BPM.
            if (newBPM < 1f)
            {
                text = "1";
                newBPM = 1;
            }

            // Limit decimal places to 4.
            newBPM = System.Math.Round(newBPM, 4);

            RiqEntity tempoChange = GameManager.instance.Beatmap.TempoChanges[0];
            tempoChange["tempo"] = (float)newBPM;
            GameManager.instance.Beatmap.TempoChanges[0] = tempoChange;

            // In case the newBPM ended up differing from the inputted string.
            UpdateStartingBPMText();

            Timeline.instance.FitToSong();
        }

        public void UpdateStartingVolText()
        {
            StartingVolumeSpecialVolume.text = (GameManager.instance.Beatmap.VolumeChanges[0]["volume"]).ToString("G");
        }

        public void UpdateStartingVolFromText()
        {
            // Failsafe against empty string.
            if (String.IsNullOrEmpty(StartingVolumeSpecialVolume.text))
                StartingVolumeSpecialVolume.text = "100";

            var newVol = Convert.ToInt32(StartingVolumeSpecialVolume.text);
            newVol = Mathf.Clamp(newVol, 0, 100);

            RiqEntity volChange = GameManager.instance.Beatmap.VolumeChanges[0];
            volChange["volume"] = newVol;
            GameManager.instance.Beatmap.VolumeChanges[0] = volChange;

            UpdateStartingVolText();
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