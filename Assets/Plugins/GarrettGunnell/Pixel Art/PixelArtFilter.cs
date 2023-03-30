using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelArtFilter : MonoBehaviour {
    public Shader pixelArtFilter;

    [Range(0, 8)]
    public float downSamples = 0;

    private Material pixelArtMat;
    
    void OnEnable() {
        pixelArtMat ??= new Material(pixelArtFilter);
        pixelArtMat.hideFlags = HideFlags.HideAndDontSave;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        int width = source.width;
        int height = source.height;

        RenderTexture[] textures = new RenderTexture[8];

        RenderTexture currentSource = source;

        for (int i = 0; i < downSamples; ++i) {
            width /= 2;
            height /= 2;

            if (height < 2)
                break;

            RenderTexture currentDestination = textures[i] = RenderTexture.GetTemporary(width, height, 0, source.format);
            Graphics.Blit(currentSource, currentDestination, pixelArtMat);
            currentSource = currentDestination;
        }

        Graphics.Blit(currentSource, destination, pixelArtMat);

        for (int i = 0; i < downSamples; ++i) {
            RenderTexture.ReleaseTemporary(textures[i]);
        }
    }
}
