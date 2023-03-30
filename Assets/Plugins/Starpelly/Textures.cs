using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Starpelly.Textures
{
    public static class TexturesExtension
    {
        public static Texture2D RenderTextureTo2DTexture(RenderTexture rt)
        {
            var texture = new Texture2D(rt.width, rt.height, rt.graphicsFormat, 0, TextureCreationFlags.None);
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.Apply();

            RenderTexture.active = null;

            return texture;
        }
    }
}