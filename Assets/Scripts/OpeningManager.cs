using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace HeavenStudio
{
    public class OpeningManager : MonoBehaviour
    {
        [SerializeField] Animator openingAnim;
        [SerializeField] TMP_Text buildText;

        public static string OnOpenFile;
        bool fastBoot = false;
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
                        OnOpenFile = args[i];
                    }
                }
                if (args[i] == "--fastboot")
                {
                    fastBoot = true;
                }
            }

            #if UNITY_EDITOR
                buildText.text = "EDITOR";
            #else
                buildText.text = Application.buildGUID.Substring(0, 8) + " " + AppInfo.Date.ToString("dd/MM/yyyy hh:mm:ss");
            #endif

            if (!GlobalGameManager.IsFirstBoot)
            {
                fastBoot = true;
            }
            
            if (fastBoot)
            {
                OnFinishDisclaimer(0.1f);
            }
            else
            {
                openingAnim.Play("FirstOpening", -1, 0);
                StartCoroutine(WaitAndFinishOpening());
            }
        }

        IEnumerator WaitAndFinishOpening()
        {
            yield return new WaitForSeconds(3f);
            OnFinishDisclaimer(0.35f);
        }

        void OnFinishDisclaimer(float fadeDuration = 0)
        {
            if (OnOpenFile is not null or "")
            {
                GlobalGameManager.LoadScene("Game", fadeDuration, 0.5f);
            }
            else
            {
                GlobalGameManager.LoadScene("Editor", fadeDuration, fadeDuration);
            }
        }
    }
}