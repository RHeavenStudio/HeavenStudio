using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.IO.Compression;
using System.Text;

using UnityEngine;
using UnityEngine.Audio;

namespace HeavenStudio
{
    public class Initializer : MonoBehaviour
    {
        public TextAsset level;
        public AudioClip music;
        public GameObject canvas;
        public bool debugUI;

        public bool playOnStart = false;
        public bool editor = false;

        string json = "";
        string ext = "";

        private void Start()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string input = "";
            for (int i = 1; i < args.Length; i++) {
                // first arg is always this executable
                Debug.Log(args[i]);
                if (args[i].IndexOfAny(Path.GetInvalidPathChars()) == -1)
                {
                    if (File.Exists(args[i]))
                    {
                        input = args[i];
                        editor = false;
                        playOnStart = true;
                    }
                }
                else if (args[i] == "-debug")
                {
                    debugUI = true;
                }
            }

            GameObject Cameras = Instantiate(Resources.Load<GameObject>("Prefabs/Cameras")); Cameras.name = "Cameras";
            GameObject MainCamera = Cameras.transform.GetChild(0).gameObject;
            GameObject CursorCamera = Cameras.transform.GetChild(1).gameObject;
            GameObject OverlayCamera = Cameras.transform.GetChild(2).gameObject;
            GameObject GameLetterbox = Cameras.transform.GetChild(3).gameObject;

            GameObject Cursor = Instantiate(Resources.Load<GameObject>("Prefabs/Cursor"));
            Cursor.name = "Cursor";

            GameObject Games = new GameObject();
            Games.name = "Games";

            GameObject GameManager = new GameObject();
            GameManager.name = "GameManager";
            GameManager gameManager = GameManager.AddComponent<GameManager>();
            gameManager.playOnStart = playOnStart;

            gameManager.GamesHolder = Games;
            gameManager.CircleCursor = Cursor.transform.GetChild(0).GetComponent<CircleCursor>();
            gameManager.GameCamera = MainCamera.GetComponent<Camera>();
            gameManager.CursorCam = CursorCamera.GetComponent<Camera>();
            gameManager.OverlayCamera = OverlayCamera.GetComponent<Camera>();
            gameManager.GameLetterbox = GameLetterbox;

            GameObject Profiler = Instantiate(Resources.Load<GameObject>("Prefabs/GameProfiler"));
            Profiler.name = "GameProfiler";
            if (!debugUI)
            {
                Profiler.GetComponent<DebugUI>().enabled = false;
                Profiler.transform.GetChild(0).gameObject.SetActive(false);
            }

            GameObject Conductor = new GameObject();
            Conductor.name = "Conductor";
            AudioSource source = Conductor.AddComponent<AudioSource>();
            source.clip = music;
            Conductor.AddComponent<Conductor>();
            Conductor.GetComponent<Conductor>().musicSource = source;
            source.outputAudioMixerGroup = Settings.GetMusicMixer();
            // Conductor.AddComponent<AudioDspTimeKeeper>();

            if (editor)
            {
                this.GetComponent<HeavenStudio.Editor.Editor>().Init();
            }
            else
            {
                this.GetComponent<HeavenStudio.Editor.Editor>().enabled = false;
                this.GetComponent<HeavenStudio.Editor.EditorTheme>().enabled = false;
                this.GetComponent<HeavenStudio.Editor.BoxSelection>().enabled = false;
                canvas.SetActive(false);

                OpenCmdRemix(input);
                Debug.Log(json);
                gameManager.txt = json;
                gameManager.ext = ext;
                gameManager.Init();
            }
        }

        public void OpenCmdRemix(string path)
        {
            if (path == string.Empty) return;
            if (!File.Exists(path)) return;
            byte[] MusicBytes;
            bool loadedMusic = false;
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
                        json = reader.ReadToEnd();
                        ext = extension;
                        break;
                    }
                    case "song.ogg":
                    {
                        using var stream = entry.Open();
                        using var memoryStream = new MemoryStream();
                        stream.CopyTo(memoryStream);
                        MusicBytes = memoryStream.ToArray();
                        Conductor.instance.musicSource.clip = OggVorbis.VorbisPlugin.ToAudioClip(MusicBytes, "music");
                        loadedMusic = true;
                        break;
                    }
                }

            if (!loadedMusic)
            {
                Conductor.instance.musicSource.clip = null;
                MusicBytes = null;
            }
        }
    }
}