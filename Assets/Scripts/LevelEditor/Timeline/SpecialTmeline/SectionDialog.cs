using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using HeavenStudio;
using HeavenStudio.Editor;
using HeavenStudio.Editor.Track;
using TMPro;

public class SectionDialog : Dialog
{
    SectionTimelineObj sectionObj;
    [SerializeField] TMP_InputField sectionName;
    [SerializeField] Toggle challengeEnable;

    public void SwitchSectionDialog()
    {
        if(dialog.activeSelf) {
            sectionObj = null;
            dialog.SetActive(false);
            Editor.instance.inAuthorativeMenu = false;
        } else {
            Editor.instance.inAuthorativeMenu = true;
            ResetAllDialogs();
            dialog.SetActive(true);
        }
    }

    public void SetSectionObj(SectionTimelineObj sectionObj)
    {
        this.sectionObj = sectionObj;
        sectionName.text = sectionObj.chartSection.sectionName;
        challengeEnable.isOn = sectionObj.chartSection.startPerfect;
    }

    public void DeleteSection()
    {
        if(dialog.activeSelf) {
            dialog.SetActive(false);
            Editor.instance.inAuthorativeMenu = false;
        }
        if (sectionObj == null) return;
        GameManager.instance.Beatmap.beatmapSections.Remove(sectionObj.chartSection);
        sectionObj.DeleteObj();
    }

    public void ChangeSectionName(string name)
    {
        if (sectionObj == null) return;
        sectionObj.chartSection.sectionName = name;
        sectionObj.UpdateLabel();
    }

    public void SetSectionChallenge()
    {
        if (sectionObj == null) return;
        sectionObj.chartSection.startPerfect = challengeEnable.isOn;
    }
}
