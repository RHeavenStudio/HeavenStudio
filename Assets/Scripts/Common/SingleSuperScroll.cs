using UnityEngine;

namespace HeavenStudio.Common
{
    public class SingleSuperScroll : MonoBehaviour
    {
        #region Private

        [SerializeField] Renderer _renderer;
        [SerializeField] Sprite _sprite;

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
            Debug.Log(_renderer.bounds);
        }

        public void LateUpdate()
        {
            _renderer.material.mainTextureScale = Tile;
            _renderer.material.mainTextureOffset = new Vector2(NormalizedX, -NormalizedY) * Tile;

            if (_renderer.material.mainTextureOffset.x >= 0) {

            }

            if (AutoScroll) {
                float songPos = Conductor.instance.songPositionInBeats/100;
                NormalizedX = songPos*AutoScrollX;
                NormalizedY = songPos*AutoScrollY;
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