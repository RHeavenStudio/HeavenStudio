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
        private int SecretCounter = 0;
        private bool SecretActive = false;
        [SerializeField] private TextAsset creditsText;
        [SerializeField] private TMP_Text creditsDisplay;
        [SerializeField] private GameObject secretObject;

        private void Start()
        {
            SecretCounter = 0;
            secretObject.SetActive(false);
        }

        public void OnClickCountUp()
        {
            SecretCounter++;
            Debug.Log("SecretCounter: " + SecretCounter);
            if (SecretCounter == 10)
            {
                secretObject.SetActive(true);
            }
        }

        public void OnClickSecret()
        {
            if (SecretActive) return;

            SecretActive = true;
            Jukebox.PlayOneShot("applause");
            Debug.Log("Activating Studio Dance...");

            Editor.instance.StudioDanceManager.OpenDanceWindow();
        }

        public void MakeSecretInactive()
        {
            SecretCounter = 0;
            secretObject.SetActive(false);
            SecretActive = false;
            Editor.instance.StudioDanceManager.CloseDanceWindow();
        }

        public override void OnOpenTab()
        {
            creditsDisplay.text = creditsText.text;
        }

        public override void OnCloseTab()
        {
        }
    }
}