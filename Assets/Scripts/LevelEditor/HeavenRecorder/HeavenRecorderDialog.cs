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
    [SerializeField]TextMeshProUGUI beatNumError;
    [SerializeField]TextMeshProUGUI FPSError;
    [SerializeField]TextMeshProUGUI exportButtonText;
    [SerializeField]Slider qualitySlide;
    [SerializeField]TMP_InputField startBeat;
    [SerializeField]TMP_InputField endBeat;
    [SerializeField]TMP_InputField FPS;
    [SerializeField]Button exportButton;
    [SerializeField]Color exportButtonColorReady;

    [SerializeField]bool checkPathSet = false;
    [SerializeField]bool checkBeatNumsSet;

    [SerializeField]float startBeatValue;
    [SerializeField]float endBeatValue;

    [SerializeField]GameObject ExportUI;
    [SerializeField]GameObject RecordingUI;
    [SerializeField]GameObject exitButton;

    // Start is called before the first frame update
    void Start()
    {
        UpdateQualityNum();
        SetFPS();
        beatNumError.SetText("");
        FPSError.SetText("");
    }

    string BeatError()
    {
        //error #1: beats are equal, so nothing would even be recorded
        if(startBeatValue == endBeatValue)
        {
            checkBeatNumsSet = false;
            return "ERROR: START AND END BEAT EQUAL (are you trying to record nothing??)";
        }
        //error #2: endbeat - startbeat = negative number, so the recording would never end
        if(endBeatValue - startBeatValue < 0)
        {
            checkBeatNumsSet = false;
            return "ERROR: END BEAT BEHIND START BEAT (might wanna swap them idk)";
        }
        checkBeatNumsSet = true;
        return "";
    }

    public void SetBeat(bool isStart)
    {
        if(isStart)
        {
            startBeatValue = float.Parse(startBeat.text);
        }
        else
        {
            endBeatValue = float.Parse(endBeat.text);
        }

        if(startBeatValue != null && endBeatValue != null)
        {
            beatNumError.SetText(BeatError());
            PreFlightCheckSendOff();
        }
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
        checkPathSet = true;
        directoryText.SetText("Directory: " + pathDialog);
        PreFlightCheckSendOff();
    }

    public void UpdateQualityNum()
    {
        qualityNum.SetText(" " + qualitySlide.value.ToString());
        heavenRecorder.SetBitRate((int)qualitySlide.value);
    }

    public void SetFPS()
    {
        heavenRecorder.SetCamFPS(float.Parse(FPS.text));
    }

    void PreFlightCheckSendOff()
    {
        //check two bools and send off all of the beat data to heavenrecordercontroller
        if(checkBeatNumsSet && checkPathSet)
        {
            heavenRecorder.SetBeatData(startBeatValue, endBeatValue);
            exportButton.interactable = true;
            exportButtonText.color = exportButtonColorReady;
        }
    }

    public void RecordingIndicator(bool isRecording)
    {
        if(isRecording)
        {
            ExportUI.SetActive(false);
            RecordingUI.SetActive(true);
            exitButton.SetActive(false);
            FPSError.SetText("");
        }
        else
        {
            ExportUI.SetActive(true);
            RecordingUI.SetActive(false);
            exitButton.SetActive(true);
        }
    }

    public void LagOutError()
    {
        FPSError.SetText("LAG OUT ERROR: Set your FPS Lower!! (have mercy on your PC...)");
    }
}
}
