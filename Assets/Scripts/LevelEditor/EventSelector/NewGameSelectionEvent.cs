using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HeavenStudio.Editor
{
    public class NewGameSelectionEvent : MonoBehaviour
    {
        [NonSerialized] public int index;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private GameObject _gearIcon;

        public void SetText(string text)
        {
            _text.text = text;
        }

        public void SetTextColor(Color color)
        {
            _text.color = color;
        }

        public void SetActiveGearIcon(bool a)
        {
            _gearIcon.SetActive(a);
        }

        public void Drag()
        {
            if (Conductor.instance.NotStopped() || Editor.instance.inAuthorativeMenu) return;
        }
    }
}

