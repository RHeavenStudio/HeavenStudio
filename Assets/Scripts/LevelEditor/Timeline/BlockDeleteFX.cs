using HeavenStudio.Util;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.Editor.Track
{
    public class BlockDeleteFX : MonoBehaviour
    {
        private RectTransform rectTransform;

        [SerializeField]
        private Image mainImage;

        private bool started = false;
        private float deleteTime = 0.0f;

        private double eBeat;
        private float eLength;
        private int eLayer;
        private bool wasESelected = false;

        public void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Create(double beat, float length, int layer, bool selected)
        {
            started = true;
            deleteTime = Time.time;

            eBeat = beat;
            eLength = length;
            eLayer = layer;
            wasESelected = selected;

            Destroy(this.gameObject, 0.714f);

            Update();
        }

        private void Update()
        {
            if (!started) return;

            rectTransform.anchoredPosition = new Vector2((float)eBeat * Timeline.instance.PixelsPerBeat, -eLayer * Timeline.instance.LayerHeight());
            rectTransform.sizeDelta = new Vector2(eLength * Timeline.instance.PixelsPerBeat, Timeline.instance.LayerHeight());


            // I added this because I was going to use the effect when you undo a place as well, but I didn't like it.
            // var color = (wasESelected) ? Color.cyan : EditorTheme.theme.LayerGradientIndex(eLayer);

            var color = EditorTheme.theme.LayerGradientIndex(eLayer);
            var norm = (Time.time - deleteTime) * 1.4f;
            mainImage.color = Color.Lerp(color, new Color(color.r, color.g, color.b, 0.0f), EasingFunction.EaseOutCirc(0, 1, norm));

            var extrudeAnim = EasingFunction.EaseOutCirc(0.0f, 8.0f, norm);
            mainImage.rectTransform.sizeDelta = new Vector2(extrudeAnim, extrudeAnim);
        }
    }
}