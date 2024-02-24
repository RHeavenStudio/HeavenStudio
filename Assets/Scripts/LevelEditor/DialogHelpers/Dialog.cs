using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;

using TMPro;

namespace HeavenStudio.Editor
{
    public class Dialog : MonoBehaviour
    {
        public bool IsOpen { get { return dialog.activeSelf; } }
        [SerializeField] protected GameObject dialog;
        public void ForceState(bool onoff = false)
        {
            dialog.SetActive(onoff);
            if (Editor.instance == null) return;
            Editor.instance.canSelect = !onoff;
            Editor.instance.inAuthorativeMenu = onoff;
        }

        public static void ResetAllDialogs()
        {
            foreach (var dialog in FindObjectsOfType<Dialog>())
            {
                dialog.ForceState(false);
            }
        }
    }
}