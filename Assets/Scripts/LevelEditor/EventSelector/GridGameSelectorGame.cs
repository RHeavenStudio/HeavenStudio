using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.Editor
{
    public class GridGameSelectorGame : MonoBehaviour
    {
        public GameObject GameTitlePreview;

        public GridGameSelector GridGameSelector;

        private void Start()
        {
            Tooltip.AddTooltip(this.gameObject, this.gameObject.name);
        }

        public void OnClick()
        {
            GridGameSelector.SelectGame(this.gameObject.name, this.transform.GetSiblingIndex());
        }
    }
}