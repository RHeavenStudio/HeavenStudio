using System;
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
        public int LayerCount = 5;
        public bool metronomeEnabled;
        public bool resizable;
        private bool movingPlayback;
        public CurrentTimelineState timelineState = new CurrentTimelineState();
        public float snapInterval = 0.25f; // 4/4

        [Header("Components")]
        [SerializeField] private RawImage waveform;

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

        [Header("Timeline Components")]
        [SerializeField] private RectTransform TimelineSlider;
        [SerializeField] private RectTransform TimelineGridSelect;
        [SerializeField] private RectTransform TimelineEventGrid;
        [SerializeField] private TMP_Text TimelinePlaybackBeat;
        public ScrollRect TimelineScroll;
        public RectTransform TimelineContent;
        [SerializeField] private ZoomComponent zoomComponent;
        [SerializeField] private RectTransform TimelineSongPosLineRef;
        [SerializeField] private RectTransform TimelineEventObjRef;
        [SerializeField] private RectTransform LayersRect;

        [SerializeField] private GameObject TimelineSectionDisplay;
        [SerializeField] private TMP_Text TimelineSectionText;
        [SerializeField] private Slider TimelineSectionProgress;

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

        public float leftSide => (TimelineContent.localPosition.x / TimelineContent.localScale.x) * -1;
        public float rightSide => (TimelineScroll.viewport.rect.width / TimelineContent.localScale.x) + leftSide;
        
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
            // beatmap entities
            for (int i = 0; i < eventObjs.Count; i++)
            {
                Destroy(eventObjs[i].gameObject);
            }
            eventObjs.Clear();

            for (int i = 0; i < GameManager.instance.Beatmap.entities.Count; i++)
            {
                var e = GameManager.instance.Beatmap.entities[i];

                AddEventObject(e.datamodel, false, new Vector3(e.beat, -e.track * LayerHeight()), e, false, RandomID());
            }

            SpecialInfo.Setup();
            UpdateOffsetText();
            UpdateStartingBPMText();
            UpdateStartingVolText();
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
                zoomComponent.ZoomIn(3, Vector2.zero);
            });
            ZoomOutBTN.onClick.AddListener(delegate
            {
                zoomComponent.ZoomOut(-3, Vector2.zero);
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

            timelineState.SetState(CurrentTimelineState.State.Selection);

            AutoBtnUpdate();
            GameManager.instance.onSectionChange += OnSectionChange;
        }

        public void FitToSong()
        {
            var currentSizeDelta = TimelineContent.sizeDelta;
            float songBeats = Conductor.instance.SongLengthInBeats();
            if (songBeats == 0) songBeats = 320;
            else songBeats += 10;
            TimelineContent.sizeDelta = new Vector2(songBeats, currentSizeDelta.y);
            TimelineEventGrid.sizeDelta = new Vector2(songBeats, currentSizeDelta.y);
        }

        public void CreateWaveform()
        {
            Debug.Log("what");
            // DrawWaveform();
            StartCoroutine(DrawWaveformRealtime());
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
            waveform.rectTransform.anchoredPosition = new Vector2(
                -(GameManager.instance.Beatmap.firstBeatOffset / (60.0f / GameManager.instance.Beatmap.bpm)), 
                waveform.rectTransform.anchoredPosition.y);

            WaveformBTN.transform.GetChild(0).GetComponent<Image>().color = (Conductor.instance.musicSource.clip != null && waveform.gameObject.activeInHierarchy) ? Color.white : Color.gray;

            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                SongBeat.text = $"Beat {string.Format("{0:0.000}", TimelineSlider.localPosition.x)}";
                SongPos.text = FormatTime((float) Conductor.instance.GetSongPosFromBeat(TimelineSlider.localPosition.x));
            }
            else
            {
                SongBeat.text = $"Beat {string.Format("{0:0.000}", Conductor.instance.songPositionInBeats)}";
                SongPos.text = FormatTime(Conductor.instance.songPosition);
            }
            TimelineSectionProgress.value = GameManager.instance.sectionProgress;

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
                if (Input.GetKey(KeyCode.LeftShift)) moveSpeed *= 6;
                
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                {
                    TimelineContent.transform.localPosition += new Vector3(moveSpeed * Time.deltaTime, 0);
                }
                else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                {
                    TimelineContent.transform.localPosition += new Vector3(-moveSpeed * Time.deltaTime, 0);
                }
            }
            #endregion

            if (Input.GetMouseButton(1) && !Conductor.instance.isPlaying && Editor.MouseInRectTransform(TimelineGridSelect))
            {
                movingPlayback = true;
            }
            else if (Input.GetMouseButtonUp(1) || Conductor.instance.isPlaying)
            {
                movingPlayback = false;
            }
            if (movingPlayback)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(TimelineContent, Input.mousePosition, Editor.instance.EditorCamera, out lastMousePos);
                TimelineSlider.localPosition = new Vector3(Mathf.Max(Mathp.Round2Nearest(lastMousePos.x + 0.12f, Timeline.SnapInterval()), 0), TimelineSlider.transform.localPosition.y);

                if (TimelineSlider.localPosition.x != lastBeatPos)
                    Conductor.instance.SetBeat(TimelineSlider.transform.localPosition.x);

                lastBeatPos = TimelineSlider.localPosition.x;
            }

            if (Conductor.instance.isPlaying)
                PlaybackFocus(true);

            TimelineContent.transform.localPosition = new Vector3(Mathf.Clamp(TimelineContent.transform.localPosition.x, Mathf.NegativeInfinity, 0), TimelineContent.transform.localPosition.y);

            CurrentTempo.text = $"    = {Conductor.instance.songBpm}";

            LayersRect.GetWorldCorners(LayerCorners);
        }

        public static float GetScaleModifier()
        {
            Camera cam = Editor.instance.EditorCamera;
            return Mathf.Pow(cam.pixelWidth/1280f, 1f) * Mathf.Pow(cam.pixelHeight/720f, 0f);
        }

        public Vector2 LayerCornersToDist()
        {
            Vector3[] v = LayerCorners;
            return new Vector2(Mathf.Abs(v[1].x - v[2].x), Mathf.Abs(v[3].y - v[1].y));
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
            {   PauseBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
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
        private string FormatTime(float time)
        {
            int minutes = (int)time / 60;
            int seconds = (int)time - 60 * minutes;
            int milliseconds = (int)(1000 * (time - minutes * 60 - seconds));
            return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }

        public bool CheckIfMouseInTimeline()
        {
            return (this.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(TimelineEventGrid, Input.mousePosition, Editor.instance.EditorCamera));
        }

        #endregion

        #region Functions

        public void PlaybackFocus(bool lerp)
        {
            var lerpSpd = (lerp) ? 12f : 10000000; // im lazy

            var newPos = new Vector3((-Conductor.instance.songPositionInBeats * TimelineContent.localScale.x) + 200, TimelineContent.transform.localPosition.y);
            TimelineContent.transform.localPosition =
                Vector3.Lerp(TimelineContent.transform.localPosition, newPos, Time.deltaTime * lerpSpd);
        }

        public void WaveformToggle()
        {
            if (Conductor.instance.musicSource.clip == null) return;

            waveform.gameObject.SetActive(!waveform.gameObject.activeInHierarchy);
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

        public TimelineEventObj AddEventObject(string eventName, bool dragNDrop = false, Vector3 pos = new Vector3(), DynamicBeatmap.DynamicEntity entity = null, bool addEvent = false, string eventId = "")
        {
            var game = EventCaller.instance.GetMinigame(eventName.Split(0));
            var action = EventCaller.instance.GetGameAction(game, eventName.Split(1));
            GameObject g = Instantiate(TimelineEventObjRef.gameObject, TimelineEventObjRef.parent);
            g.transform.localPosition = pos;
            g.transform.GetChild(3).GetComponent<TMP_Text>().text = action.displayName;

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
                entity.track = entity.eventObj.GetTrack();
            }


            if (addEvent)
            {
                DynamicBeatmap.DynamicEntity tempEntity = entity;

                if (entity == null)
                {
                    DynamicBeatmap.DynamicEntity en = new DynamicBeatmap.DynamicEntity();
                    en.datamodel = eventName;
                    en.eventObj = eventObj;

                    GameManager.instance.Beatmap.entities.Add(en);
                    GameManager.instance.SortEventsList();

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
                    GameManager.instance.Beatmap.entities.Add(entity);
                    GameManager.instance.SortEventsList();
                }
            }

            eventObjs.Add(eventObj);

            eventObj.eventObjID = eventId;

            return eventObj;
        }

        private List<TimelineEventObj> duplicatedEventObjs = new List<TimelineEventObj>();
        public TimelineEventObj CopyEventObject(TimelineEventObj e)
        {
            DynamicBeatmap.DynamicEntity clone = e.entity.DeepCopy();
            TimelineEventObj dup = AddEventObject(clone.datamodel, false, new Vector3(clone.beat, -clone.track * Timeline.instance.LayerHeight()), clone, true, RandomID());
            duplicatedEventObjs.Add(dup);

            return dup;
        }

        public void FinalizeDuplicateEventStack()
        {
            CommandManager.instance.Execute(new Commands.Duplicate(duplicatedEventObjs));
            duplicatedEventObjs = new List<TimelineEventObj>();
        }

        public void DestroyEventObject(DynamicBeatmap.DynamicEntity entity)
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
            return Mathf.Clamp(Mathp.Round2Nearest(y, size), -size * 4f, 0f);
        }

        public float LayerHeight()
        {
            return LayersRect.rect.height / 5f;
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

        public void UpdateOffsetText()
        {
            FirstBeatOffset.text = (GameManager.instance.Beatmap.firstBeatOffset * 1000f).ToString("G");
        }

        public void UpdateOffsetFromText()
        {
            // Failsafe against empty string.
            if (String.IsNullOrEmpty(FirstBeatOffset.text))
                FirstBeatOffset.text = "0";
            
            // Convert ms to s.
            var newOffset = Convert.ToSingle(FirstBeatOffset.text) / 1000f;

            // Limit decimal places to 4.
            newOffset = (float)System.Math.Round(newOffset, 4);

            GameManager.instance.Beatmap.firstBeatOffset = newOffset;

            UpdateOffsetText();
        }

        public void UpdateStartingBPMText()
        {
            StartingTempoSpecialAll.text = GameManager.instance.Beatmap.bpm.ToString("G");
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

            GameManager.instance.Beatmap.bpm = (float) newBPM;

            // In case the newBPM ended up differing from the inputted string.
            UpdateStartingBPMText();

            Timeline.instance.FitToSong();
        }

        public void UpdateStartingVolText()
        {
            StartingVolumeSpecialVolume.text = (GameManager.instance.Beatmap.musicVolume).ToString("G");
        }

        public void UpdateStartingVolFromText()
        {
            // Failsafe against empty string.
            if (String.IsNullOrEmpty(StartingVolumeSpecialVolume.text))
                StartingVolumeSpecialVolume.text = "100";
            
            var newVol = Convert.ToInt32(StartingVolumeSpecialVolume.text);
            newVol = Mathf.Clamp(newVol, 0, 100);

            GameManager.instance.Beatmap.musicVolume = newVol;

            UpdateStartingVolText();
        }

        public void OnSectionChange(DynamicBeatmap.ChartSection section)
        {
            if (section == null)
            {
                TimelineSectionDisplay.SetActive(false);
            }
            else
            {
                if (!TimelineSectionDisplay.activeSelf)
                    TimelineSectionDisplay.SetActive(true);
                TimelineSectionText.text = section.sectionName;
                TimelineSectionProgress.value = GameManager.instance.sectionProgress;
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