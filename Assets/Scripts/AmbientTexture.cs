using UnityEngine;

namespace HeavenStudio
{
    public class AmbientTexture : MonoBehaviour
    {
        public RenderTexture ambientTex;

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);
            Graphics.Blit(null, ambientTex);
        }
    }
}
