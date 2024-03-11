using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectSettingsUI : MonoBehaviour
{
    public ImageEffectController imageEffectController;
    public BackgroundManager backgroundManager;
    public Slider impastoSlider;
    public Slider impastoSlider2;
    public Slider blurSlider;
    public Slider vignetteSlider;
    public Slider vignetteSlider2;
    public Button captureBtn;
    private void Start()
    {
        impastoSlider.onValueChanged.AddListener(UpdateImpastoIntensity);
        impastoSlider2.onValueChanged.AddListener(UpdateImpastoNormalIntensity);
        vignetteSlider2.onValueChanged.AddListener(UpdateVignetteSmoothness);
        blurSlider.onValueChanged.AddListener(UpdateBlurIntensity);
        vignetteSlider.onValueChanged.AddListener(UpdateVignetteIntensity);
        captureBtn.onClick.AddListener(() => backgroundManager.CaptureAndSetBackground());
    }
    private void UpdateImpastoIntensity(float value)
    {
        imageEffectController.impastoIntensity = value;
    }
    private void UpdateImpastoNormalIntensity(float value)
    {
        imageEffectController.normalInfluence = value;
    }
    private void UpdateVignetteSmoothness(float value)
    {
        imageEffectController.vignetteSmoothness = value;
    }
    private void UpdateBlurIntensity(float value)
    {
        imageEffectController.blurIntensity = value;
    }
    private void UpdateVignetteIntensity(float value)
    {
        imageEffectController.vignetteIntensity = value;
    }
}

