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

    public bool autoPlay;
    public bool overlays;

    public TextMeshProUGUI fileSizeEstimate;
    public TextMeshProUGUI lagSpikeText;

    [SerializeField]HeavenRecorderController heavenRecorder;
    [SerializeField]Button getDirectory;
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
    [SerializeField]Color ERRORCOLOR;
    [SerializeField]Color exportButtonColorNotReady;
    [SerializeField]RenderTexture yesOverlayCam;
    [SerializeField]RawImage exportMP4Preview;

    [SerializeField]bool checkPathSet = false;
    [SerializeField]bool checkBeatNumsSet;

    [SerializeField]float startBeatValue;
    [SerializeField]float endBeatValue;

    [SerializeField]GameObject ExportUI;
    [SerializeField]GameObject RecordingUI;
    [SerializeField]GameObject exitButton;

    [SerializeField]Slider progressBar;
    [SerializeField]TextMeshProUGUI progressText;

    // Start is called before the first frame update
    void Start()
    {
        UpdateQualityNum();
        SetFPS();
        autoPlay = true;
        overlays = false;
        heavenRecorder.ChangeOverlayPreview();
        beatNumError.SetText("No Errors! I am okay.");
        FPSError.SetText("No Errors! I am okay.");
        exportMP4Preview.texture = yesOverlayCam; //temporary
    }

    public void OnAutoPlayChange(bool tickOn)
    {
        autoPlay = tickOn;
    }

    public void OnOverlayChange(bool tickOn)
    {
        overlays = tickOn;
        heavenRecorder.ChangeOverlayPreview();
    }

    string BeatError()
    {
        //error #1: beats are equal, so nothing would even be recorded
        if(startBeatValue == endBeatValue)
        {
            checkBeatNumsSet = false;
            beatNumError.color = ERRORCOLOR;
            PreFlightCheckSendOff();
            return "ERROR: START AND END BEAT EQUAL (are you trying to record nothing??)";
        }
        //error #2: endbeat - startbeat = negative number, so the recording would never end
        if(endBeatValue - startBeatValue < 0)
        {
            checkBeatNumsSet = false;
            beatNumError.color = ERRORCOLOR;
            PreFlightCheckSendOff();
            return "ERROR: END BEAT BEHIND START BEAT (might wanna swap them idk)";
        }
        checkBeatNumsSet = true;
        beatNumError.color = Color.white;
        return "No Errors! I am okay.";
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
        directoryText.SetText(pathDialog);
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
        beatNumError.color = Color.white;
        FPSError.SetText("No Errors! I am okay.");
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
        else
        {
            exportButton.interactable = false;
            exportButtonText.color = exportButtonColorNotReady;
        }
    }

    public void RecordingIndicator(bool isRecording)
    {
        if(isRecording)
        {
            RecordingUI.SetActive(true);
            qualitySlide.interactable = false;
            startBeat.interactable = false;
            endBeat.interactable = false;
            FPS.interactable = false;
            getDirectory.interactable = false;
            exitButton.SetActive(false);
            FPSError.SetText("No Errors! I am okay.");
        }
        else
        {
            RecordingUI.SetActive(false);
            exitButton.SetActive(true);
            qualitySlide.interactable = true;
            startBeat.interactable = true;
            endBeat.interactable = true;
            FPS.interactable = true;
            getDirectory.interactable = true;
        }
    }

    public void LagOutError()
    {
        beatNumError.color = ERRORCOLOR;
        FPSError.SetText("LAG OUT ERROR: Set your FPS Lower!! (have mercy on your PC...)");   
    }

    public void ChangeExportButton(bool status)
    {
        if(status)
        {
            exportButtonText.SetText("STOP");
        }
        else
        {
            exportButtonText.SetText("EXPORT");
        }
    }

    public void UpdateProgressBar(float progress)
    {
        progressBar.value = progress;
        progressText.SetText(progress + "% Complete");
    }
}
}
