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
        [SerializeField] private GameObject secretObject;
        [SerializeField] private StudioDanceManager secretContent;

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

            secretContent.OpenDanceWindow();
        }

        public void MakeSecretInactive()
        {
            SecretCounter = 0;
            secretObject.SetActive(false);
            SecretActive = false;
            secretContent.CloseDanceWindow();
        }

        public override void OnOpenTab()
        {
        }

        public override void OnCloseTab()
        {
        }
    }
}