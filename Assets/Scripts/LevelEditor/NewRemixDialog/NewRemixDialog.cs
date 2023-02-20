using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Editor;

public class NewRemixDialog : Dialog
{
    public void SwitchNewDialog()
    {
        if(dialog.activeSelf) {
            dialog.SetActive(false);
        } else {
            ResetAllDialogs();
            dialog.SetActive(true);
        }
    }

    public void Confirm()
    {
        Editor.instance.NewRemix();
        if(dialog.activeSelf) {
            dialog.SetActive(false);
        }
    }
}
