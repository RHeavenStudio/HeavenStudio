using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageEffectController : MonoBehaviour
{
    public ScreenCaptureManager screenCaptureManager;
    public Material effectMaterial;
    public float impastoIntensity;
    public float normalInfluence;
    public float blurIntensity;
    public float vignetteIntensity = 0.1357143f;
    public float vignetteSmoothness = 0.3328571f;
#if UNITY_EDITOR
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
#endif

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        effectMaterial.SetFloat("_Radius", impastoIntensity);
        effectMaterial.SetFloat("_NormalWeight", normalInfluence);
        effectMaterial.SetFloat("_BlurSize", blurIntensity);
        effectMaterial.SetFloat("_VignetteIntensity", vignetteIntensity);
        effectMaterial.SetFloat("_VignetteSmoothness", vignetteSmoothness);
        Graphics.Blit(source, destination, effectMaterial);
    }
}