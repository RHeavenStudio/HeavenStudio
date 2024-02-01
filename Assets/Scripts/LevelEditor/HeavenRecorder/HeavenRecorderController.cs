using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFmpegOut;
using System.IO;

namespace FFmpegOut
{
public class HeavenRecorderController : MonoBehaviour
{
    public CameraCapture recorder;
    public AudioRenderer audioRenderer;
    
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
}
}
