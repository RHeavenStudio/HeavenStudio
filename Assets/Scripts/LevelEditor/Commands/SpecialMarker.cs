using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Jukebox;

using HeavenStudio.Editor.Track;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine.Timeline;

namespace HeavenStudio.Editor.Commands
{
    public class AddMarker : ICommand
    {
        private RiqEntity placedEntityData;
        private Guid placedEventID;

        private int placedTimes = 0;
        private SpecialTimeline.HoveringTypes type;

        public AddMarker(RiqEntity placedEntityData, Guid placedEventID, SpecialTimeline.HoveringTypes type)
        {
            this.placedEntityData = placedEntityData.DeepCopy();
            this.placedEventID = placedEventID;
            this.type = type;
        }

        public void Execute()
        {
            if (placedTimes > 0)
            {
                var entity = placedEntityData.DeepCopy();
                entity.guid = placedEventID;

                switch (type)
                {
                    case SpecialTimeline.HoveringTypes.TempoChange:
                        GameManager.instance.Beatmap.TempoChanges.Add(entity);
                        SpecialTimeline.instance.AddTempoChange(false, entity);
                        break;
                    case SpecialTimeline.HoveringTypes.VolumeChange:
                        GameManager.instance.Beatmap.VolumeChanges.Add(entity);
                        SpecialTimeline.instance.AddVolumeChange(false, entity);
                        break;
                    case SpecialTimeline.HoveringTypes.SectionChange:
                        GameManager.instance.Beatmap.SectionMarkers.Add(entity);
                        SpecialTimeline.instance.AddChartSection(false, entity);
                        break;
                }

                GameManager.instance.SortEventsList();
            }
            placedTimes++;
        }

        public void Undo()
        {
            RiqEntity createdEntity = null;
            switch (type)
            {
                case SpecialTimeline.HoveringTypes.TempoChange:
                    createdEntity = GameManager.instance.Beatmap.TempoChanges.Find(x => x.guid == placedEventID);
                    break;
                case SpecialTimeline.HoveringTypes.VolumeChange:
                    createdEntity = GameManager.instance.Beatmap.VolumeChanges.Find(x => x.guid == placedEventID);
                    break;
                case SpecialTimeline.HoveringTypes.SectionChange:
                    createdEntity = GameManager.instance.Beatmap.SectionMarkers.Find(x => x.guid == placedEventID);
                    break;
            }
            if (createdEntity != null)
            {
                placedEntityData = createdEntity.DeepCopy();

                var marker = SpecialTimeline.instance.specialTimelineObjs[createdEntity.guid];

                switch (type)
                {
                    case SpecialTimeline.HoveringTypes.TempoChange:
                        GameManager.instance.Beatmap.TempoChanges.Remove(createdEntity);
                        break;
                    case SpecialTimeline.HoveringTypes.VolumeChange:
                        GameManager.instance.Beatmap.VolumeChanges.Remove(createdEntity);
                        break;
                    case SpecialTimeline.HoveringTypes.SectionChange:
                        GameManager.instance.Beatmap.SectionMarkers.Remove(createdEntity);
                        break;
                }

                SpecialTimeline.instance.specialTimelineObjs.Remove(createdEntity.guid);
                GameObject.Destroy(marker.gameObject);

                GameManager.instance.SortEventsList();
            }
        }
    }

    public class DeleteMarker : ICommand
    {
        private RiqEntity deletedEntityData;
        private Guid deletedEventID;
        private SpecialTimeline.HoveringTypes type;

        public DeleteMarker(Guid deletedEventID, SpecialTimeline.HoveringTypes type)
        {
            this.deletedEventID = deletedEventID;
            this.type = type;
        }

        public void Execute()
        {
            RiqEntity deletedEntity = null;
            switch (type)
            {
                case SpecialTimeline.HoveringTypes.TempoChange:
                    deletedEntity = GameManager.instance.Beatmap.TempoChanges.Find(x => x.guid == deletedEventID);
                    break;
                case SpecialTimeline.HoveringTypes.VolumeChange:
                    deletedEntity = GameManager.instance.Beatmap.VolumeChanges.Find(x => x.guid == deletedEventID);
                    break;
                case SpecialTimeline.HoveringTypes.SectionChange:
                    deletedEntity = GameManager.instance.Beatmap.SectionMarkers.Find(x => x.guid == deletedEventID);
                    break;
            }
            if (deletedEntity != null)
            {
                var marker = SpecialTimeline.instance.specialTimelineObjs[deletedEntity.guid];

                deletedEntityData = deletedEntity.DeepCopy();
                deletedEntityData.guid = deletedEntity.guid;

                switch (type)
                {
                    case SpecialTimeline.HoveringTypes.TempoChange:
                        GameManager.instance.Beatmap.TempoChanges.Remove(deletedEntity);
                        break;
                    case SpecialTimeline.HoveringTypes.VolumeChange:
                        GameManager.instance.Beatmap.VolumeChanges.Remove(deletedEntity);
                        break;
                    case SpecialTimeline.HoveringTypes.SectionChange:
                        GameManager.instance.Beatmap.SectionMarkers.Remove(deletedEntity);
                        break;
                }

                SpecialTimeline.instance.specialTimelineObjs.Remove(deletedEntity.guid);
                GameObject.Destroy(marker.gameObject);

                GameManager.instance.SortEventsList();
            }

        }

        public void Undo()
        {
            if (deletedEntityData != null)
            {
                switch (type)
                {
                    case SpecialTimeline.HoveringTypes.TempoChange:
                        GameManager.instance.Beatmap.TempoChanges.Add(deletedEntityData);
                        SpecialTimeline.instance.AddTempoChange(false, deletedEntityData);
                        break;
                    case SpecialTimeline.HoveringTypes.VolumeChange:
                        GameManager.instance.Beatmap.VolumeChanges.Add(deletedEntityData);
                        SpecialTimeline.instance.AddVolumeChange(false, deletedEntityData);
                        break;
                    case SpecialTimeline.HoveringTypes.SectionChange:
                        GameManager.instance.Beatmap.SectionMarkers.Add(deletedEntityData);
                        SpecialTimeline.instance.AddChartSection(false, deletedEntityData);
                        break;
                }
                deletedEntityData = null;
            }
        }
    }

    public class MoveMarker : ICommand
    {
        private Guid entityId;
        private double newBeat, lastBeat;
        private SpecialTimeline.HoveringTypes type;

        public MoveMarker(Guid entityId, double newBeat, SpecialTimeline.HoveringTypes type)
        {
            this.entityId = entityId;
            this.newBeat = newBeat;
            this.type = type;
        }

        public void Execute()
        {
            RiqEntity movedEntity = null;
            switch (type)
            {
                case SpecialTimeline.HoveringTypes.TempoChange:
                    movedEntity = GameManager.instance.Beatmap.TempoChanges.Find(x => x.guid == entityId);
                    break;
                case SpecialTimeline.HoveringTypes.VolumeChange:
                    movedEntity = GameManager.instance.Beatmap.VolumeChanges.Find(x => x.guid == entityId);
                    break;
                case SpecialTimeline.HoveringTypes.SectionChange:
                    movedEntity = GameManager.instance.Beatmap.SectionMarkers.Find(x => x.guid == entityId);
                    break;
            }
            if (movedEntity != null)
            {
                lastBeat = movedEntity.beat;
                movedEntity.beat = newBeat;
            }
        }

        public void Undo()
        {
            RiqEntity movedEntity = null;
            switch (type)
            {
                case SpecialTimeline.HoveringTypes.TempoChange:
                    movedEntity = GameManager.instance.Beatmap.TempoChanges.Find(x => x.guid == entityId);
                    break;
                case SpecialTimeline.HoveringTypes.VolumeChange:
                    movedEntity = GameManager.instance.Beatmap.VolumeChanges.Find(x => x.guid == entityId);
                    break;
                case SpecialTimeline.HoveringTypes.SectionChange:
                    movedEntity = GameManager.instance.Beatmap.SectionMarkers.Find(x => x.guid == entityId);
                    break;
            }
            if (movedEntity != null)
            {
                movedEntity.beat = lastBeat;
            }
        }
    }
}