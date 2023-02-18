using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

using Starpelly;
using TMPro;

namespace HeavenStudio.Editor.Track
{
    public class PostProcessingTimeline : MonoBehaviour
    {
        private RectTransform _rectTransform;

        [SerializeField] private RectTransform _ppObject;
        [SerializeField] private UILineRenderer _lines;

        public PostProcessVolume volume;
        public Vignette vignette;
        public Bloom bloom;
        public ChromaticAberration chromaticAberration;

        private List<RectTransform> _ppObjects = new List<RectTransform>();

        public UnityEngine.Gradient keyGradient;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();

            _ppObject.gameObject.SetActive(false);

            volume = GameCamera.GetCamera().GetComponent<PostProcessVolume>();

            volume.profile.TryGetSettings(out vignette);
            volume.profile.TryGetSettings(out bloom);
            volume.profile.TryGetSettings(out chromaticAberration);

            keyGradient = new UnityEngine.Gradient();
            var colorKeys = new List<GradientColorKey>();

            colorKeys.Add(new GradientColorKey(EditorTheme.theme.properties.Layer1Col.Hex2RGB(), 0.0f));
            colorKeys.Add(new GradientColorKey(EditorTheme.theme.properties.Layer2Col.Hex2RGB(), 0.25f));
            colorKeys.Add(new GradientColorKey(EditorTheme.theme.properties.Layer3Col.Hex2RGB(), 0.5f));
            colorKeys.Add(new GradientColorKey(EditorTheme.theme.properties.Layer4Col.Hex2RGB(), 0.75f));
            colorKeys.Add(new GradientColorKey(EditorTheme.theme.properties.Layer5Col.Hex2RGB(), 1.0f));

            keyGradient.colorKeys = colorKeys.ToArray();
        }

        private void CreateKeyframe(float beat, float y)
        {
            var obj = Instantiate(_ppObject, _ppObject.parent);
            obj.gameObject.SetActive(true);
            obj.anchoredPosition = new Vector2(beat, y);
            _ppObjects.Add(obj);

            _ppObjects.Sort((x, y) => x.anchoredPosition.x.CompareTo(y.anchoredPosition.x));
        }

        private void Update()
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, Input.mousePosition, Editor.instance.EditorCamera, out mousePos);
            if (Input.GetMouseButtonDown(0) && Timeline.instance.CheckIfMouseInTimeline())
            {
                CreateKeyframe(Mathp.Round2Nearest(mousePos.x, Timeline.SnapInterval()), mousePos.y);
            }

            _lines.Points = new Vector2[_ppObjects.Count + 2];

            var multScale = Timeline.instance.TimelineContent.localScale.x;

            var nextKeyframeBeat = 0.0f;
            var nextKeyframeIntensity = 0.0f;

            var intensity = 0.0f;
            var keyframeBeat = 0.0f;

            _lines.Points[0].x = 0;
            _lines.Points[0].y = -_rectTransform.rect.height;

            _lines.Points[_lines.Points.Length - 1].x = 100 * multScale;
            _lines.Points[_lines.Points.Length - 1].y = -_rectTransform.rect.height;

            for (int i = 1; i < _lines.Points.Length - 1; i++)
            {
                var ia = i - 1;
                var keyframe = _ppObjects[ia];
                var keyframeX = keyframe.anchoredPosition.x;
                var keyframeY = keyframe.anchoredPosition.y;

                _lines.Points[i].x = keyframeX * multScale;
                _lines.Points[i].y = keyframeY;

                var normalized = Mathp.Normalize(keyframeY, -_rectTransform.rect.height, 0);
                keyframe.GetChild(0).GetChild(0).GetComponent<Image>().color = keyGradient.Evaluate(Mathp.NormalizeInvert(keyframeY, -_rectTransform.rect.height, 0));
                keyframe.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = ((int)(normalized * 100f)).ToString();

                if (Conductor.instance.songPositionInBeats >= keyframeX)
                {
                    var nextKeyframe = (i < _ppObjects.Count) ? _ppObjects[i] : keyframe;
                    nextKeyframeBeat = nextKeyframe.anchoredPosition.x;
                    nextKeyframeIntensity = Mathp.Normalize(nextKeyframe.anchoredPosition.y, -220, 0);

                    intensity = normalized;
                    keyframeBeat = keyframeX;
                }
            }
            chromaticAberration.intensity.value = Mathf.Lerp(intensity, nextKeyframeIntensity, Conductor.instance.GetPositionFromBeat(keyframeBeat, nextKeyframeBeat - keyframeBeat)) * 50f;
        }
    }
}
