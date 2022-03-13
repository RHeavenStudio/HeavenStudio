using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.Common
{
    public class Parallax : MonoBehaviour
    {
        [SerializeField]
        private Vector2 parallaxEffectMultiplier;

        private Transform camTransform;
        private Vector3 lastCamPos;
        public float textureUnitSizeX;

        public Camera cam;
        public bool sprite = true;

        private void Awake()
        {
            if (cam == null)
                cam = Camera.main;
        }

        private void Start()
        {
            camTransform = cam.transform;
            lastCamPos = camTransform.position;
            /*if (sprite)
            {
                Sprite sprite = GetComponent<SpriteRenderer>().sprite;
                Texture2D texture = sprite.texture;
                textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
            }
            else
            {
                Image image = GetComponent<Image>();
                Texture texture = image.mainTexture;
                textureUnitSizeX = texture.width / image.pixelsPerUnit;
            }*/
        }

        private void LateUpdate()
        {
            Vector3 deltaMovement = camTransform.position - lastCamPos;
            transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier.x, deltaMovement.y * parallaxEffectMultiplier.y, 0);
            lastCamPos = camTransform.position;

            if (Mathf.Abs(camTransform.position.x - transform.position.x) >= textureUnitSizeX)
            {
                float offsetPosX = (camTransform.position.x - transform.position.x) % textureUnitSizeX;
                transform.position = new Vector3(camTransform.position.x + offsetPosX, transform.position.y);
            }
        }
    }
}