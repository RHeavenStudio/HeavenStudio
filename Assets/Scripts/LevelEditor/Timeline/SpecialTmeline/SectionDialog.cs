using System;
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
    const float MIN_WEIGHT = 0, MAX_WEIGHT = 10;
    SectionTimelineObj sectionObj;
    [SerializeField] TMP_InputField sectionName;
    [SerializeField] Toggle challengeEnable;
    [SerializeField] Slider markerWeight;
    [SerializeField] TMP_InputField markerWeightManual;

    [SerializeField] Sprite catOff;
    [SerializeField] Button[] catButtons;
    [SerializeField] Sprite[] catSprites;

    bool initHooks;

    public void SwitchSectionDialog()
    {
        if (dialog.activeSelf)
        {
            sectionObj = null;
            dialog.SetActive(false);
            Editor.instance.inAuthorativeMenu = false;
        }
        else
        {
            Editor.instance.inAuthorativeMenu = true;
            ResetAllDialogs();
            dialog.SetActive(true);

            markerWeight.maxValue = MAX_WEIGHT;
            markerWeight.minValue = MIN_WEIGHT;
            markerWeight.wholeNumbers = true;

            if (!initHooks)
            {
                initHooks = true;
                for (int i = 0; i < catButtons.Length; i++)
                {
                    int cat = i;
                    catButtons[i].onClick.AddListener(() =>
                    {
                        if (sectionObj == null) return;
                        sectionObj.chartEntity["category"] = cat;
                        UpdateCatButtonState();
                    });
                }
            }
        }
    }

    public void SetSectionObj(SectionTimelineObj sectionObj)
    {
        this.sectionObj = sectionObj;
        sectionName.text = sectionObj.chartEntity["sectionName"];
        challengeEnable.isOn = sectionObj.chartEntity["startPerfect"];
        markerWeight.value = sectionObj.chartEntity["weight"];

        markerWeight.maxValue = MAX_WEIGHT;
        markerWeight.minValue = MIN_WEIGHT;
        markerWeight.wholeNumbers = true;

        UpdateCatButtonState();
    }

    public void DeleteSection()
    {
        if (sectionObj != null)
        {
            sectionObj.Remove();
        }
        if (dialog.activeSelf)
        {
            SwitchSectionDialog();
        }
    }

    public void ChangeSectionName()
    {
        if (sectionObj == null) return;
        string name = sectionName.text;
        if (string.IsNullOrWhiteSpace(name)) name = string.Empty;
        sectionObj.chartEntity["sectionName"] = name;
        sectionObj.UpdateLabel();
    }

    public void SetSectionChallenge()
    {
        if (sectionObj == null) return;
        sectionObj.chartEntity["startPerfect"] = challengeEnable.isOn;
    }

    public void SetSectionWeight()
    {
        if (sectionObj == null) return;
        sectionObj.chartEntity["weight"] = markerWeight.value;
        markerWeightManual.text = ((float) sectionObj.chartEntity["weight"]).ToString("G");
    }

    public void SetSectionWeightManual()
    {
        if (sectionObj == null) return;
        sectionObj.chartEntity["weight"] = Mathf.Round((float)Math.Clamp(Convert.ToSingle(markerWeightManual.text), MIN_WEIGHT, MAX_WEIGHT));
        markerWeight.value = sectionObj.chartEntity["weight"];
        markerWeightManual.text = ((float) sectionObj.chartEntity["weight"]).ToString("G");
    }

    void UpdateCatButtonState()
    {
        if (sectionObj == null) return;
        for (int i = 0; i < catButtons.Length; i++)
        {
            if (i == (int) sectionObj.chartEntity["category"])
                catButtons[i].GetComponent<Image>().sprite = catSprites[i];
            else
                catButtons[i].GetComponent<Image>().sprite = catOff;
        }
    }
}
