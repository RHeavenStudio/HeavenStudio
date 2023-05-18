using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] private Button createButton;

        [SerializeField] private float Bpm = 114f;
        private void Start()
        {
            createButton.onClick.AddListener(delegate { GlobalGameManager.LoadScene("Editor"); Jukebox.PlayOneShot("ui/UIEnter"); });
        }
    }
}

