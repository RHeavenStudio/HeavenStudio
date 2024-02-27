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
    const float MIN_WEIGHT = 0, MAX_WEIGHT = 10, WEIGHT_INTERVAL = 0.1f;
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
            markerWeight.wholeNumbers = false;

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
        markerWeightManual.text = sectionObj.chartEntity["weight"].ToString("0.0");

        markerWeight.maxValue = MAX_WEIGHT;
        markerWeight.minValue = MIN_WEIGHT;
        markerWeight.wholeNumbers = false;

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
        sectionObj.chartEntity["weight"] = RoundNearest(markerWeight.value, WEIGHT_INTERVAL);
        markerWeight.value = sectionObj.chartEntity["weight"];
        markerWeightManual.text = ((float) sectionObj.chartEntity["weight"]).ToString("0.0");
    }

    public void SetSectionWeightManual()
    {
        if (sectionObj == null) return;
        sectionObj.chartEntity["weight"] = RoundNearest((float)Math.Clamp(Convert.ToSingle(markerWeightManual.text), MIN_WEIGHT, MAX_WEIGHT), WEIGHT_INTERVAL);
        markerWeight.value = sectionObj.chartEntity["weight"];
        markerWeightManual.text = ((float) sectionObj.chartEntity["weight"]).ToString("0.0");
    }

    float RoundNearest(float a, float interval)
    {
        int root = Mathf.RoundToInt(a / interval);
        return root * interval;
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
