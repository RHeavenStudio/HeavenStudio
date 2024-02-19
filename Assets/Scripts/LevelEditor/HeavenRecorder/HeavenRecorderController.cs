using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FFmpegOut;
using System.IO;
using HeavenStudio.Editor;
using HeavenStudio;
using HeavenStudio.Editor.Track;
using HeavenStudio.Common;

namespace FFmpegOut
{
public class HeavenRecorderController : MonoBehaviour
{
    public CameraCapture recorder;
    public AudioRenderer audioRenderer;

    public string pathHeavenRecorder;
    public string actualDirectory;

    [SerializeField]HeavenRecorderDialog dialog;
    [SerializeField]Timeline timeline;
    [SerializeField]RawImage editorPreviewWindow;
    
    bool exporting = true;
    bool prevOverlay;
    float endBeatRecorder;
    float startBeatRecorder;
    float prevStartBeat;
    float prevVolume;
    int lastFrameDrop;

    public bool enableInput;
    // Start is called before the first frame update
    void Start()
    {
        recorder.enabled = false;
        enableInput = true;
        recorder.width = PersistentDataManager.gameSettings.resolutionWidth;
        recorder.height = PersistentDataManager.gameSettings.resolutionHeight;
    }

    // Update is called once per frame
    void Update()
    {
        if((Conductor.instance.songPositionInBeats > endBeatRecorder))
        {
            StopRecording();
        }
        if(recorder.dropping)
        {
            StopRecording();
            dialog.LagOutError();
            recorder.dropping = false;
        }
        if(audioRenderer.recorderBegin && !recorder.enabled)
        {
            recorder.enabled = true;
            timeline.PlaybackBeat = startBeatRecorder;
            // Conductor.instance.SetBeat(startBeatRecorder);
            timeline.PlayCheck(true);
        }
        if(recorder.enabled)
        {
            if(lastFrameDrop < recorder._frameDropCount)
            {
                dialog.lagSpikeText.SetText("Lag Spikes: " + recorder._frameDropCount + "/5");
            }
            lastFrameDrop = recorder._frameDropCount;
            dialog.UpdateProgressBar(Mathf.Round((Conductor.instance.songPositionInBeats / endBeatRecorder) * 100f));
        }
        // if(Input.GetKeyDown(KeyCode.L))
        // {
        //         //to simulate lag
        //     Application.targetFrameRate = 10;
        // }
    }

    void Merge()
    {
        exporting = false;
        FFmpegSession _session;
        print("-y -i "+ "\"" + pathHeavenRecorder + "\"" + "temp.mp4 -i " +  "\"" + pathHeavenRecorder + "\"" + "temp.wav -filter:a "+ "\"" + "volume=3" + "\"" + "-c:v copy -c:a aac " + "\"" + actualDirectory + "\"");
        _session = FFmpegSession.CreateWithArguments("-y -i "+ "\"" + pathHeavenRecorder + "\"" + "temp.mp4 -i " +  "\"" + pathHeavenRecorder + "\"" + "temp.wav -af silenceremove=1:0:0dB -filter:a "+ "\"" + "volume=3" + "\"" + " -c:v copy -c:a aac " + "\"" + actualDirectory + "\"");
        _session.Close();
        _session.Dispose(); 
        _session = null;
        File.Delete(pathHeavenRecorder + "temp.wav");
        File.Delete(pathHeavenRecorder + "temp.mp4");
    }

    public void GetUsablePath()
    {
        actualDirectory = pathHeavenRecorder;
        actualDirectory = actualDirectory.Replace("\\", "/");
        if(actualDirectory.Contains(".mp4") == false)
        {
            actualDirectory += ".mp4";
        }
        int numberOfSlashes = 0;
        int tempNumberOfSlashes = 0;
        int index = 0;
        int charsAfterSlash = 0;
        //hello guys i am a cs student and this is the most thinking i've had to do so far
        //yo guys i cant even lie if astrl or minenice see this it would so horribly embarassing that i would crawl up into a hole and die just pretend that AI wrote this
        //this implementation is such an eyesore there's prob a better way to implment it but my brain cannot think hard enough
        foreach (char c in pathHeavenRecorder)
        {
            if(c=='\\')
            {
                numberOfSlashes++;
            }
        }
        foreach (char c in pathHeavenRecorder)
        {
            if(c=='\\')
            {
                tempNumberOfSlashes++;
            }
            if(tempNumberOfSlashes == numberOfSlashes)
            {
                charsAfterSlash++;
            }
            else
            {
                index++;
            }
        }
        pathHeavenRecorder = pathHeavenRecorder.Remove(index+1,charsAfterSlash-1);
        pathHeavenRecorder = pathHeavenRecorder.Replace("\\", "/");
        WritePathToDialogue();
        recorder.path = pathHeavenRecorder + "temp.mp4";
    }

    public void WritePathToDialogue()
    {
        dialog.pathDialog = actualDirectory;
        dialog.SetPathText();
    }

    public void SetBitRate(int bitrate)
    {
        recorder.bitRate = bitrate;
        // print(recorder.bitRate);
    }

    public void SetBeatData(float startBeat, float endBeat)
    {
        prevStartBeat = timeline.PlaybackBeat;
        startBeatRecorder = startBeat; 
        endBeatRecorder = endBeat;
    }

    public void BeginRecording()
    {
        prevOverlay = PersistentDataManager.gameSettings.overlaysInEditor;
        audioRenderer.SAMPLE_RATE = PersistentDataManager.gameSettings.sampleRate;
        prevVolume = PersistentDataManager.gameSettings.masterVolume;
        dialog.RecordingIndicator(true);
        GlobalGameManager.ChangeMasterVolume(0.3f);
        editorPreviewWindow.enabled = false;
        if(!recorder.enabled)
        {
            if(File.Exists(pathHeavenRecorder + "temp.wav"))
            {
                File.Delete(pathHeavenRecorder + "temp.wav");
            }
            if(File.Exists(pathHeavenRecorder + "temp.mp4"))
            {
                File.Delete(pathHeavenRecorder + "temp.mp4");
            }
            audioRenderer.Clear();
            audioRenderer.Rendering = true;
            print("recording started!");
            // recorder.enabled = true;
            exporting = true;
            enableInput = false;
            if(dialog.autoPlay)
            {
                timeline.AutoplayBTN.GetComponent<Animator>().Play("Idle", 0, 0);
                GameManager.instance.ToggleAutoplay(true);
            }
            else
            {
                timeline.AutoplayBTN.GetComponent<Animator>().Play("Disabled", 0, 0);
                GameManager.instance.ToggleAutoplay(false);
            }
            ChangeOverlayPreview();
            dialog.ChangeExportButton(true);
        }
    }

    public void ExportButton()
    {
        if(!recorder.enabled)
        {
            dialog.ChangeExportButton(true);
            BeginRecording();
        }
        else
        {
            dialog.ChangeExportButton(false);
            StopRecording();
        }
    }

    public void StopRecording()
    {
        if(recorder.enabled && audioRenderer.Rendering)
            {
                print("recording stopped!");
                audioRenderer.Rendering = false;
                if(audioRenderer.Save(pathHeavenRecorder + "temp.wav").State == AudioRenderer.Status.SUCCESS && exporting == true)
                {
                    audioRenderer.recorderBegin = false;
                    recorder.enabled = false;
                    Merge();
                    audioRenderer.Clear();
                }
                timeline.PlaybackBeat = prevStartBeat;
                timeline.PlayCheck(true);
                // Conductor.instance.SetBeat(prevStartBeat);
                dialog.RecordingIndicator(false);
                GlobalGameManager.ChangeMasterVolume(prevVolume);
                enableInput = true;
                dialog.ChangeExportButton(false);
                dialog.lagSpikeText.SetText("Lag Spikes: 0/5");
                dialog.UpdateProgressBar(0);
                editorPreviewWindow.enabled = true;
                PersistentDataManager.gameSettings.overlaysInEditor = prevOverlay;
            }
    }

    public void ChangeOverlayPreview()
    {
        if(dialog.overlays)
        {
            PersistentDataManager.gameSettings.overlaysInEditor = true;
        }
        else
        {
            PersistentDataManager.gameSettings.overlaysInEditor = false;
        }
    }

    public void SetCamFPS(float fps)
    {
        recorder.frameRate = fps;
    }
}
}
