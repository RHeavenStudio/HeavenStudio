using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class PropertiesDialog : MonoBehaviour
    {
        [SerializeField] private GameObject propertiesMenu;
        //this may all be moved to a different script in the future

        private void Start() {}

        public void SwitchPropertiesDialog()
        {
            if(propertiesMenu.activeSelf) {
                propertiesMenu.SetActive(false);
            } else {
                propertiesMenu.SetActive(true);
            }
        }

        private void Update() {}
    }
}