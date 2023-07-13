using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.Common
{
    public class CanvasScroll : MonoBehaviour
    {
        [SerializeField] RawImage[] _imgs;

        public float NormalizedX = 0.0f;
        public float NormalizedY = 0.0f;
        public Vector2 Normalized { get { return new Vector2(NormalizedX, NormalizedY); } set { NormalizedX = value.x; NormalizedY = value.y; } }

        public float TileX = 1.0f;
        public float TileY = 1.0f;
        public Vector2 Tile { get { return new Vector2(TileX, TileY); } set { TileX = value.x; TileY = value.y; } }

        private void Update()
        {
            foreach (var img in _imgs)
            {
                img.uvRect = new Rect(new Vector2(NormalizedX, NormalizedY) * Tile, img.uvRect.size);
            }
        }
    }
}

