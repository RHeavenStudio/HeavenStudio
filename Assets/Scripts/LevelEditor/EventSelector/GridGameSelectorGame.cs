using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using DG.Tweening;

namespace HeavenStudio.Editor
{
    public class GridGameSelectorGame : MonoBehaviour
    {
        public GameObject GameTitlePreview;
        public Animator FavoriteStar;
        public bool StarActive;

        public GridGameSelector GridGameSelector;

        public Texture MaskTex;
        public Texture BgTex;
        private Material m_Material;

        private void Start()
        {
            Tooltip.AddTooltip(this.gameObject, this.gameObject.name);
            Debug.Log(StarActive);
            if (StarActive) FavoriteStar.Play("Appear", -1, 1);
        }

        public void SetupTextures()
        {
            if (m_Material == null)
            {
                m_Material = Instantiate(GetComponent<Image>().material);
                GetComponent<Image>().material = m_Material;
            }
            m_Material.SetTexture("_MaskTex", MaskTex);
            m_Material.SetTexture("_BgTex", BgTex);
        }

        public void OnClick()
        {
            if (Input.GetMouseButtonUp(1)) {
                FavoriteStar.Play(StarActive ? "Disappear" : "Appear");
                StarActive = !StarActive;
            } else if (Input.GetMouseButtonUp(0)) {
                GridGameSelector.SelectGame(this.gameObject.name, this.transform.GetSiblingIndex());
            }
        }

        //TODO: animate between shapes
        public void ClickIcon()
        {
            transform.DOScale(new Vector3(1.15f, 1.15f, 1f), 0.1f);
            BgTex = Resources.Load<Texture>($"Sprites/GeneralPurpose/Circle");
            SetupTextures();
        }

        public void UnClickIcon()
        {
            transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f);
            BgTex = Resources.Load<Texture>($"Sprites/GeneralPurpose/Square");
            SetupTextures();
        }
    }
}