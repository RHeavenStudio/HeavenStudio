using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class Dialog : MonoBehaviour
    {
        [SerializeField] protected GameObject dialog;
        public void ForceState(bool onoff = false)
        {
            Editor.instance.canSelect = !onoff;
            Editor.instance.inAuthorativeMenu = onoff;
            dialog.SetActive(onoff);
        }

        public static void ResetAllDialogs()
        {
            foreach(var dialog in FindObjectsOfType<Dialog>())
            {
                dialog.ForceState(false);
            }
        }
    }
}