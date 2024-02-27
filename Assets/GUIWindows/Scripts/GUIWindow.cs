using UnityEngine;

namespace Rellac.Windows
{
    /// <summary>
    /// Simple script to destroy the target GameObject when window is closed
    /// </summary>
    public class GUIWindow : MonoBehaviour
    {
        [SerializeField] private float maxWidth = 0;
        [SerializeField] private float maxHeight = 0;

        /// <summary>
        /// Close window by destroying this GameObject
        /// </summary>
        public void CloseWindow()
        {
            Destroy(gameObject);
        }

        private void Update()
        {
            // limit window size
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (maxWidth > 0 && rectTransform.rect.width > maxWidth)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
            }
            if (maxHeight > 0 && rectTransform.rect.height > maxHeight)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxHeight);
            }

            // keep in bounds of parent
            RectTransform parent = transform.parent.GetComponent<RectTransform>();
            if (parent != null)
            {
                Vector3[] corners = new Vector3[4];
                parent.GetWorldCorners(corners);
                Vector3 min = corners[0];
                Vector3 max = corners[2];

                Vector3[] myCorners = new Vector3[4];
                rectTransform.GetWorldCorners(myCorners);

                if (myCorners[0].x < min.x)
                {
                    rectTransform.localPosition += new Vector3(min.x - myCorners[0].x, 0, 0);
                }
                if (myCorners[2].x > max.x)
                {
                    rectTransform.localPosition -= new Vector3(myCorners[2].x - max.x, 0, 0);
                }
                if (myCorners[0].y < min.y)
                {
                    rectTransform.localPosition += new Vector3(0, min.y - myCorners[0].y, 0);
                }
                if (myCorners[2].y > max.y)
                {
                    rectTransform.localPosition -= new Vector3(0, myCorners[2].y - max.y, 0);
                }

            }
        }
    }
}