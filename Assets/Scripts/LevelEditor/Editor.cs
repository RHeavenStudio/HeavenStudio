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

using HeavenStudio.Editor;
using HeavenStudio.Editor.Track;
using HeavenStudio.Util;

using System.IO.Compression;
using System.Text;

namespace HeavenStudio.Editor
{
    public class Editor : MonoBehaviour
    {
        private Initializer Initializer;

        [SerializeField] public Canvas MainCanvas;
        [SerializeField] public Camera EditorCamera;

        // [SerializeField] public GameObject EditorLetterbox;
        [SerializeField] public GameObject GameLetterbox;

        [Header("Rect")]
        [SerializeField] private RenderTexture ScreenRenderTexture;
        [SerializeField] private RawImage Screen;
        [SerializeField] private RectTransform GridGameSelector;
        public RectTransform eventSelectorBG;

        [Header("Components")]
        [SerializeField] private Timeline Timeline;
        [SerializeField] private TMP_Text GameEventSelectorTitle;

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

        [SerializeField] private Button EditorThemeBTN;
        [SerializeField] private Button EditorSettingsBTN;

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
        public bool isCursorEnabled = true;

        public static Editor instance { get; private set; }

        private void Start()
        {
            instance = this;
            Initializer = GetComponent<Initializer>();
            canSelect = true;
        }

        public void Init()
        {
            GameCamera.instance.camera.targetTexture = ScreenRenderTexture;
            GameManager.instance.CursorCam.targetTexture = ScreenRenderTexture;
            GameManager.instance.OverlayCamera.targetTexture = ScreenRenderTexture;
            Screen.texture = ScreenRenderTexture;

            GameManager.instance.Init();
            Timeline.Init();

            for (int i = 0; i < EventCaller.instance.minigames.Count; i++)
            {
                GameObject GameIcon_ = Instantiate(GridGameSelector.GetChild(0).gameObject, GridGameSelector);
                GameIcon_.GetComponent<Image>().sprite = GameIcon(EventCaller.instance.minigames[i].name);
                GameIcon_.GetComponent<GridGameSelectorGame>().MaskTex = GameIconMask(EventCaller.instance.minigames[i].name);
                GameIcon_.GetComponent<GridGameSelectorGame>().UnClickIcon();
                GameIcon_.gameObject.SetActive(true);
                GameIcon_.name = EventCaller.instance.minigames[i].displayName;
            }

            Tooltip.AddTooltip(NewBTN.gameObject, "New <color=#adadad>[Ctrl+N]</color>");
            Tooltip.AddTooltip(OpenBTN.gameObject, "Open <color=#adadad>[Ctrl+O]</color>");
            Tooltip.AddTooltip(SaveBTN.gameObject, "Save Project <color=#adadad>[Ctrl+S]</color>\nSave Project As <color=#adadad>[Ctrl+Alt+S]</color>");
            Tooltip.AddTooltip(UndoBTN.gameObject, "Undo <color=#adadad>[Ctrl+Z]</color>");
            Tooltip.AddTooltip(RedoBTN.gameObject, "Redo <color=#adadad>[Ctrl+Y or Ctrl+Shift+Z]</color>");
            Tooltip.AddTooltip(MusicSelectBTN.gameObject, "Music Select");
            Tooltip.AddTooltip(EditorThemeBTN.gameObject, "Editor Theme");
            Tooltip.AddTooltip(FullScreenBTN.gameObject, "Preview <color=#adadad>[Tab]</color>");
            Tooltip.AddTooltip(TempoFinderBTN.gameObject, "Tempo Finder");
            Tooltip.AddTooltip(SnapDiagBTN.gameObject, "Snap Settings");

            Tooltip.AddTooltip(EditorSettingsBTN.gameObject, "Editor Settings <color=#adadad>[Ctrl+Shift+O]</color>");
            UpdateEditorStatus(true);
        }

        public void LateUpdate()
        {
            #region Keyboard Shortcuts
            if (!editingInputField)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    Fullscreen();
                }

                if (Input.GetKeyDown(KeyCode.Delete))
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
                        LoadRemix("");
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
                UndoBTN.transform.GetChild(0).GetComponent<Image>().color = "BD8CFF".Hex2RGB();
            else
                UndoBTN.transform.GetChild(0).GetComponent<Image>().color = Color.gray;

            if (CommandManager.instance.canRedo())
                RedoBTN.transform.GetChild(0).GetComponent<Image>().color = "FFD800".Hex2RGB();
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
                    Conductor.instance.musicSource.clip = await LoadClip(Path.Combine(paths)); 
                    changedMusic = true;
                    
                    Timeline.FitToSong();
                }
            } 
            );
            #else
            StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", extensions, false, async (string[] paths) =>
            {
                if (paths.Length > 0)
                {
                    Conductor.instance.musicSource.clip = await LoadClip("file://" + Path.Combine(paths));
                    changedMusic = true;

                    Timeline.FitToSong();
                }
            }
            );
            #endif
        }

        private async Task<AudioClip> LoadClip(string path)
        {
            AudioClip clip = null;

            AudioType audioType = AudioType.OGGVORBIS;

            // this is a bad solution but i'm lazy
            if (path.Substring(path.Length - 3) == "ogg")
                audioType = AudioType.OGGVORBIS;
            else if (path.Substring(path.Length - 3) == "mp3")
                audioType = AudioType.MPEG;
            else if (path.Substring(path.Length - 3) == "wav")
                audioType = AudioType.WAV;

            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
            {
                uwr.SendWebRequest();

                try
                {
                    while (!uwr.isDone) await Task.Delay(5);

                    if (uwr.result == UnityWebRequest.Result.ProtocolError) Debug.Log($"{uwr.error}");
                    else
                    {
                        clip = DownloadHandlerAudioClip.GetContent(uwr);
                    }
                }
                catch (Exception err)
                {
                    Debug.Log($"{err.Message}, {err.StackTrace}");
                }
            }

            return clip;
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
                new ExtensionFilter("Heaven Studio Remix File", "tengoku")
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
            using (FileStream zipFile = File.Open(path, FileMode.Create))
            {
                using (var archive = new ZipArchive(zipFile, ZipArchiveMode.Update))
                {
                    var levelFile = archive.CreateEntry("remix.json", System.IO.Compression.CompressionLevel.NoCompression);
                    using (var zipStream = levelFile.Open())
                        zipStream.Write(Encoding.UTF8.GetBytes(GetJson()), 0, Encoding.UTF8.GetBytes(GetJson()).Length);

                    var propertiesFile = archive.CreateEntry("properties.json", System.IO.Compression.CompressionLevel.NoCompression);
                    using (var zipStream = propertiesFile.Open())
                        zipStream.Write(Encoding.UTF8.GetBytes(GetPropertiesJson()), 0, Encoding.UTF8.GetBytes(GetPropertiesJson()).Length);

                    if (changedMusic || currentRemixPath != path)
                    {
                        // this gets rid of the music file for some reason, someone remind me to find a fix for this soon
                    }

                    byte[] bytes = OggVorbis.VorbisPlugin.GetOggVorbis(Conductor.instance.musicSource.clip, 1);
                    var musicFile = archive.CreateEntry("song.ogg", System.IO.Compression.CompressionLevel.NoCompression);
                    using (var zipStream = musicFile.Open())
                        zipStream.Write(bytes, 0, bytes.Length);
                }

                currentRemixPath = path;
                UpdateEditorStatus(false);
            }
        }

        public void LoadRemix(string json = "")
        {
            GameManager.instance.LoadRemix(json);
            Timeline.instance.LoadRemix();
            Timeline.instance.TempoInfo.UpdateStartingBPMText();
            Timeline.instance.VolumeInfo.UpdateStartingVolumeText();
            Timeline.instance.TempoInfo.UpdateOffsetText();
            Timeline.FitToSong();
        }

        public void GetProperties(string json = "")
        {
            Properties.PropController.instance.LoadProperties(json);
            Debug.Log("properties sent");
        }

        public void OpenRemix()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Heaven Studio Remix File", new string[] { "tengoku", "rhmania" })
            };

            StandaloneFileBrowser.OpenFilePanelAsync("Open Remix", "", extensions, false, (string[] paths) =>
            {
                var path = Path.Combine(paths);

                if (path != String.Empty)
                {
                    loadedMusic = false;

                    using (FileStream zipFile = File.Open(path, FileMode.Open))
                    {
                        using (var archive = new ZipArchive(zipFile, ZipArchiveMode.Read))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (entry.Name == "remix.json")
                                {
                                    using (var stream = entry.Open())
                                    {
                                        byte[] bytes;
                                        using (var ms = new MemoryStream())
                                        {
                                            stream.CopyTo(ms);
                                            bytes = ms.ToArray();
                                            string json = Encoding.UTF8.GetString(bytes);
                                            LoadRemix(json);
                                        }
                                    }
                                }
                                else if (entry.Name == "song.ogg")
                                {
                                    using (var stream = entry.Open())
                                    {
                                        byte[] bytes;
                                        using (var ms = new MemoryStream())
                                        {
                                            stream.CopyTo(ms);
                                            bytes = ms.ToArray();
                                            Conductor.instance.musicSource.clip = OggVorbis.VorbisPlugin.ToAudioClip(bytes, "music");
                                            loadedMusic = true;
                                            Timeline.FitToSong();
                                        }
                                    }
                                }
                                else if (entry.Name == "properties.json")
                                {
                                    using (var stream = entry.Open())
                                    {
                                        byte[] bytes;
                                        using (var ms = new MemoryStream())
                                        {
                                            stream.CopyTo(ms);
                                            bytes = ms.ToArray();
                                            string json = Encoding.UTF8.GetString(bytes);
                                            GetProperties(json);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!loadedMusic)
                        Conductor.instance.musicSource.clip = null;

                    currentRemixPath = path;
                    remixName = Path.GetFileName(path);
                    UpdateEditorStatus(false);
                    CommandManager.instance.Clear();
                    Timeline.FitToSong();
                }
            });
        }

        #endregion

        public void Fullscreen()
        {
            if (fullscreen == false)
            {
                // EditorLetterbox.SetActive(false);
                GameLetterbox.SetActive(true);

                MainCanvas.enabled = false;
                EditorCamera.enabled = false;
                GameCamera.instance.camera.targetTexture = null;
                GameManager.instance.CursorCam.enabled = false;
                GameManager.instance.OverlayCamera.targetTexture = null;
                fullscreen = true;
            }
            else
            {
                // EditorLetterbox.SetActive(true);
                GameLetterbox.SetActive(false);

                MainCanvas.enabled = true;
                EditorCamera.enabled = true;
                GameCamera.instance.camera.targetTexture = ScreenRenderTexture;
                GameManager.instance.CursorCam.enabled = true && isCursorEnabled;
                GameManager.instance.OverlayCamera.targetTexture = ScreenRenderTexture;
                fullscreen = false;

                GameCamera.instance.camera.rect = new Rect(0, 0, 1, 1);
                GameManager.instance.CursorCam.rect = new Rect(0, 0, 1, 1);
                GameManager.instance.OverlayCamera.rect = new Rect(0, 0, 1, 1);
                EditorCamera.rect = new Rect(0, 0, 1, 1);
            }
        }

        private void UpdateEditorStatus(bool updateTime)
        {
            if (discordDuringTesting || !Application.isEditor)
            DiscordRPC.DiscordRPC.UpdateActivity("In Editor", $"{remixName}", updateTime);
        }

        public string GetJson()
        {
            string json = string.Empty;
            json = JsonConvert.SerializeObject(GameManager.instance.Beatmap);
            return json;
        }

        public string GetPropertiesJson()
        {
            string json = string.Empty;
            json = JsonConvert.SerializeObject(Properties.PropController.instance.properties);
            return json;
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
