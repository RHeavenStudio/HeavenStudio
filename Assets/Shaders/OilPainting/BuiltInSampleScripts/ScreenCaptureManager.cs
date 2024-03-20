using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCaptureManager : MonoBehaviour
{
    public Texture2D CaptureScreen()
    {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        Camera.main.targetTexture = rt;
        Camera.main.Render();
        Camera.main.targetTexture = null;
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height,
            TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();
        RenderTexture.active = null;
        Destroy(rt);
        return screenShot;
    }
}

