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
        public Animator StarAnim;
        bool starActive;

        public GridGameSelector GridGameSelector;

        public Texture MaskTex;
        public Texture BgTex;
        private Material m_Material;

        private void Start()
        {
            Tooltip.AddTooltip(this.gameObject, this.gameObject.name);
        }

        private void OnEnable()
        {
            if (starActive) StarAnim.Play("Appear", 0, 1);
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
            if (Input.GetMouseButtonUp(1)) 
            {
                if (starActive) StarAnim.CrossFade("Disappear", 0.01f);
                else StarAnim.Play("Appear");
                starActive = !starActive;
            } 
            else if (Input.GetMouseButtonUp(0)) 
            {
                GridGameSelector.SelectGame(this.gameObject.name);
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