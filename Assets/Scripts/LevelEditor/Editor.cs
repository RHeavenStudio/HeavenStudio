using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using Newtonsoft.Json;
using TMPro;
using Starpelly;
using SFB;

using HeavenStudio.Common;
using HeavenStudio.Editor.Track;
using HeavenStudio.Util;
using HeavenStudio.StudioDance;

using Jukebox;

using System.IO.Compression;
using System.Text;

namespace HeavenStudio.Editor
{
    public class Editor : MonoBehaviour
    {
        private GameInitializer Initializer;

        [SerializeField] public Canvas MainCanvas;
        [SerializeField] public Camera EditorCamera;

        [Header("Rect")]
        [SerializeField] private RenderTexture ScreenRenderTexture;
        [SerializeField] private RawImage Screen;
        [SerializeField] private RectTransform GridGameSelector;
        public RectTransform eventSelectorBG;

        [Header("Components")]
        [SerializeField] private Timeline Timeline;
        [SerializeField] private TMP_Text GameEventSelectorTitle;
        [SerializeField] private TMP_Text BuildDateDisplay;
        [SerializeField] public StudioDanceManager StudioDanceManager;

        [Header("Toolbar")]
        [SerializeField] private Button NewBTN;
        [SerializeField] private Button OpenBTN;
        [SerializeField] private Button SaveBTN;
        [SerializeField] private Button UndoBTN;
        [SerializeField] private Button RedoBTN;
        [SerializeField] private Button MusicSelectBTN;
        [SerializeField] private Button FullScreenBTN;
        [SerializeField] private Button TempoFinderBTN;
        [SerializeField] private Button SnapDiagBTN;
        [SerializeField] private Button ChartParamBTN;

        [SerializeField] private Button EditorThemeBTN;
        [SerializeField] private Button EditorSettingsBTN;

        [Header("Dialogs")]
        [SerializeField] private Dialog[] Dialogs;

        [Header("Tooltip")]
        public TMP_Text tooltipText;

        [Header("Properties")]
        private bool changedMusic = false;
        private bool loadedMusic = false;
        private string currentRemixPath = "";
        private string remixName = "";
        public bool fullscreen;
        public bool discordDuringTesting = false;
        public bool canSelect = true;
        public bool editingInputField = false;
        public bool inAuthorativeMenu = false;
        public bool isCursorEnabled = true;
        public bool isDiscordEnabled = true;

        public bool isShortcutsEnabled { get { return (!inAuthorativeMenu) && (!editingInputField); } }

        public static Editor instance { get; private set; }

        private void Start()
        {
            instance = this;
            Initializer = GetComponent<GameInitializer>();
            canSelect = true;
        }

        public void Init()
        {
            GameManager.instance.StaticCamera.targetTexture = ScreenRenderTexture;
            GameManager.instance.CursorCam.targetTexture = ScreenRenderTexture;
            Screen.texture = ScreenRenderTexture;

            GameManager.instance.Init();
            Timeline.Init();

            foreach (var minigame in EventCaller.instance.minigames)
                AddIcon(minigame);

            Tooltip.AddTooltip(NewBTN.gameObject, "New <color=#adadad>[Ctrl+N]</color>");
            Tooltip.AddTooltip(OpenBTN.gameObject, "Open <color=#adadad>[Ctrl+O]</color>");
            Tooltip.AddTooltip(SaveBTN.gameObject, "Save Project <color=#adadad>[Ctrl+S]</color>\nSave Project As <color=#adadad>[Ctrl+Alt+S]</color>");
            Tooltip.AddTooltip(UndoBTN.gameObject, "Undo <color=#adadad>[Ctrl+Z]</color>");
            Tooltip.AddTooltip(RedoBTN.gameObject, "Redo <color=#adadad>[Ctrl+Y or Ctrl+Shift+Z]</color>");
            Tooltip.AddTooltip(MusicSelectBTN.gameObject, "Music Select");
            Tooltip.AddTooltip(FullScreenBTN.gameObject, "Preview <color=#adadad>[Tab]</color>");
            Tooltip.AddTooltip(TempoFinderBTN.gameObject, "Tempo Finder");
            Tooltip.AddTooltip(SnapDiagBTN.gameObject, "Snap Settings");
            Tooltip.AddTooltip(ChartParamBTN.gameObject, "Remix Properties");

            Tooltip.AddTooltip(EditorSettingsBTN.gameObject, "Editor Settings <color=#adadad>[Ctrl+Shift+O]</color>");
            UpdateEditorStatus(true);

            BuildDateDisplay.text = GlobalGameManager.buildTime;
            isCursorEnabled  = PersistentDataManager.gameSettings.editorCursorEnable;
            isDiscordEnabled = PersistentDataManager.gameSettings.discordRPCEnable;
            GameManager.instance.CursorCam.enabled = isCursorEnabled;
        }

        public void AddIcon(Minigames.Minigame minigame)
        {
            if (minigame.hidden) return;
            GameObject GameIcon_ = Instantiate(GridGameSelector.GetChild(0).gameObject, GridGameSelector);
            GameIcon_.GetComponent<Image>().sprite = GameIcon(minigame.name);
            GameIcon_.GetComponent<GridGameSelectorGame>().MaskTex = GameIconMask(minigame.name);
            GameIcon_.GetComponent<GridGameSelectorGame>().UnClickIcon();
            GameIcon_.gameObject.SetActive(true);
            GameIcon_.name = minigame.displayName;
        }

        public void LateUpdate()
        {
            #region Keyboard Shortcuts
            if (isShortcutsEnabled)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    Fullscreen();
                }

                if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
                {
                    List<TimelineEventObj> ev = new List<TimelineEventObj>();
                    for (int i = 0; i < Selections.instance.eventsSelected.Count; i++) ev.Add(Selections.instance.eventsSelected[i]);
                    CommandManager.instance.Execute(new Commands.Deletion(ev));
                }

                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                            CommandManager.instance.Redo();
                        else
                            CommandManager.instance.Undo();
                    }
                    else if (Input.GetKeyDown(KeyCode.Y))
                    {
                        CommandManager.instance.Redo();
                    }

                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        if (Input.GetKeyDown(KeyCode.D))
                        {
                            ToggleDebugCam();
                        }
                    }
                }

                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (Input.GetKeyDown(KeyCode.N))
                    {
                        NewBTN.onClick.Invoke();
                    }
                    else if (Input.GetKeyDown(KeyCode.O))
                    {
                        OpenRemix();
                    }
                    else if (Input.GetKey(KeyCode.LeftAlt))
                    {
                        if (Input.GetKeyDown(KeyCode.S))
                        {
                            SaveRemix(true);
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.S))
                    {
                        SaveRemix(false);
                    }
                }
            }
            #endregion

            if (CommandManager.instance.canUndo())
                UndoBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            else
                UndoBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;

            if (CommandManager.instance.canRedo())
                RedoBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            else
                RedoBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;

            if (Timeline.instance.timelineState.selected && Editor.instance.canSelect)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    List<TimelineEventObj> selectedEvents = Timeline.instance.eventObjs.FindAll(c => c.selected == true && c.eligibleToMove == true);

                    if (selectedEvents.Count > 0)
                    {
                        List<TimelineEventObj> result = new List<TimelineEventObj>();

                        for (int i = 0; i < selectedEvents.Count; i++)
                        {
                            //TODO: this is in LateUpdate, so this will never run! change this to something that works properly
                            if (!(selectedEvents[i].isCreating || selectedEvents[i].wasDuplicated))
                            {
                                result.Add(selectedEvents[i]);
                            }
                            selectedEvents[i].OnUp();
                        }
                        CommandManager.instance.Execute(new Commands.Move(result));
                    }
                }
            }
        }

        public static Sprite GameIcon(string name)
        {
            return Resources.Load<Sprite>($"Sprites/Editor/GameIcons/{name}");
        }

        public static Texture GameIconMask(string name)
        {
            return Resources.Load<Texture>($"Sprites/Editor/GameIcons/{name}_mask");
        }

        #region Dialogs

        public void SelectMusic()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Music Files", "mp3", "ogg", "wav")
            };

            #if UNITY_STANDALONE_WINDOWS
            StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", extensions, false, async (string[] paths) => 
            {
                if (paths.Length > 0)
                {
                    try
                    {
                        if (paths.Length == 0) return;
                        RiqFileHandler.WriteSong(paths[0]);
                        StartCoroutine(LoadMusic());
                        return;
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log($"Error selecting music file: {e.Message}");
                        return;
                    }
                }
                await Task.Yield();
            } 
            );
            #else
            StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", extensions, false, async (string[] paths) =>
            {
                if (paths.Length > 0)
                {
                    try
                    {
                        if (paths.Length == 0) return;
                        RiqFileHandler.WriteSong(paths[0]);
                        StartCoroutine(LoadMusic());
                        return;
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log($"Error selecting music file: {e.Message}");
                        Debug.LogException(e);
                        return;
                    }
                }
                await Task.Yield();
            }
            );
            #endif
        }

        IEnumerator LoadMusic()
        {
            yield return GameManager.instance.LoadMusic();
            Timeline.FitToSong();
            // Timeline.CreateWaveform();
        }

        public void SaveRemix(bool saveAs = true)
        {
            if (saveAs == true)
            {
                SaveRemixFilePanel();
            }
            else
            {
                if (currentRemixPath == string.Empty)
                {
                    SaveRemixFilePanel();
                }
                else
                {
                    SaveRemixFile(currentRemixPath);
                }
            }
        }

        private void SaveRemixFilePanel()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Heaven Studio Remix File", "riq")
            };
            
            StandaloneFileBrowser.SaveFilePanelAsync("Save Remix As", "", "remix_level", extensions, (string path) =>
            {
                if (path != String.Empty)
                {
                    SaveRemixFile(path);
                }
            });
        }

        private void SaveRemixFile(string path)
        {
            try
            {
                RiqFileHandler.WriteRiq(GameManager.instance.Beatmap);
                RiqFileHandler.PackRiq(path, true);
                Debug.Log("Packed RIQ successfully!");
                return;
            }
            catch (System.Exception e)
            {
                Debug.Log($"Error packing RIQ: {e.Message}");
                return;
            }
        }

        public void NewRemix()
        {
            if (Timeline.instance != null)
                Timeline.instance?.Stop(0);
            else
                GameManager.instance.Stop(0);
            LoadRemix(true);
        }

        public void LoadRemix(bool create = false)
        {
            if (create)
                GameManager.instance.NewRemix();
            else
                GameManager.instance.LoadRemix();
            Timeline.instance.LoadRemix();
            Timeline.FitToSong();

            currentRemixPath = string.Empty;
        }

        public void OpenRemix()
        {
            var extensions = new[]
            {
                new ExtensionFilter("All Supported Files ", new string[] { "riq", "tengoku", "rhmania" }),
                new ExtensionFilter("Heaven Studio Remix File ", new string[] { "riq" }),
                new ExtensionFilter("Legacy Heaven Studio Remix ", new string[] { "tengoku", "rhmania" })
            };

            StandaloneFileBrowser.OpenFilePanelAsync("Open Remix", "", extensions, false, (string[] paths) =>
            {
                var path = Path.Combine(paths);

                if (path == string.Empty) return;

                try
                {
                    string tmpDir = RiqFileHandler.ExtractRiq(path);
                    Debug.Log("Imported RIQ successfully!");
                    LoadRemix();
                }
                catch (System.Exception e)
                {
                    Debug.Log($"Error importing RIQ: {e.Message}");
                    Debug.LogException(e);
                }

                StartCoroutine(LoadMusic());

                currentRemixPath = path;
                remixName = Path.GetFileName(path);
                UpdateEditorStatus(false);
                CommandManager.instance.Clear();
            });
        }

        #endregion

        public void Fullscreen()
        {
            MainCanvas.gameObject.SetActive(fullscreen);
            if (fullscreen == false)
            {
                MainCanvas.enabled = false;
                EditorCamera.enabled = false;
                GameManager.instance.StaticCamera.targetTexture = null;
                GameManager.instance.CursorCam.enabled = false;
                fullscreen = true;

            }
            else
            {
                MainCanvas.enabled = true;
                EditorCamera.enabled = true;
                GameManager.instance.StaticCamera.targetTexture = ScreenRenderTexture;
                GameManager.instance.CursorCam.enabled = true && isCursorEnabled;
                fullscreen = false;

                GameCamera.instance.camera.rect = new Rect(0, 0, 1, 1);
                GameManager.instance.CursorCam.rect = new Rect(0, 0, 1, 1);
                GameManager.instance.OverlayCamera.rect = new Rect(0, 0, 1, 1);
                EditorCamera.rect = new Rect(0, 0, 1, 1);
            }
            Timeline.AutoBtnUpdate();
        }

        private void UpdateEditorStatus(bool updateTime)
        {
            GlobalGameManager.UpdateDiscordStatus($"{remixName}", true, updateTime);
        }

        public void SetGameEventTitle(string txt)
        {
            GameEventSelectorTitle.text = txt;
        }

        public static bool MouseInRectTransform(RectTransform rectTransform)
        {
            return (rectTransform.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Editor.instance.EditorCamera));
        }

        public void ToggleDebugCam()
        {
            var game = GameManager.instance.currentGameO;

            if (game != null)
            {
                foreach(FreeCam c in game.GetComponentsInChildren<FreeCam>(true))
                {
                    c.enabled = !c.enabled;
                }
            }
        }
    }
}
