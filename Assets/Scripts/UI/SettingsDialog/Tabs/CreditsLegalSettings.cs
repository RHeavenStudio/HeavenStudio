using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;
using HeavenStudio.Util;
using HeavenStudio.StudioDance;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class CreditsLegalSettings : TabsContent
    {
        private static int SecretCounter = 0;
        private static bool SecretActive = false;
        [SerializeField] private TextAsset creditsText;
        [SerializeField] private TMP_Text creditsDisplay;
        [SerializeField] private GameObject secretObject;

        private void Start()
        {
            SecretActive = false;
            SecretCounter = 0;
            secretObject.SetActive(false);
        }

        public void OnClickCountUp()
        {
            SecretCounter++;
            if (SecretCounter == 10 && Editor.instance != null)
            {
                secretObject.SetActive(true);
            }
        }

        public static void OnClickSecret()
        {
            if (SecretActive) return;

            SecretActive = true;
            SoundByte.PlayOneShot("applause");

            if (Editor.instance == null)
            {

            }
            else
            {
                Editor.instance.StudioDanceManager.OpenDanceWindow();
            }
        }

        public static void MakeSecretInactive()
        {
            SecretCounter = 0;
            SecretActive = false;
        }

        public override void OnOpenTab()
        {
            creditsDisplay.text = creditsText.text;
            if (SecretCounter == 0)
            {
                secretObject.SetActive(false);
            }
        }

        public override void OnCloseTab()
        {
        }
    }
}