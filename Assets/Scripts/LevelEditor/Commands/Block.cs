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
    public class Delete : ICommand
    {
        private List<Guid> toDeleteIds;
        private List<RiqEntityMore> deletedEntities = new();

        struct RiqEntityMore
        {
            public RiqEntity riqEntity;
            public bool selected;
        }

        public Delete(List<Guid> ids)
        {
            toDeleteIds = ids;
        }

        public void Execute()
        {
            for (var i = 0; i < toDeleteIds.Count; i++)
            {
                var entity = GameManager.instance.Beatmap.Entities.Find(c => c.guid == toDeleteIds[i]);
                if (entity != null)
                {
                    var marker = TimelineBlockManager.Instance.EntityMarkers[entity.guid];

                    var clonedEntity = entity.DeepCopy();
                    clonedEntity.guid = entity.guid;

                    deletedEntities.Add(new() { riqEntity = clonedEntity, selected =  marker.selected });

                    TimelineBlockManager.Instance.CreateDestroyFX(entity);

                    Selections.instance.Deselect(marker);

                    GameManager.instance.Beatmap.Entities.Remove(entity);

                    TimelineBlockManager.Instance.EntityMarkers.Remove(entity.guid);
                    GameObject.Destroy(marker.gameObject);
                }
            }

            GameManager.instance.SortEventsList();
        }

        public void Undo()
        {
            for (var i = 0; i < deletedEntities.Count; i++)
            {
                var deletedEntity = deletedEntities[i];
                GameManager.instance.Beatmap.Entities.Add(deletedEntity.riqEntity);
                var marker = TimelineBlockManager.Instance.CreateEntity(deletedEntity.riqEntity);

                /*if (deletedEntities[i].selected)
                    Selections.instance.ShiftClickSelect(marker);*/
            }
            GameManager.instance.SortEventsList();
            deletedEntities.Clear();
        }
    }

    public class Place : ICommand
    {
        private RiqEntity placedEntityData;
        private Guid placedEventID;

        // Redo times basically
        private int placeTimes = 0;

        public Place(RiqEntity entity, Guid placedEventID)
        {
            this.placedEntityData = entity.DeepCopy();
            this.placedEventID = placedEventID;
        }

        public void Execute()
        {
            if (placeTimes > 0)
            {
                var entity = placedEntityData.DeepCopy();
                entity.guid = placedEventID;

                GameManager.instance.Beatmap.Entities.Add(entity);

                var marker = TimelineBlockManager.Instance.CreateEntity(entity);

                GameManager.instance.SortEventsList();
            }
            placeTimes++;
        }

        public void Undo()
        {
            var createdEntity = GameManager.instance.Beatmap.Entities.Find(c => c.guid == placedEventID);
            if (createdEntity != null)
            {
                placedEntityData = createdEntity.DeepCopy();

                if (TimelineBlockManager.Instance.EntityMarkers.ContainsKey(createdEntity.guid))
                {
                    var marker = TimelineBlockManager.Instance.EntityMarkers[createdEntity.guid];
                    Selections.instance.Deselect(marker);

                    TimelineBlockManager.Instance.EntityMarkers.Remove(createdEntity.guid);
                    GameObject.Destroy(marker.gameObject);
                }

                GameManager.instance.Beatmap.Entities.Remove(createdEntity);
                GameManager.instance.SortEventsList();
            }
        }
    }

    public class Duplicate : ICommand
    {
        public List<RiqEntity> dupEntityData = new();
        private readonly List<Guid> placedEntityIDs = new();

        public Duplicate(List<TimelineEventObj> original)
        {
            var entities = original.Select(c => c.entity).ToList();

            foreach (var entity in entities)
            {
                dupEntityData.Add(entity.DeepCopy());
            }

            for (var i = 0; i < original.Count; i++)
            {
                placedEntityIDs.Add(Guid.NewGuid());
            }
        }

        public void Execute()
        {
            var entities = new List<RiqEntity>();
            foreach (var entity in dupEntityData)
            {
                entities.Add(entity.DeepCopy());
            }

            Selections.instance.DeselectAll();

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                entity.guid = placedEntityIDs[i];

                GameManager.instance.Beatmap.Entities.Add(entity);
                var marker = TimelineBlockManager.Instance.CreateEntity(entity);
                Selections.instance.DragSelect(marker);

                if (i == entities.Count - 1)
                    marker.BeginMoving(false);
            }
            GameManager.instance.SortEventsList();
        }

        public void Undo()
        {
            var deletedEntities = new List<RiqEntity>();
            for (var i = 0; i < placedEntityIDs.Count; i++)
            {
                var placedEntityID = placedEntityIDs[i];
                var createdEntity = GameManager.instance.Beatmap.Entities.Find(c => c.guid == placedEntityID);

                if (createdEntity != null)
                {
                    deletedEntities.Add(createdEntity);

                    if (TimelineBlockManager.Instance.EntityMarkers.ContainsKey(placedEntityID))
                    {
                        var marker = TimelineBlockManager.Instance.EntityMarkers[placedEntityID];
                        Selections.instance.Deselect(marker);

                        TimelineBlockManager.Instance.EntityMarkers.Remove(placedEntityID);
                        GameObject.Destroy(marker.gameObject);
                    }

                    GameManager.instance.Beatmap.Entities.Remove(createdEntity);
                }
            }
            GameManager.instance.SortEventsList();
            dupEntityData.Clear();
            foreach (var entity in deletedEntities)
            {
                dupEntityData.Add(entity.DeepCopy());
            }
        }
    }

    public class Paste : ICommand
    {
        private List<RiqEntity> pasteEntityData = new();
        private readonly List<Guid> entityIds = new();

        public Paste(List<RiqEntity> original)
        {
            original.Sort((x, y) => x.beat.CompareTo(y.beat));
            var firstEntityBeat = original[0].beat;
            for (var i = 0; i < original.Count; i++)
            {
                var entity = original[i].DeepCopy();
                entity.beat = Conductor.instance.songPositionInBeatsAsDouble + (entity.beat - firstEntityBeat);
                entityIds.Add(Guid.NewGuid());

                pasteEntityData.Add(entity);
            }
        }

        public void Execute()
        {
            var entities = new List<RiqEntity>();
            foreach (var entity in pasteEntityData)
            {
                entities.Add(entity.DeepCopy());
            }

            Selections.instance.DeselectAll();
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                entity.guid = entityIds[i];

                GameManager.instance.Beatmap.Entities.Add(entity);
                var marker = TimelineBlockManager.Instance.CreateEntity(entity);

                Selections.instance.DragSelect(marker);
            }

            GameManager.instance.SortEventsList();
        }

        public void Undo()
        {
            var deletedEntities = new List<RiqEntity>();
            for (var i = 0; i < entityIds.Count; i++)
            {
                var pastedEntityID = entityIds[i];
                var pastedEntity = GameManager.instance.Beatmap.Entities.Find(c => c.guid == pastedEntityID);

                if (pastedEntity != null)
                {
                    deletedEntities.Add(pastedEntity);

                    if (TimelineBlockManager.Instance.EntityMarkers.ContainsKey(pastedEntityID))
                    {
                        var marker = TimelineBlockManager.Instance.EntityMarkers[pastedEntityID];
                        Selections.instance.Deselect(marker);

                        TimelineBlockManager.Instance.EntityMarkers.Remove(pastedEntityID);
                        GameObject.Destroy(marker.gameObject);
                    }

                    GameManager.instance.Beatmap.Entities.Remove(pastedEntity);
                }
            }
            GameManager.instance.SortEventsList();
            pasteEntityData.Clear();
            foreach (var entity in deletedEntities)
            {
                pasteEntityData.Add(entity.DeepCopy());
            }
        }
    }

    public class Move : ICommand
    {
        private readonly List<Guid> entityIDs = new();
        private EntityMove newMove;
        private EntityMove lastMove;

        private struct EntityMove
        {
            public List<double> beat;
            public List<int> layer;

            public EntityMove(List<double> beat, List<int> layer)
            {
                this.beat = beat;
                this.layer = layer;
            }
        }

        public Move(List<RiqEntity> originalEntities, List<double> newBeat, List<int> newLayer)
        {
            entityIDs = originalEntities.Select(c => c.guid).ToList();
            newMove = new EntityMove(newBeat, newLayer);
        }

        public void Execute()
        {
            lastMove = new EntityMove();
            lastMove.beat = new();
            lastMove.layer = new();

            for (var i = 0; i < entityIDs.Count; i++)
            {
                var entity = GameManager.instance.Beatmap.Entities.Find(c => c.guid == entityIDs[i]);

                lastMove.beat.Add(entity.beat);
                lastMove.layer.Add((int)entity["track"]);

                entity.beat = newMove.beat[i];
                entity["track"] = newMove.layer[i];

                if (TimelineBlockManager.Instance.EntityMarkers.ContainsKey(entity.guid))
                    TimelineBlockManager.Instance.EntityMarkers[entity.guid].SetColor((int)entity["track"]);
            }
        }

        public void Undo()
        {
            for (var i = 0; i < entityIDs.Count; i++)
            {
                var entity = GameManager.instance.Beatmap.Entities.Find(c => c.guid == entityIDs[i]);

                entity.beat = lastMove.beat[i];
                entity["track"] = lastMove.layer[i];

                if (TimelineBlockManager.Instance.EntityMarkers.ContainsKey(entity.guid))
                    TimelineBlockManager.Instance.EntityMarkers[entity.guid].SetColor((int)entity["track"]);
            }
        }
    }

    public class Resize : ICommand
    {
        public Guid entityId;
        private EntityResize newResize;
        private EntityResize lastResize;

        public struct EntityResize
        {
            public double beat;
            public float length;

            public EntityResize(double beat, float length)
            {
                this.beat = beat;
                this.length = length;
            }
        }

        public Resize(Guid entityId, double newBeat, float newLength)
        {
            this.entityId = entityId;
            newResize = new EntityResize(newBeat, newLength);

        }

        public void Execute()
        {
            var entity = GameManager.instance.Beatmap.Entities.Find(c => c.guid == entityId);

            lastResize = new EntityResize(entity.beat, entity.length);

            entity.beat = newResize.beat;
            entity.length = newResize.length;

            if (TimelineBlockManager.Instance.EntityMarkers.ContainsKey(entityId))
                TimelineBlockManager.Instance.EntityMarkers[entityId].SetWidthHeight();
        }

        public void Undo()
        {
            var entity = GameManager.instance.Beatmap.Entities.Find(c => c.guid == entityId);

            entity.beat = lastResize.beat;
            entity.length = lastResize.length;

            if (TimelineBlockManager.Instance.EntityMarkers.ContainsKey(entityId))
                TimelineBlockManager.Instance.EntityMarkers[entityId].SetWidthHeight();
        }
    }
}