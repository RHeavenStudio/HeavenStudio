using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using HeavenStudio.Common;

namespace HeavenStudio
{
    public class OpeningManager : MonoBehaviour
    {
        [SerializeField] AudioSource openingAudio;
        [SerializeField] Animator openingAnim;
        [SerializeField] TMP_Text buildText;
        [SerializeField] TMP_Text versionDisclaimer;
        [SerializeField] bool enableSecondDisclaimer;

        bool fastBoot = false;
        float timer = 0;

        void Start()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                // first arg is always this executable
                Debug.Log(args[i]);
                if (args[i].IndexOfAny(Path.GetInvalidPathChars()) == -1)
                {
                    if (File.Exists(args[i]) && (args[i].EndsWith(".riq") || args[i].EndsWith(".tengoku")))
                    {
                        GlobalGameManager.PlayOpenFile = args[i];
                    }
                }
                if (args[i] == "--nosplash")
                {
                    fastBoot = true;
                }
            }

#if UNITY_EDITOR
            buildText.text = "EDITOR";
#else
            buildText.text = Application.buildGUID.Substring(0, 8) + " " + AppInfo.Date.ToString("dd/MM/yyyy hh:mm:ss");
#endif

            if ((Application.platform is RuntimePlatform.OSXPlayer or RuntimePlatform.OSXEditor) || !enableSecondDisclaimer)
            {
                versionDisclaimer.text = "";
            }
            // else
            // {
            //     string ver = "<color=#FFFFCC>If you're coming from an older Heaven Studio build, copy your settings configs over from\n<color=#FFFF00>";
            //     if (Application.platform is RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor)
            //     {
            //         ver += Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\LocalLow\\Megaminerzero\\Heaven Studio\\";
            //         ver += "<color=#FFFFCC>\nto\n<color=#FFFF00>";
            //         ver += Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\LocalLow\\RHeavenStudio\\Heaven Studio\\";
            //     }
            //     else if (Application.platform is RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxEditor)
            //     {
            //         ver += "~/.config/unity3d/Megaminerzero/Heaven Studio/";
            //         ver += "<color=#FFFFCC>\nto\n<color=#FFFF00>";
            //         ver += "~/.config/unity3d/RHeavenStudio/Heaven Studio/";
            //     }
            //     versionDisclaimer.text = ver;
            // }

            if (!GlobalGameManager.IsFirstBoot && !PersistentDataManager.gameSettings.showSplash)
            {
                fastBoot = true;
            }

            if (fastBoot)
            {
                OnFinishDisclaimer(0.1f);
            }
            else
            {
                timer = 0;
                openingAnim.Play("FirstOpening", -1, 0);
                openingAudio.PlayScheduled(AudioSettings.dspTime + 0.5);
                StartCoroutine(WaitAndFinishOpening());
            }
        }

        void Update()
        {
            if (!fastBoot)
            {
                timer += Time.deltaTime;
            }
        }

        IEnumerator WaitAndFinishOpening()
        {
            WaitUntil wait = new WaitUntil(() => CheckForInput() || (timer >= 8));
            yield return wait;
            OnFinishDisclaimer(0.35f);
        }

        bool CheckForInput()
        {
            var controllers = PlayerInput.GetInputControllers();
            foreach (var newController in controllers)
            {
                if (newController.GetLastButtonDown(true) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        void OnFinishDisclaimer(float fadeDuration = 0)
        {
            openingAudio.Stop();
            if (GlobalGameManager.PlayOpenFile is not null or "")
            {
                GlobalGameManager.LoadScene("Game", fadeDuration, 0.5f);
            }
            else
            {
                GlobalGameManager.LoadScene("Title", fadeDuration, fadeDuration);
            }
        }
    }
}