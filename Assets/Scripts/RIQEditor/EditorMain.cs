using System.IO;
using System.IO.Compression;
using HeavenStudio.Editor;
using SFB;
using Starpelly;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.RIQEditor
{
    public class EditorMain : MonoBehaviour
    {
        public static EditorMain Instance { get; private set; }

        public readonly EditorTheme Theme = new();
        
        public Camera EditorCamera;
        public Canvas MainCanvas, TimelineCanvas;

        public Timeline Timeline;
        
        [Header("Rect")]
        [SerializeField] private RenderTexture ScreenRenderTexture;

        [Header("GridGameSelector")] 
        [SerializeField] private GameObject SpecialIconPrefab;
        [SerializeField] private GameObject NormalIconPrefab;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Debug.LogError("There shouldn't be more than 1 EditorMain!");
        }

        public void Init()
        {
            GameManager.instance.StaticCamera.targetTexture = ScreenRenderTexture;
            GameManager.instance.CursorCam.targetTexture = ScreenRenderTexture;
            
            GameManager.instance.Init();

            foreach (var minigame in EventCaller.instance.minigames)
            {
                var pref = minigame.fxOnly ? SpecialIconPrefab : NormalIconPrefab;
                var gameIcon = Instantiate(pref, pref.transform.parent);
                gameIcon.GetComponent<Image>().sprite = GameIcon(minigame.name);
            }
            SpecialIconPrefab.SetActive(false);
            NormalIconPrefab.SetActive(false);

            Theme.LayerColors = new[]
            {
                "EF476F".Hex2RGB(),
                "F5813D".Hex2RGB(),
                "FFD166".Hex2RGB(),
                "06D6A0".Hex2RGB(),
                "118AB2".Hex2RGB(),
                "1134B2".Hex2RGB(),
                "7D3DBC".Hex2RGB(),
                "BD439B".Hex2RGB()
            };
            Theme.LoadColors();
        }

        #region Custom

        public static Sprite GameIcon(string name)
        {
            return Resources.Load<Sprite>($"Sprites/Editor/GameIcons/{name}");
        }

        public static Texture GameIconMask(string name)
        {
            return Resources.Load<Texture>($"Sprites/Editor/GameIcons/{name}_mask");
        }

        #endregion

        #region Saving and Loading

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
                string extension = path.GetExtension();

                using var zipFile = File.Open(path, FileMode.Open);
                using var archive = new ZipArchive(zipFile, ZipArchiveMode.Read);

                foreach (var entry in archive.Entries)
                    switch (entry.Name)
                    {
                        case "remix.json":
                        {
                            using var stream = entry.Open();
                            using var reader = new StreamReader(stream);
                            LoadRemix(reader.ReadToEnd(), extension);

                            break;
                        }
                        case "song.ogg":
                        {
                            using var stream = entry.Open();
                            using var memoryStream = new MemoryStream();
                            stream.CopyTo(memoryStream);
                            var MusicBytes = memoryStream.ToArray();
                            Conductor.instance.musicSource.clip = OggVorbis.VorbisPlugin.ToAudioClip(MusicBytes, "music");
                            break;
                        }
                    }
            });
        }
        
        public void LoadRemix(string json = "", string type = "riq")
        {
            GameManager.instance.LoadRemix(json, type);
            Timeline.Load();
        }

        #endregion
    }
}
