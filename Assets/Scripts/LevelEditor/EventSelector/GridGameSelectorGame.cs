using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.Editor
{
    public class GridGameSelectorGame : MonoBehaviour
    {
        public GameObject GameTitlePreview;

        public GridGameSelector GridGameSelector;

        public Texture MaskTex;
        public Texture BgTex;
        private Material m_Material;

        private void Start()
        {
            Tooltip.AddTooltip(this.gameObject, this.gameObject.name);
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
            GridGameSelector.SelectGame(this.gameObject.name, this.transform.GetSiblingIndex());
        }

        public void ClickIcon()
        {
            BgTex = Resources.Load<Texture>($"Sprites/GeneralPurpose/Circle");
            SetupTextures();
        }

        public void UnClickIcon()
        {
            BgTex = Resources.Load<Texture>($"Sprites/GeneralPurpose/Square");
            SetupTextures();
        }
    }
}