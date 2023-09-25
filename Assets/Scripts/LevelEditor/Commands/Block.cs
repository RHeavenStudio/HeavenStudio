using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Jukebox;

using HeavenStudio.Editor.Track;
using Newtonsoft.Json;
using UnityEditor;

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
                    clonedEntity.guid = entity.guid; // We have to do this because entities (as of when I'm typing this), do not have Guids.

                    deletedEntities.Add(new() { riqEntity = clonedEntity, selected =  marker.selected });


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
        private string placedEventJson;
        private Guid placedEventID;

        // Redo times basically
        private int placeTimes = 0;

        public Place(string placedEventJson, Guid placedEventID)
        {
            this.placedEventJson = placedEventJson;
            this.placedEventID = placedEventID;
        }

        public void Execute()
        {
            if (placeTimes > 0)
            {
                var entity = JsonConvert.DeserializeObject<RiqEntity>(placedEventJson, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.None,
                });
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
                placedEventJson = JsonConvert.SerializeObject(createdEntity, Formatting.None, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.None,
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });

                var marker = TimelineBlockManager.Instance.EntityMarkers[createdEntity.guid];

                Selections.instance.Deselect(marker);

                GameManager.instance.Beatmap.Entities.Remove(createdEntity);

                TimelineBlockManager.Instance.EntityMarkers.Remove(createdEntity.guid);
                GameObject.Destroy(marker.gameObject);

                GameManager.instance.SortEventsList();
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
            }
        }

        public void Undo()
        {
            for (var i = 0; i < entityIDs.Count; i++)
            {
                var entity = GameManager.instance.Beatmap.Entities.Find(c => c.guid == entityIDs[i]);

                entity.beat = lastMove.beat[i];
                entity["track"] = lastMove.layer[i];
                Debug.Log(lastMove.beat[i]);
            }
        }
    }

    public class Duplicate : ICommand
    {
        public string dupEntityJson;
        private readonly List<System.Guid> placedEntityIDs = new();

        public Duplicate(List<RiqEntity> original)
        {
            dupEntityJson = JsonConvert.SerializeObject(original, Formatting.None, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });


        }

        public void Execute()
        {

        }

        public void Undo()
        {

        }
    }
}