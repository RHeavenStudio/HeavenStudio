using UnityEngine;
using System;

namespace HeavenStudio.Common
{
    public class SingleSuperScroll : MonoBehaviour
    {
        #region Private

        [SerializeField] Renderer _renderer;
        [SerializeField] Sprite _sprite;
        private float songSpeed = Conductor.instance.songBpm/100;

        #endregion

        #region Public

        public float NormalizedX = 0.0f;
        public float NormalizedY = 0.0f;
        public Vector2 Normalized { get { return new Vector2(NormalizedX, NormalizedY); } set { NormalizedX = value.x; NormalizedY = value.y; } }
        public bool AutoScroll;
        public float AutoScrollX;
        public float AutoScrollY;

        public float TileX = 1.0f;
        public float TileY = 1.0f;
        public Vector2 Tile { get { return new Vector2(TileX, TileY); } set { TileX = value.x; TileY = value.y; } }

        public Material Material => _renderer.material;

        #endregion

        #region MonoBehaviour

        private void Start()
        {
            _renderer.material = new Material(Shader.Find("Unlit/Transparent"));

            var spriteRect = _sprite.rect;
            var tex = CropTexture(_sprite.texture, new Rect(spriteRect.x, spriteRect.y, spriteRect.width, spriteRect.height));
            tex.wrapMode = TextureWrapMode.Clamp;
            Material.mainTexture = tex;
        }

        public void LateUpdate()
        {
            _renderer.material.mainTextureScale = Tile;
            _renderer.material.mainTextureOffset = new Vector2(NormalizedX, -NormalizedY) * Tile;
            
            //Debug.Log(NormalizedX*10+" is being compared to "+_renderer.transform.localScale.x/2);
            Debug.Log(_sprite.bounds.extents);

            if (NormalizedX*5*TileX >= _renderer.transform.localScale.x/2) {
                NormalizedX = -1;
            }

            
            //if (NormalizedX*5*TileX >= _sprite.bounds) {
            //    NormalizedX = -1;
            //}

            if (AutoScroll && Conductor.instance.NotStopped()) {
                float modifier = Time.deltaTime*songSpeed;
                NormalizedX += modifier*AutoScrollX/TileX;
                NormalizedY += modifier*AutoScrollY/TileY;
            }
        }

        #endregion

        #region Custom

        private Texture2D CropTexture(Texture2D original, Rect rect)
        {
            var colors = original.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            var newTex = new Texture2D((int)rect.width, (int)rect.height);

            newTex.SetPixels(colors);
            newTex.Apply();

            return newTex;
        }

        #endregion
    }
}