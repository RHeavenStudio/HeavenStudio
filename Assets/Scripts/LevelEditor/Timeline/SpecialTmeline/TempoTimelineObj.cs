using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using DG.Tweening;

namespace HeavenStudio.Editor.Track
{
    public class TempoTimelineObj : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TMP_Text tempoTXT;
        [SerializeField] private RectTransform raycastRect;

        public DynamicBeatmap.TempoChange tempoChange;

        private float startPosX;
        private bool moving = false;

        public bool hovering;

        private float lastPosX;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            tempoTXT = transform.GetChild(2).GetComponent<TMP_Text>();
            UpdateTempo();
        }

        private void Update()
        {
            if (Timeline.instance.timelineState.tempoChange && !Conductor.instance.NotStopped())
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(raycastRect, Input.mousePosition, Editor.instance.EditorCamera))
                {
                    float newTempo = Input.mouseScrollDelta.y;

                    if (Input.GetKey(KeyCode.LeftShift))
                        newTempo *= 5f;
                    if (Input.GetKey(KeyCode.LeftControl))
                        newTempo /= 100f;

                    tempoChange.tempo += newTempo;

                    //make sure tempo is positive
                    if (tempoChange.tempo < 1)
                        tempoChange.tempo = 1;

                    if (Input.GetMouseButtonDown(0))
                    {
                        Vector3 mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);
                        startPosX = mousePos.x - transform.position.x;
                        moving = true;
                        lastPosX = transform.localPosition.x;
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        GameManager.instance.Beatmap.tempoChanges.Remove(tempoChange);
                        transform.parent.GetComponent<TempoTimeline>().tempoTimelineObjs.Remove(this);
                        Destroy(this.gameObject);
                    }

                    hovering = true;
                }
                else
                {
                    hovering = false;
                }

                if (moving)
                {
                    Vector3 mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);

                    transform.position = new Vector3(mousePos.x - startPosX, transform.position.y, 0);
                    transform.localPosition = new Vector3(Mathf.Clamp(Starpelly.Mathp.Round2Nearest(transform.localPosition.x, Timeline.SnapInterval()), 0, Mathf.Infinity), transform.localPosition.y);
                }
                if (Input.GetMouseButtonUp(0))
                {
                    if (transform.parent.GetComponent<TempoTimeline>().tempoTimelineObjs.Find(c => c.gameObject.transform.localPosition.x == this.transform.localPosition.x && c != this) != null)
                    {
                        transform.localPosition = new Vector3(lastPosX, transform.localPosition.y);
                    }
                    else
                    {
                        tempoChange.beat = transform.localPosition.x;
                    }

                    moving = false;
                    lastPosX = transform.localPosition.x;
                }

                UpdateTempo();
            }
        }

        private void UpdateTempo()
        {
            tempoTXT.text = $"{tempoChange.tempo} BPM";
            Timeline.instance.FitToSong();
        }
    }
}