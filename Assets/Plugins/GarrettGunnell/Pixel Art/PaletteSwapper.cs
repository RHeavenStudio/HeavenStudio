using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteSwapper : MonoBehaviour {
    public Shader paletteShader;
    
    public Texture colorPalette;
    public bool invert = false;

    private Material paletteMat;
    
    void OnEnable() {
        paletteMat ??= new Material(paletteShader);
        paletteMat.hideFlags = HideFlags.HideAndDontSave;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        paletteMat.SetTexture("_ColorPalette", colorPalette);
        paletteMat.SetInt("_Invert", invert ? 1 : 0);
        Graphics.Blit(source, destination, paletteMat);
    }
}
