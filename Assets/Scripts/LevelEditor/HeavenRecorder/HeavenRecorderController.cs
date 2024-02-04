using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using FFmpegOut;
using System.IO;
using HeavenStudio.Editor;

namespace FFmpegOut
{
public class HeavenRecorderController : MonoBehaviour
{
    public CameraCapture recorder;
    public AudioRenderer audioRenderer;

    public string pathHeavenRecorder;

    [SerializeField]HeavenRecorderDialog dialog;
    
    bool exporting = true;
    // Start is called before the first frame update
    void Start()
    {
        recorder.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            if(recorder.enabled)
            {
                print("recording stopped!");
                recorder.enabled = false;
                audioRenderer.Rendering = false;
                if(audioRenderer.Save("C:/Users/pikmi/Downloads/temp.wav").State == AudioRenderer.Status.SUCCESS && exporting == true)
                {
                    Merge();
                    audioRenderer.Clear();
                }
            }
            else
            {
                if(File.Exists("C:/Users/pikmi/Downloads/temp.wav"))
                {
                    File.Delete("C:/Users/pikmi/Downloads/temp.wav");
                }
                if(File.Exists("C:/Users/pikmi/Downloads/temp.mp4"))
                {
                    File.Delete("C:/Users/pikmi/Downloads/temp.mp4");
                }
                audioRenderer.Clear();
                audioRenderer.Rendering = true;
                print("recording started!");
                recorder.enabled = true;
                exporting = true;
            }
        }
    }

    void Merge()
    {
        exporting = false;
        FFmpegSession _session;
        _session = FFmpegSession.CreateWithArguments("-y -i C:/Users/pikmi/Downloads/temp.mp4 -i C:/Users/pikmi/Downloads/temp.wav -c:v copy -c:a aac C:/Users/pikmi/Downloads/output.mp4");
        _session.Close();
        _session.Dispose(); 
        _session = null;
        File.Delete("C:/Users/pikmi/Downloads/temp.wav");
        File.Delete("C:/Users/pikmi/Downloads/temp.mp4");
    }

    public void GetUsablePath()
    {
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
        pathHeavenRecorder = pathHeavenRecorder.Replace("\\", "\\\\");
        WritePathToDialogue();
    }

    public void WritePathToDialogue()
    {
        dialog.pathDialog = pathHeavenRecorder;
        dialog.SetPathText();
    }

    public void SetBitRate(int bitrate)
    {
        recorder.bitRate = bitrate;
        // print(recorder.bitRate);
    }
}
}
