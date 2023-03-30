using Starpelly;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

namespace HeavenStudio.Editor.Track
{
    public class NodesTimeline : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform _eventsRect;

        [SerializeField] private RectTransform _ppObject;
        [SerializeField] private UILineRenderer _lines;

        private List<NodeEventObj> _nodeObjects = new();

        private int lastNodeLayer;
        private float y0 => (((NodeType)Editor.instance.currentNodeLayer == NodeType.Lens_Distortion) ? _eventsRect.rect.height * 0.5f : 0);
        private float min => (((NodeType)Editor.instance.currentNodeLayer == NodeType.Lens_Distortion) ? -100f : 0f);
        private float max = 100f; // Create a better system for this sometime

        private void Start()
        {
            _ppObject.gameObject.SetActive(false);

            for (int j = 0; j < System.Enum.GetNames(typeof(NodeType)).Length; j++)
            {
            }
            for (int i = 0; i < 12; i++)
            {
                // GameManager.instance.Beatmap.nodes.Add(new Node(NodeType.Pixelize, i + 1, Random.Range(0, 100)));
            }
        }

        private void CreateNode(NodeType type, float time, float intensity)
        {
            var newNode = new Node(type, time, intensity);
            GameManager.instance.Beatmap.nodes.Add(newNode);
            GameManager.instance.SortEventsList();

            CreateNodeObject(newNode);
        }

        private void Update()
        {
            if (Editor.MouseInRectTransform(_eventsRect) && Editor.MouseInRectTransform(Timeline.instance.TimelineScroll.viewport))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    
                }
            }

            var multScale = Timeline.instance.TimelineContent.localScale.x;
            var maxBeat = Conductor.instance.SongLengthInBeats();

            var nodes = GameManager.instance.Beatmap.GetNodesFromType((NodeType)Editor.instance.currentNodeLayer);
            var c = EditorTheme.LayersGradient.Evaluate((float)Editor.instance.currentNodeLayer / System.Enum.GetNames(typeof(NodeType)).Length);
            c.a = 0.5f;
            _lines.color = c;
            _lines.Points = new Vector2[nodes.Count + 2];

            _lines.Points[0].x = 0;
            _lines.Points[0].y = y0;

            _lines.Points[_lines.Points.Length - 1].x = Conductor.instance.SongLengthInBeats() * multScale;
            _lines.Points[_lines.Points.Length - 1].y = y0;

            for (int i = 1; i < _lines.Points.Length - 1;  i++)
            {
                var node = nodes[i - 1];

                _lines.Points[i].x = node.Beat * multScale;
                _lines.Points[i].y = Mathf.LerpUnclamped(y0, _eventsRect.rect.height, node.Intensity / 100f);

                _nodeObjects[i - 1].UpdateNode(c, new Vector2(node.Beat, _lines.Points[i].y));
            }

            if (Editor.instance.currentNodeLayer != lastNodeLayer)
            {
                OnNodeLayerSwitch();
            }
            lastNodeLayer = Editor.instance.currentNodeLayer;
        }

        private void OnNodeLayerSwitch()
        {
            for (int i = 0; i < _nodeObjects.Count; i++)
                Destroy(_nodeObjects[i].gameObject);
            _nodeObjects.Clear();

            CreateNodeObjects((NodeType)Editor.instance.currentNodeLayer);
        }

        private void CreateNodeObjects(NodeType type)
        {
            var nodes = GameManager.instance.Beatmap.GetNodesFromType(type);
            for (int i = 0; i < nodes.Count; i++)
            {
                CreateNodeObject(nodes[i]);
            }
        }

        private void CreateNodeObject(Node node)
        {
            var obj = Instantiate(_ppObject.gameObject, _ppObject.transform.parent);
            obj.SetActive(true);
            var no = obj.GetComponent<NodeEventObj>();
            no.nodeEntity = node;
            no.OnCreate();
            _nodeObjects.Add(no);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var mp = new Vector2();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_eventsRect, Input.mousePosition, Editor.instance.EditorCamera, out mp);

            var intensity = Mathf.Lerp(min, max, Mathp.Normalize(mp.y, 0, _eventsRect.rect.height));
            CreateNode(
                (NodeType)Editor.instance.currentNodeLayer,
                Mathp.Round2Nearest(Timeline.instance.MouseTime, Timeline.SnapInterval()), 
                intensity);
        }
    }
}
