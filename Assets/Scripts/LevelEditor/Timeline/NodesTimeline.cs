using Starpelly;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace HeavenStudio.Editor.Track
{
    public class NodesTimeline : MonoBehaviour
    {
        [SerializeField] private RectTransform _eventsRect;

        [SerializeField] private RectTransform _ppObject;
        [SerializeField] private UILineRenderer _lines;

        private List<NodeEventObj> _ppObjects = new();

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
            GameManager.instance.Beatmap.nodes.Add(new Node(type, time, intensity));
            GameManager.instance.SortEventsList();
        }

        private void Update()
        {
            if (Editor.MouseInRectTransform(_eventsRect))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var mp = new Vector2();
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_eventsRect, Input.mousePosition, Editor.instance.EditorCamera, out mp);

                    CreateNode((NodeType)Editor.instance.currentNodeLayer, Mathp.Round2Nearest(Timeline.instance.MouseTime, Timeline.SnapInterval()), Mathf.Lerp(0, 100, Mathp.Normalize(mp.y, 0, _eventsRect.rect.height)));
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
            _lines.Points[0].y = 0;

            _lines.Points[_lines.Points.Length - 1].x = 100 * multScale;
            _lines.Points[_lines.Points.Length - 1].y = 0;

            for (int i = 1; i < _lines.Points.Length - 1;  i++)
            {
                var node = nodes[i - 1];

                _lines.Points[i].x = node.Beat * multScale;
                _lines.Points[i].y = Mathf.Lerp(0, _eventsRect.rect.height, node.Intensity / 100f);
            }
        }
    }
}
