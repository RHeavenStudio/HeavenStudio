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
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] RenderTexture gameRenderTexture;
        [SerializeField] RenderTexture overlayRenderTexture;

        [SerializeField] HeavenStudio.Editor.Editor editorGO;
        [SerializeField] String debug_cmdFile;

        [SerializeField] GameManager gameManager;

        [SerializeField] GameObject MainCamera;
        [SerializeField] GameObject CursorCamera;
        [SerializeField] GameObject OverlayCamera;
        [SerializeField] GameObject StaticCamera;
        [SerializeField] GameObject Cursor;
        
        [SerializeField] GameObject Profiler;

        public bool debugUI;

        public bool playOnStart = false;
        public bool fromCmd = false;

        string json = "";
        string ext = "";

        private void Start()
        {
            string input = "";
            if (debug_cmdFile != string.Empty)
            {
                if (debug_cmdFile.IndexOfAny(Path.GetInvalidPathChars()) == -1)
                {
                    if (File.Exists(debug_cmdFile))
                    {
                        input = debug_cmdFile;
                        fromCmd = true;
                        playOnStart = true;
                    }
                }
            }
            else if (OpeningManager.OnOpenFile is not null or "")
            {
                if (editorGO == null && OpeningManager.OnOpenFile.IndexOfAny(Path.GetInvalidPathChars()) == -1)
                {
                    if (File.Exists(OpeningManager.OnOpenFile))
                    {
                        input = OpeningManager.OnOpenFile;
                        fromCmd = true;
                        playOnStart = true;
                    }
                }
                OpeningManager.OnOpenFile = null;
            }

            GameObject Games = new GameObject();
            Games.name = "Games";

            gameManager.playOnStart = playOnStart;

            gameManager.GamesHolder = Games;
            gameManager.CircleCursor = Cursor.transform.GetChild(0).GetComponent<CircleCursor>();
            gameManager.GameCamera = MainCamera.GetComponent<Camera>();
            gameManager.CursorCam = CursorCamera.GetComponent<Camera>();
            gameManager.OverlayCamera = OverlayCamera.GetComponent<Camera>();
            gameManager.StaticCamera = StaticCamera.GetComponent<Camera>();

            if (!debugUI && Profiler != null)
            {
                Profiler.GetComponent<DebugUI>().enabled = false;
                Profiler.transform.GetChild(0).gameObject.SetActive(false);
            }

            GameObject Conductor = new GameObject();
            Conductor.name = "Conductor";
            AudioSource source = Conductor.AddComponent<AudioSource>();
            Conductor.AddComponent<Conductor>();
            Conductor.GetComponent<Conductor>().musicSource = source;
            source.outputAudioMixerGroup = Settings.GetMusicMixer();
            // Conductor.AddComponent<AudioDspTimeKeeper>();

            GlobalGameManager.GameRenderTexture = gameRenderTexture;
            GlobalGameManager.OverlayRenderTexture = overlayRenderTexture;
            GlobalGameManager.ResetGameRenderTexture();

            if (editorGO == null)
            {
                OpenCmdRemix(input);
                Debug.Log(json);
                gameManager.txt = json;
                gameManager.ext = ext;
                gameManager.Init();
            }
            else
            {
                editorGO.Init();
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