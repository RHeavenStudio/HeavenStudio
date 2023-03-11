using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

using Starpelly;
using TMPro;
using Kino;

namespace HeavenStudio.Editor.Track
{
    public class PostProcessingTimeline : MonoBehaviour
    {
        public static PostProcessingTimeline Instance { get; private set; }

        [SerializeField] private RectTransform _timelineRect;

        [SerializeField] private RectTransform _ppObject;
        [SerializeField] private UILineRenderer _lines;

        private List<NodeEventObj> _ppObjects = new List<NodeEventObj>();

        private int lastLayer;

        private float maxBeat = 320.0f;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _ppObject.gameObject.SetActive(false);
        }

        private void CreateNode(float beat, float intensity, bool create, Nodes.Node node, Nodes.NodeType type = Nodes.NodeType.Chromatic_Aberration)
        {
            var obj = Instantiate(_ppObject, _ppObject.parent).GetComponent<NodeEventObj>();
            obj.rectTransform.gameObject.SetActive(true);
            obj.rectTransform.anchoredPosition = new Vector2(beat, Mathf.Lerp(-250f, 0.0f, intensity / 100.0f));

            if (create)
            {
                var nodes = GameManager.instance.Beatmap.nodes;

                node = new Nodes.Node(type, beat, intensity);
                nodes.Add(node);
                nodes.Sort((x, y) => x.Beat.CompareTo(y.Beat));
            }

            obj.nodeEntity = node;
            _ppObjects.Add(obj);
            _ppObjects.Sort((x, y) => x.nodeEntity.Beat.CompareTo((y.nodeEntity.Beat)));
        }

        private void CreateAllNodes(Nodes.NodeType type)
        {
            var newNodes = GameManager.instance.Beatmap.nodes.FindAll(c => c.Type == type);
            for (int i = 0; i < newNodes.Count; i++)
            {
                CreateNode(newNodes[i].Beat, newNodes[i].Intensity, false, newNodes[i]);
            }
        }

        private void DestroyAllNodes()
        {
            for (int i = 0; i < _ppObjects.Count; i++)
            {
                Destroy(_ppObjects[i].gameObject);
            }
            _ppObjects.Clear();
        }

        private void Update()
        {
            if (lastLayer != Editor.instance.currentNodeLayer)
            {
                DestroyAllNodes();
                CreateAllNodes((Nodes.NodeType)Editor.instance.currentNodeLayer);
            }
            lastLayer = Editor.instance.currentNodeLayer;

            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_timelineRect, Input.mousePosition, Editor.instance.EditorCamera, out mousePos);
            if (Input.GetMouseButtonDown(0) && Timeline.instance.CheckIfMouseInTimeline())
            {
                CreateNode(
                    Mathp.Round2Nearest(mousePos.x, Timeline.SnapInterval()), 
                    Mathp.NormalizeInvert(Mathf.Abs(mousePos.y), 0, _timelineRect.rect.height) * 100.0f,
                    true, null, (Nodes.NodeType)Editor.instance.currentNodeLayer);
            }

            _lines.Points = new Vector2[_ppObjects.Count + 2];

            var multScale = Timeline.instance.TimelineContent.localScale.x;

            _lines.Points[0].x = 0;
            _lines.Points[0].y = -_timelineRect.rect.height;

            _lines.Points[_lines.Points.Length - 1].x = maxBeat * multScale;
            _lines.Points[_lines.Points.Length - 1].y = -_timelineRect.rect.height;


            for (int i = 0; i < _ppObjects.Count; i++)
            {
                _ppObjects[i].UpdateNode(EditorTheme.LayersGradient, _timelineRect);
            }

            for (int i = 1; i < _lines.Points.Length - 1; i++)
            {
                var keyframe = _ppObjects[i - 1];

                _lines.Points[i].x = keyframe.nodeEntity.Beat * multScale;
                _lines.Points[i].y = keyframe.rectTransform.anchoredPosition.y;
            }

            // chromaticAberration.intensity.value = inten * 50.0f;
            // vignette.intensity.value = inten * 0.30f;
            // bloom.intensity.value = inten * 20f;
            // digitalGlitch.intensity = inten;
            // analogGlitch.scanLineJitter = inten * 1.35f;
        }
    }
}
