using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

using HeavenStudio.Common;

namespace HeavenStudio
{
    public class GlobalGameManager : MonoBehaviour
    {
        public static GlobalGameManager instance { get; set; }

        public static string buildTime = "00/00/0000 00:00:00";

        public static int loadedScene;
        public int lastLoadedScene;
        public static float fadeDuration;

        public GameObject loadScenePrefab;
        public GameObject hourGlass;

        public static string levelLocation;
        public static bool officialLevel;

        public static int CustomScreenWidth = 1280;
        public static int CustomScreenHeight = 720;

        public static readonly (int width, int height)[] DEFAULT_SCREEN_SIZES = new[] { (1280, 720), (1920, 1080), (2560, 1440), (3840, 2160)};
        public static readonly string[] DEFAULT_SCREEN_SIZES_STRING = new[] { "1280x720", "1920x1080", "2560x1440", "3840x2160", "Custom" };
        public static int ScreenSizeIndex = 0;

        public static float MasterVolume = 0.8f;

        public enum Scenes : int
        {
            SplashScreen = 0,
            Menu = 1,
            Editor = 2,
            Game = 3
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Init()
        {
            BasicCheck();

            loadedScene = 0;
            fadeDuration = 0;

            PersistentDataManager.LoadSettings();

            ScreenSizeIndex = PersistentDataManager.gameSettings.resolutionIndex;
            CustomScreenWidth = PersistentDataManager.gameSettings.resolutionWidth;
            CustomScreenHeight = PersistentDataManager.gameSettings.resolutionHeight;

            ChangeMasterVolume(PersistentDataManager.gameSettings.masterVolume);
            if (PersistentDataManager.gameSettings.isFullscreen)
            {
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
                Screen.fullScreen = true;
            }
            else
            {
                Screen.fullScreen = false;
                ChangeScreenSize();
            }
        }

        public void Awake()
        {
            Init();
            DontDestroyOnLoad(this.gameObject);
            instance = this;
            Starpelly.OS.ChangeWindowTitle("Heaven Studio DEMO");
            QualitySettings.maxQueuedFrames = 1;
            PlayerInput.InitInputControllers();
            #if UNITY_EDITOR
                buildTime = "(EDITOR) " + System.DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm:ss");
            #else
                buildTime = AppInfo.Date.ToString("dd/MM/yyyy hh:mm:ss");
            #endif
        }

        public static GameObject CreateFade()
        {
            GameObject fade = new GameObject();
            DontDestroyOnLoad(fade);
            fade.transform.localScale = new Vector3(4000, 4000);
            SpriteRenderer sr = fade.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Sprites/GeneralPurpose/Square");
            sr.sortingOrder = 20000;
            fade.layer = 5;
            return fade;
        }


        public static void BasicCheck()
        {
            if (FindGGM() == null)
            {
                GameObject GlobalGameManager = new GameObject("GlobalGameManager");
                GlobalGameManager.name = "GlobalGameManager";
                GlobalGameManager.AddComponent<GlobalGameManager>();
            }
        }

        public static GameObject FindGGM()
        {
            if (GameObject.Find("GlobalGameManager") != null)
                return GameObject.Find("GlobalGameManager");
            else
                return null;
        }

        public static void LoadScene(int sceneIndex, float duration = 0.35f)
        {
            print("bruh");
            BasicCheck();
            loadedScene = sceneIndex;
            fadeDuration = duration;

            // DOTween.Clear(true);
            // SceneManager.LoadScene(sceneIndex);

            GameObject fade = CreateFade();
            fade.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
            fade.GetComponent<SpriteRenderer>().DOColor(Color.black, fadeDuration).OnComplete(() => { SceneManager.LoadScene(loadedScene); fade.GetComponent<SpriteRenderer>().DOColor(new Color(0, 0, 0, 0), fadeDuration).OnComplete(() => { Destroy(fade); }); });
        }

        public static void WindowFullScreen()
        {
            Debug.Log("WindowFullScreen");
            if (!Screen.fullScreen)
            {
                // Set the resolution to the display's current resolution
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
                Screen.fullScreen = true;
                PersistentDataManager.gameSettings.isFullscreen = true;
            }
            else
            {
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
                Screen.fullScreen = false;
                PersistentDataManager.gameSettings.isFullscreen = false;
            }
        }

        public static void ChangeScreenSize()
        {
            FullScreenMode mode = Screen.fullScreenMode;
            if (ScreenSizeIndex == DEFAULT_SCREEN_SIZES_STRING.Length - 1)
            {
                Screen.SetResolution(CustomScreenWidth, CustomScreenHeight, mode);
                PersistentDataManager.gameSettings.resolutionWidth = CustomScreenWidth;
                PersistentDataManager.gameSettings.resolutionHeight = CustomScreenHeight;
                PersistentDataManager.gameSettings.resolutionIndex = DEFAULT_SCREEN_SIZES_STRING.Length - 1;
            }
            else
            {
                Screen.SetResolution(DEFAULT_SCREEN_SIZES[ScreenSizeIndex].width, DEFAULT_SCREEN_SIZES[ScreenSizeIndex].height, mode);
                PersistentDataManager.gameSettings.resolutionWidth = DEFAULT_SCREEN_SIZES[ScreenSizeIndex].width;
                PersistentDataManager.gameSettings.resolutionHeight = DEFAULT_SCREEN_SIZES[ScreenSizeIndex].height;
                PersistentDataManager.gameSettings.resolutionIndex = ScreenSizeIndex;
            }
        }

        public static void ChangeMasterVolume(float value)
        {
            MasterVolume = value;
            AudioListener.volume = MasterVolume;
            PersistentDataManager.gameSettings.masterVolume = MasterVolume;
        }

        void OnApplicationQuit()
        {
            PersistentDataManager.SaveSettings();
            Debug.Log("Disconnecting JoyShocks...");
            PlayerInput.DisconnectJoyshocks();
        }
    }
}
