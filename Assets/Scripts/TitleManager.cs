using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace HeavenStudio
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] Button createButton;
        private void Start()
        {
            createButton.onClick.AddListener(delegate { GlobalGameManager.LoadScene("Editor"); });
        }
    }
}

