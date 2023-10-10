﻿using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

using HeavenStudio.Common;
using System.Runtime.InteropServices;

namespace HeavenStudio
{
    public class GlobalGameManager : MonoBehaviour
    {
        public static GlobalGameManager instance { get; set; }

        [Header("Loading Screen")]
        [SerializeField] Image fadeImage;
        [SerializeField] TMP_Text loadingText;

        [Header("Dialog Box")]
        [SerializeField] GameObject messagePanel;
        [SerializeField] TMP_Text messageHeader;
        [SerializeField] TMP_Text messageBody;
        [SerializeField] TMP_Text errorBuild;
        [SerializeField] Button errorOkButton;
        [SerializeField] Button errorLogButton;
        [SerializeField] Slider dialogProgress;
        public static bool IsShowingDialog;

        public static string buildTime = "00/00/0000 00:00:00";

        public static bool discordDuringTesting = false;

        static string loadedScene;
        static string lastLoadedScene;
        static AsyncOperation asyncLoad;

        public static string levelLocation;
        public static bool officialLevel;

        public static bool IsFirstBoot = false;

        public static int CustomScreenWidth = 1280;
        public static int CustomScreenHeight = 720;

        public static readonly (int width, int height)[] DEFAULT_SCREEN_SIZES = new[] { (1280, 720), (1920, 1080), (2560, 1440), (3840, 2160)};
        public static readonly string[] DEFAULT_SCREEN_SIZES_STRING = new[] { "1280x720", "1920x1080", "2560x1440", "3840x2160", "Custom" };
        public static int ScreenSizeIndex = 0;

        public static float MasterVolume = 0.8f;
        public static int currentDspSize = 512;
        public static int currentSampleRate = 44100;
        public static readonly int[] DSP_BUFFER_SIZES =
        {
            128, 256, 340, 480, 512, 1024
        };

        public static readonly int[] SAMPLE_RATES =
        {
            22050, 44100, 48000, 88200, 96000,
        };

        public static RenderTexture GameRenderTexture;
        public static RenderTexture OverlayRenderTexture;

        public enum Scenes : int
        {
            SplashScreen = 0,
            Menu = 1,
            Editor = 2,
            Game = 3
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            Application.wantsToQuit += WantsToQuit;

            BasicCheck();

            Minigames.InitPreprocessor();
            loadedScene = SceneManager.GetActiveScene().name;

            PersistentDataManager.LoadSettings();

            ScreenSizeIndex = PersistentDataManager.gameSettings.resolutionIndex;
            CustomScreenWidth = PersistentDataManager.gameSettings.resolutionWidth;
            CustomScreenHeight = PersistentDataManager.gameSettings.resolutionHeight;

            ChangeMasterVolume(PersistentDataManager.gameSettings.masterVolume);
            
            if (PersistentDataManager.gameSettings.dspSize == 0)
                PersistentDataManager.gameSettings.dspSize = 512;
            if (PersistentDataManager.gameSettings.sampleRate == 0)
                PersistentDataManager.gameSettings.sampleRate = 44100;
            currentDspSize = PersistentDataManager.gameSettings.dspSize;
            currentSampleRate = PersistentDataManager.gameSettings.sampleRate;

            ChangeAudioSettings(currentDspSize, currentSampleRate);

            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            QualitySettings.maxQueuedFrames = 1;
            if (PersistentDataManager.gameSettings.isFullscreen)
            {
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.ExclusiveFullScreen);
                Screen.fullScreen = true;
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.fullScreen = false;
                ChangeScreenSize();
            }
            PlayerInput.InitInputControllers();
            #if UNITY_EDITOR
                Starpelly.OS.ChangeWindowTitle("Heaven Studio UNITYEDITOR ");
                buildTime = "(EDITOR) " + System.DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm:ss");
            #else
                Starpelly.OS.ChangeWindowTitle("Heaven Studio (INDEV) " + Application.buildGUID.Substring(0, 8));
                buildTime = Application.buildGUID.Substring(0, 8) + " " + AppInfo.Date.ToString("dd/MM/yyyy hh:mm:ss");
            #endif          
        }

        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
            fadeImage.gameObject.SetActive(false);
            loadingText.enabled = false;

            messagePanel.SetActive(false);
            IsShowingDialog = false;
        }

        private void Update() 
        {
            PlayerInput.UpdateInputControllers();
        }

        IEnumerator LoadSceneAsync(string scene, float fadeOut)
        {
            //TODO: create flow mem loading icon
            asyncLoad = SceneManager.LoadSceneAsync(scene);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            //TODO: fade out flow mem loading icon
            instance.fadeImage.DOKill();
            instance.loadingText.enabled = false;
            instance.fadeImage.DOFade(0, fadeOut).OnComplete(() =>
            {
                instance.fadeImage.gameObject.SetActive(false);
            });
        }

        IEnumerator ForceFadeAsync(float hold, float fadeOut)
        {
            yield return new WaitForSeconds(hold);
            instance.fadeImage.DOKill();
            instance.loadingText.enabled = false;
            instance.fadeImage.DOFade(0, fadeOut).OnComplete(() =>
            {
                instance.fadeImage.gameObject.SetActive(false);
            });
        }

        public void HideDialog()
        {
            Debug.Log("Hiding dialog");
            messagePanel.SetActive(false);
            IsShowingDialog = false;
        }

        public void OpenLogFolder()
        {
            // TODO
        }

        public static void BasicCheck()
        {
            if (FindGGM() == null)
            {
                // load the global game manager prefab
                GameObject ggm = Instantiate(Resources.Load("Prefabs/GlobalGameManager") as GameObject);
                DontDestroyOnLoad(ggm);
            }
        }

        public static GameObject FindGGM()
        {
            if (instance != null)
                return instance.gameObject;
            else
                return null;
        }

        public static void LoadScene(string scene, float fadeIn = 0.35f, float fadeOut = 0.35f)
        {
            if (scene == loadedScene)
                return;
            lastLoadedScene = loadedScene;
            loadedScene = scene;

            instance.fadeImage.DOKill();
            instance.fadeImage.gameObject.SetActive(true);
            instance.fadeImage.color = new Color(0, 0, 0, 0);
            instance.fadeImage.DOFade(1, fadeIn).OnComplete(() =>
            {
                instance.StartCoroutine(instance.LoadSceneAsync(scene, fadeOut));
                instance.loadingText.enabled = true;
            });
        }

        public static void ForceFade(float fadeIn, float hold, float fadeOut)
        {
            instance.fadeImage.DOKill();
            instance.fadeImage.gameObject.SetActive(true);
            instance.fadeImage.color = new Color(0, 0, 0, 0);
            instance.loadingText.enabled = false;
            instance.fadeImage.DOFade(1, fadeIn).OnComplete(() =>
            {
                instance.StartCoroutine(instance.ForceFadeAsync(hold, fadeOut));
            });
        }

        public static void ShowErrorMessage(string header, string message)
        {
            IsShowingDialog = true;
            if (Conductor.instance != null && Conductor.instance.isPlaying)
                Conductor.instance.Pause();

            instance.messageHeader.text = header;
            instance.messageBody.text = message;
            instance.errorOkButton.gameObject.SetActive(true);
            // instance.errorLogButton.gameObject.SetActive(true);
            instance.dialogProgress.gameObject.SetActive(false);

            instance.errorBuild.gameObject.SetActive(true);
            #if UNITY_EDITOR
                instance.errorBuild.text = "(EDITOR) " + System.DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm:ss");
            #else
                instance.errorBuild.text = Application.buildGUID.Substring(0, 8) + " " + AppInfo.Date.ToString("dd/MM/yyyy hh:mm:ss");
            #endif

            instance.messagePanel.SetActive(true);
        }

        public static void ShowLoadingMessage(string header, string message, float progress = -1)
        {
            Debug.Log("ShowLoadingMessage");
            IsShowingDialog = true;
            if (Conductor.instance != null && Conductor.instance.isPlaying)
                Conductor.instance.Pause();

            instance.messageHeader.text = header;
            instance.messageBody.text = message;
            instance.errorOkButton.gameObject.SetActive(false);
            instance.errorBuild.gameObject.SetActive(false);
            // instance.errorLogButton.gameObject.SetActive(false);
            if (progress >= 0)
            {
                instance.dialogProgress.gameObject.SetActive(true);
                instance.dialogProgress.value = progress;
            }
            instance.messagePanel.SetActive(true);
        }

        public static void SetLoadingMessageProgress(float progress)
        {
            if (IsShowingDialog)
            {
                instance.dialogProgress.gameObject.SetActive(true);
                instance.dialogProgress.value = progress;
            }
        }

        public static void WindowFullScreen()
        {
            if (!Screen.fullScreen)
            {
                // Set the resolution to the display's current resolution
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.ExclusiveFullScreen);
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

        public static void ResetGameRenderTexture()
        {
            // keep 16:9 aspect ratio
            int width = Screen.width;
            int height = Screen.height;
            if (width / 16f > height / 9f)
            {
                width = (int)(height / 9f * 16f);
            }
            else
            {
                height = (int)(width / 16f * 9f);
            }

            GameRenderTexture.Release();

            GameRenderTexture.width = width;
            GameRenderTexture.height = height;

            GameRenderTexture.Create();


            OverlayRenderTexture.Release();

            OverlayRenderTexture.width = (int)(width * 1.5f);
            OverlayRenderTexture.height = (int)(height * 1.5f);

            OverlayRenderTexture.Create();
        }

        public static void ChangeMasterVolume(float value)
        {
            MasterVolume = value;
            AudioListener.volume = MasterVolume;
        }

        public static void ChangeAudioSettings(int dspSize, int sampleRate)
        {
            // don't reset audio if no changes are done
            AudioConfiguration config = AudioSettings.GetConfiguration();
            if (dspSize == config.dspBufferSize && sampleRate == config.sampleRate) return;
            currentDspSize = dspSize;
            currentSampleRate = sampleRate;

            config.dspBufferSize = currentDspSize;
            config.sampleRate = currentSampleRate;
            AudioSettings.Reset(config);

            PersistentDataManager.gameSettings.dspSize = currentDspSize;
            PersistentDataManager.gameSettings.sampleRate = currentSampleRate;
        }

        public static void UpdateDiscordStatus(string details, bool editor = false, bool updateTime = false)
        {
            if (discordDuringTesting || !Application.isEditor)
            {
                if (PersistentDataManager.gameSettings.discordRPCEnable)
                {   
                    DiscordRPC.DiscordRPC.UpdateActivity(editor ? "In Editor " : "Playing ", details, updateTime);
                    Debug.Log("Discord status updated");
                }
            }
        }

        void OnApplicationQuit()
        {
            Debug.Log("Disconnecting JoyShocks...");
            PlayerInput.CleanUp();
            Debug.Log("Clearing RIQ Cache...");
            Jukebox.RiqFileHandler.ClearCache();
        }

        public static bool ShowQuitPopUp = true;

        private static bool WantsToQuit()
        {
            if (ShowQuitPopUp)
            {
                if (SceneManager.GetActiveScene().name != "Editor") return true;


            }
            return true;
        }
    }
}
