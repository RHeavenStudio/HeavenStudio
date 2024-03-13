using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    public ScreenCaptureManager screenCaptureManager;
    public ImageEffectController imageEffectController;
    public RawImage backgroundImage;
    public void CaptureAndSetBackground()
    {
        Texture2D capturedScreen = screenCaptureManager.CaptureScreen();
        RenderTexture rt = RenderTexture.GetTemporary(capturedScreen.width,
            capturedScreen.height);
        Graphics.Blit(capturedScreen, rt, imageEffectController.effectMaterial);
        Texture2D processedTexture = new Texture2D(capturedScreen.width,
            capturedScreen.height, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        processedTexture.ReadPixels(new Rect(0, 0, capturedScreen.width,
            capturedScreen.height), 0, 0);
        processedTexture.Apply();
        RenderTexture.active = null;
        backgroundImage.texture = processedTexture;
        RenderTexture.ReleaseTemporary(rt);
    }
}

