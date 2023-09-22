using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

using Jukebox;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace HeavenStudio.Editor.Track
{
    public class TimelineBlockManager : MonoBehaviour
    {
        public static TimelineBlockManager Instance { get; private set; }

        public TimelineEventObj EntityTemplate;
        public Dictionary<int, TimelineEventObj> EntityMarkers = new();
        public ObjectPool<TimelineEventObj> Pool { get; private set; }

        private int firstMarkerToCareAbout = 0;
        private int lastMarkerToCareAbout = 0;
        private Timeline timeline;

        private RiqEntity entityToSet;

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
            }
        }

        public void UpdateMarkers()
        {
            var timeLeft = timeline.leftSide;
            var timeRight = timeline.rightSide;

            for (var i = 0; i < GameManager.instance.Beatmap.Entities.Count; i++)
            {
                var entity = GameManager.instance.Beatmap.Entities[i];

                var vLeft = entity.beat + entity.length >= timeLeft;
                var vRight = entity.beat < timeRight;
                var active = vLeft && vRight;

                var containsMarker = EntityMarkers.ContainsKey(entity.uid);

                if (containsMarker)
                {
                    var marker = EntityMarkers[entity.uid];
                    if (marker.selected || marker.moving) active = true;
                }

                if (active)
                {
                    if (!containsMarker)
                    {
                        entityToSet = entity;
                        Pool.Get();
                    }
                    EntityMarkers[entity.uid].UpdateMarker();
                }
                else
                {
                    if (EntityMarkers.ContainsKey(entity.uid))
                        Pool.Release(EntityMarkers[entity.uid]);
                }
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

            marker.gameObject.SetActive(true);
            EntityMarkers.Add(entityToSet.uid, marker);
        }

        private void OnReturnMarkerToPool(TimelineEventObj marker)
        {
            EntityMarkers.Remove(marker.entity.uid);
            marker.gameObject.SetActive(false);
        }

        private void OnDestroyMarker(TimelineEventObj marker)
        {
            Destroy(marker.gameObject);
        }
    }
}