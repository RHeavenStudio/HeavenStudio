using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

using Jukebox;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using System.Linq;

namespace HeavenStudio.Editor.Track
{
    public class TimelineBlockManager : MonoBehaviour
    {
        public static TimelineBlockManager Instance { get; private set; }

        public TimelineEventObj EntityTemplate;
        public Dictionary<Guid, TimelineEventObj> EntityMarkers = new();
        public ObjectPool<TimelineEventObj> Pool { get; private set; }

        private int firstMarkerToCareAbout = 0;
        private int lastMarkerToCareAbout = 0;
        private Timeline timeline;


        private RiqEntity entityToSet;

        public bool InteractingWithEvents { get; private set; } = false;
        public bool MovingAnyEvents { get; private set; } = false;

        public void SetEntityToSet(RiqEntity entity)
        {
            entityToSet = entity;
        }

        private void Awake()
        {
            Instance = this;

            timeline = GetComponent<Timeline>();

            Pool = new ObjectPool<TimelineEventObj>(CreateMarker, OnTakeMarkerFromPool, OnReturnMarkerToPool, OnDestroyMarker, true, 125, 1500);
        }

        public void Load()
        {
            var timeLeft = timeline.leftSide;
            var timeRight = timeline.rightSide;

            foreach (var marker in EntityMarkers)
            {
                Destroy(marker.Value.gameObject);
            }

            EntityMarkers.Clear();
            Pool.Clear();

            foreach (var entity in GameManager.instance.Beatmap.Entities)
            {
                var vLeft = entity.beat + entity.length >= timeLeft;
                var vRight = entity.beat < timeRight;
                var active = vLeft && vRight;

                if (!active) continue;

                entityToSet = entity;
                Pool.Get();

                Debug.Log(entity.datamodel);
            }
        }

        public TimelineEventObj CreateEntity(RiqEntity entity)
        {
            entityToSet = entity;
            var marker = Pool.Get();
            marker.UpdateMarker();

            return marker;
        }

        public void OnZoom()
        {
            foreach (var marker in EntityMarkers.Values)
            {
                marker.SetWidthHeight();
            }
        }

        public void UpdateMarkers()
        {
            var timeLeft = timeline.leftSide;
            var timeRight = timeline.rightSide;

            var markersActiveBeats = new List<float>();
            foreach (var marker in EntityMarkers.Values)
            {
                if (marker.selected || marker.moving)
                {
                    markersActiveBeats.Add((float)marker.entity.beat);
                }
            }

            for (var i = 0; i < GameManager.instance.Beatmap.Entities.Count; i++)
            {
                var entity = GameManager.instance.Beatmap.Entities[i];

                var vLeft = entity.beat + entity.length >= timeLeft;
                var vRight = entity.beat < timeRight;

                var active = vLeft && vRight;

                var containsMarker = EntityMarkers.ContainsKey(entity.guid);

                if (containsMarker)
                {
                    var marker = EntityMarkers[entity.guid];
                    if (marker.selected || marker.moving) active = true;
                }

                if (active)
                {
                    if (!containsMarker)
                    {
                        entityToSet = entity;
                        Pool.Get();
                    }
                    EntityMarkers[entity.guid].UpdateMarker();
                }
                else
                {
                    if (EntityMarkers.ContainsKey(entity.guid))
                        Pool.Release(EntityMarkers[entity.guid]);
                }
            }

            InteractingWithEvents = false;
            MovingAnyEvents = false;
            foreach (var marker in EntityMarkers.Values)
            {
                if (marker.moving || marker.resizing || marker.mouseHovering)
                    InteractingWithEvents = true;
                if (marker.moving)
                    MovingAnyEvents = true;
            }
        }

        public void SortMarkers()
        {
            // Debug.Log("Sorted timeline blocks.");

            var sortedBlocks = EntityMarkers.Values.OrderByDescending(c => c.entity.length).ToList();

            var i = 0;
            foreach (var block in EntityMarkers.Values)
            {
                var index = sortedBlocks.FindIndex(c => c.entity.guid == block.entity.guid);
                block.transform.SetSiblingIndex(index + 1);

                i++;
            }
        }

        private TimelineEventObj CreateMarker()
        {
            TimelineEventObj marker = Instantiate(EntityTemplate.gameObject, Timeline.instance.TimelineEventsHolder).GetComponent<TimelineEventObj>();
            return marker;
        }

        private void OnTakeMarkerFromPool(TimelineEventObj marker)
        {
            marker.SetEntity(entityToSet);
            marker.SetMarkerInfo();

            SortMarkers();

            marker.gameObject.SetActive(true);
            EntityMarkers.Add(entityToSet.guid, marker);
        }

        private void OnReturnMarkerToPool(TimelineEventObj marker)
        {
            EntityMarkers.Remove(marker.entity.guid);
            marker.gameObject.SetActive(false);
        }

        private void OnDestroyMarker(TimelineEventObj marker)
        {
            Destroy(marker.gameObject);
        }
    }
}