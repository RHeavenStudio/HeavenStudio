using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using DG.Tweening;

namespace HeavenStudio.Editor.Track
{
    public class VolumeTimelineObj : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TMP_Text volumeTXT;
        [SerializeField] private RectTransform raycastRect;

        public DynamicBeatmap.VolumeChange volumeChange;

        private float startPosX;
        private bool moving = false;

        public bool hovering;

        private float lastPosX;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            volumeTXT = transform.GetChild(2).GetComponent<TMP_Text>();
            UpdateVolume();
        }

        private void Update()
        {
            if (Timeline.instance.timelineState.musicVolume && !Conductor.instance.NotStopped())
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(raycastRect, Input.mousePosition, Editor.instance.EditorCamera))
                {
                    float newVolume = Input.mouseScrollDelta.y;

                    if (Input.GetKey(KeyCode.LeftShift))
                        newVolume *= 5f;
                    if (Input.GetKey(KeyCode.LeftControl))
                        newVolume /= 100f;

                    volumeChange.volume += newVolume;

                    //make sure volume is positive
                    volumeChange.volume = Mathf.Clamp(volumeChange.volume, 0, 100);

                    if (Input.GetMouseButtonDown(0))
                    {
                        Vector3 mousePos = Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition);
                        startPosX = mousePos.x - transform.position.x;
                        moving = true;
                        lastPosX = transform.localPosition.x;
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        GameManager.instance.Beatmap.volumeChanges.Remove(volumeChange);
                        transform.parent.GetComponent<VolumeTimeline>().volumeTimelineObjs.Remove(this);
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
                    if (transform.parent.GetComponent<VolumeTimeline>().volumeTimelineObjs.Find(c => c.gameObject.transform.localPosition.x == this.transform.localPosition.x && c != this) != null)
                    {
                        transform.localPosition = new Vector3(lastPosX, transform.localPosition.y);
                    }
                    else
                    {
                        volumeChange.beat = transform.localPosition.x;
                    }

                    moving = false;
                    lastPosX = transform.localPosition.x;
                }

                UpdateVolume();
            }
        }

        private void UpdateVolume()
        {
            volumeTXT.text = $"{volumeChange.volume}%";
            Timeline.instance.FitToSong();
        }
    }
}