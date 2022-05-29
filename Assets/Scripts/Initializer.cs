using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace HeavenStudio
{
    public class Initializer : MonoBehaviour
    {
        public TextAsset level;
        public AudioClip music;
        public bool debugUI;

        public bool playOnStart = false;
        public bool editor = false;

        private void Start()
        {
            GameObject Cameras = Instantiate(Resources.Load<GameObject>("Prefabs/Cameras")); Cameras.name = "Cameras";
            GameObject MainCamera = Cameras.transform.GetChild(0).gameObject;
            GameObject CursorCamera = Cameras.transform.GetChild(1).gameObject;
            GameObject OverlayCamera = Cameras.transform.GetChild(2).gameObject;

            GameObject Cursor = Instantiate(Resources.Load<GameObject>("Prefabs/Cursor"));
            Cursor.name = "Cursor";

            GameObject Games = new GameObject();
            Games.name = "Games";

            GameObject GameManager = new GameObject();
            GameManager.name = "GameManager";
            GameManager gameManager = GameManager.AddComponent<GameManager>();
            gameManager.playOnStart = playOnStart;

            gameManager.txt = level;
            gameManager.GamesHolder = Games;
            gameManager.CircleCursor = Cursor.transform.GetChild(0).GetComponent<CircleCursor>();
            gameManager.GameCamera = MainCamera.GetComponent<Camera>();
            gameManager.CursorCam = CursorCamera.GetComponent<Camera>();
            gameManager.OverlayCamera = OverlayCamera.GetComponent<Camera>();

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
                gameManager.Init();
            }
        }
    }
}