using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using TMPro;
using FFmpegOut;

namespace HeavenStudio.Editor
{
public class HeavenRecorderDialog : Dialog
{
    public string pathDialog;

    [SerializeField]HeavenRecorderController heavenRecorder;
    [SerializeField]TextMeshProUGUI directoryText;
    [SerializeField]TextMeshProUGUI qualityNum;
    [SerializeField]Slider qualitySlide;
    // Start is called before the first frame update
    void Start()
    {
        UpdateQualityNum();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchTempoDialog()
    {
        if (dialog.activeSelf)
        {
            dialog.SetActive(false);
        }
        else
        {
            ResetAllDialogs();
            dialog.SetActive(true);
        }
    }
    public void SetPathText()
    {
        directoryText.SetText("Directory: " + pathDialog);
    }

    public void UpdateQualityNum()
    {
        qualityNum.SetText(" " + qualitySlide.value.ToString());
        heavenRecorder.SetBitRate((int)qualitySlide.value);
    }
}
}
