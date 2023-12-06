using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

using HeavenStudio.Common;
using HeavenStudio.Editor.Track;
using HeavenStudio.StudioDance;

using Jukebox;
using UnityEditor;
using System.Linq;

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
        [SerializeField] private RectTransform GridGameSelectorRect;
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
        [SerializeField] private Button CopyBTN;
        [SerializeField] private Button PasteBTN;
        [SerializeField] private Button MusicSelectBTN;
        [SerializeField] private Button FullScreenBTN;
        [SerializeField] private Button TempoFinderBTN;
        [SerializeField] private Button SnapDiagBTN;
        [SerializeField] private Button ChartParamBTN;
        [SerializeField] private Button SortAlphabetBTN;
        [SerializeField] private Button SortFavoritesBTN;
        [SerializeField] private Button SortChronologicBTN;
        [SerializeField] private TMP_InputField SearchBar;

        [Header("Confirm Quit")]
        [SerializeField] private GameObject _confirmQuitMain;
        [SerializeField] private Button _quitYes;
        [SerializeField] private Button _quitNo;

        [SerializeField] private Button EditorThemeBTN;
        [SerializeField] private Button EditorSettingsBTN;

        [SerializeField] private GameObject DebugHolder;

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

        private Vector2 lastScreenSize = Vector2.zero;

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
            Tooltip.AddTooltip(CopyBTN.gameObject, "Copy <color=#adadad>[Ctrl+C]</color>");
            Tooltip.AddTooltip(PasteBTN.gameObject, "Paste <color=#adadad>[Ctrl+V]</color>");
            Tooltip.AddTooltip(MusicSelectBTN.gameObject, "Music Select");
            Tooltip.AddTooltip(FullScreenBTN.gameObject, "Preview <color=#adadad>[Tab]</color>");
            Tooltip.AddTooltip(TempoFinderBTN.gameObject, "Tempo Finder");
            Tooltip.AddTooltip(SnapDiagBTN.gameObject, "Snap Settings");
            Tooltip.AddTooltip(ChartParamBTN.gameObject, "Remix Properties");
            Tooltip.AddTooltip(SortAlphabetBTN.gameObject, "Sort Alphabetically");
            Tooltip.AddTooltip(SortFavoritesBTN.gameObject, "Sort By Favorite");
            Tooltip.AddTooltip(SortChronologicBTN.gameObject, "Sort Chronologically");
            Tooltip.AddTooltip(SearchBar.gameObject, "Search Games");

            Tooltip.AddTooltip(EditorSettingsBTN.gameObject, "Editor Settings <color=#adadad>[Ctrl+Shift+O]</color>");
            UpdateEditorStatus(true);

            BuildDateDisplay.text = GlobalGameManager.buildTime;
            isCursorEnabled = PersistentDataManager.gameSettings.editorCursorEnable;
            isDiscordEnabled = PersistentDataManager.gameSettings.discordRPCEnable;
            GameManager.instance.CursorCam.enabled = isCursorEnabled;
        }

        public void AddIcon(Minigames.Minigame minigame)
        {
            if (minigame.hidden) return;
            GameObject GameIcon_ = Instantiate(GridGameSelectorRect.GetChild(0).gameObject, GridGameSelectorRect);
            GameIcon_.GetComponent<Image>().sprite = GameIcon(minigame.name);
            GameIcon_.GetComponent<GridGameSelectorGame>().MaskTex = GameIconMask(minigame.name);
            GameIcon_.GetComponent<GridGameSelectorGame>().UnClickIcon();
            GameIcon_.gameObject.SetActive(true);
            GameIcon_.name = minigame.name;

            var ggs = GridGameSelectorRect.GetComponent<GridGameSelector>();
            (minigame.fxOnly ? ggs.fxActive : ggs.mgsActive).Add(GameIcon_.GetComponent<RectTransform>());
        }

        public void ShowQuitPopUp(bool show)
        {
            _confirmQuitMain.SetActive(show);
        }

        public bool ShouldQuit = false;

        public void QuitGame()
        {
            ShouldQuit = true;
            Application.Quit();
        }

        public void Update()
        {
            if (!PersistentDataManager.gameSettings.scaleWScreenSize)
                MainCanvas.scaleFactor = 1.0f + (0.25f * PersistentDataManager.gameSettings.editorScale);

            MainCanvas.GetComponent<CanvasScaler>().uiScaleMode = (PersistentDataManager.gameSettings.scaleWScreenSize) ? CanvasScaler.ScaleMode.ScaleWithScreenSize : CanvasScaler.ScaleMode.ConstantPixelSize;
        }

        public void LateUpdate()
        {
            if (lastScreenSize != new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height))
            {
                // Timeline.OnScreenResize();
            }
            lastScreenSize = new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);

            #region Keyboard Shortcuts
            if (isShortcutsEnabled)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    Fullscreen();
                }

                if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
                {
                    CommandManager.Instance.AddCommand(new Commands.Delete(Selections.instance.eventsSelected.Select(c => c.entity.guid).ToList()));
                }

                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                            CommandManager.Instance.RedoCommand();
                        else
                            CommandManager.Instance.UndoCommand();
                    }
                    else if (Input.GetKeyDown(KeyCode.Y))
                    {
                        CommandManager.Instance.RedoCommand();
                    }
                    else if (Input.GetKeyDown(KeyCode.C))
                    {
                        Timeline.instance.CopySelected();
                    }
                    else if (Input.GetKeyDown(KeyCode.V))
                    {
                        Timeline.instance.Paste();
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

                if (Input.GetKeyDown(KeyCode.F12))
                {
                    DebugHolder.gameObject.SetActive(!DebugHolder.activeInHierarchy);
                }
            }
            #endregion

            // Undo+Redo
            if (CommandManager.Instance.CanUndo())
                UndoBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            else
                UndoBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
            if (CommandManager.Instance.CanRedo())
                RedoBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            else
                RedoBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;

            // Copy+Paste
            if (Selections.instance.eventsSelected.Count > 0)
                CopyBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            else
                CopyBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
            if (Timeline.instance.CopiedEntities.Count > 0)
                PasteBTN.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            else
                PasteBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;


            if (Timeline.instance.timelineState.selected && Editor.instance.canSelect)
            {
                /*
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
                */
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
                new ExtensionFilter("Music Files", "mp3", "ogg", "wav", "aiff", "aif", "aifc")
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
                        Debug.LogException(e);
                        GlobalGameManager.ShowErrorMessage("Error", e.Message);
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
                        GlobalGameManager.ShowErrorMessage("Error", e.Message);
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
                if (currentRemixPath == string.Empty || currentRemixPath == null)
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
                RiqFileHandler.UnlockCache();
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
            {
                GameManager.instance.NewRemix();
                currentRemixPath = string.Empty;
            }
            else
            {
                GameManager.instance.LoadRemix(true);
            }
            Timeline.instance.LoadRemix();
            Timeline.FitToSong();
        }

        public void OpenRemix()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Heaven Studio Remix File ", new string[] { "riq" }),
            };

            StandaloneFileBrowser.OpenFilePanelAsync("Open Remix", "", extensions, false, (string[] paths) =>
            {
                var path = Path.Combine(paths);
                if (path == string.Empty) return;

                try
                {
                    RiqFileHandler.UnlockCache();
                    string tmpDir = RiqFileHandler.ExtractRiq(path);
                    Debug.Log("Imported RIQ successfully!");
                    LoadRemix();
                }
                catch (System.Exception e)
                {
                    Debug.Log($"Error importing RIQ: {e.Message}");
                    Debug.LogException(e);
                    GlobalGameManager.ShowErrorMessage("Error Loading RIQ", e.Message + "\n\n" + e.StackTrace);
                    return;
                }

                StartCoroutine(LoadMusic());

                currentRemixPath = path;
                remixName = Path.GetFileName(path);
                UpdateEditorStatus(false);
                CommandManager.Instance.Clear();
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
                GameManager.instance.CursorCam.enabled = (PlayerInput.CurrentControlStyle == InputSystem.InputController.ControlStyles.Touch)
                    && isCursorEnabled;
                GameManager.instance.CursorCam.targetTexture = null;
                
                GameManager.instance.CursorCam.rect = new Rect(0, 0, 1, 1);
                fullscreen = true;
            }
            else
            {
                MainCanvas.enabled = true;
                EditorCamera.enabled = true;
                GameManager.instance.StaticCamera.targetTexture = ScreenRenderTexture;
                GameManager.instance.CursorCam.enabled = true && isCursorEnabled;
                GameManager.instance.CursorCam.targetTexture = ScreenRenderTexture;
                fullscreen = false;

                GameCamera.instance.camera.rect = new Rect(0, 0, 1, 1);
                GameManager.instance.CursorCam.rect = new Rect(0, 0, 1, 1);
                GameManager.instance.OverlayCamera.rect = new Rect(0, 0, 1, 1);
                EditorCamera.rect = new Rect(0, 0, 1, 1);
            }
            GameManager.instance.CircleCursor.LockCursor(fullscreen);
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
                foreach (FreeCam c in game.GetComponentsInChildren<FreeCam>(true))
                {
                    c.enabled = !c.enabled;
                }
            }
        }
    }
}
