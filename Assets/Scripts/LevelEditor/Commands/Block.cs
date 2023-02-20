using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HeavenStudio.Editor.Track;

namespace HeavenStudio.Editor.Commands
{
    public class Selection : IAction
    {
        List<TimelineEventObj> eventObjs;
        List<TimelineEventObj> lastEventObjs;

        public Selection(List<TimelineEventObj> eventObjs)
        {
            this.eventObjs = eventObjs;
        }

        public void Execute()
        {
        }

        public void Redo()
        {
            for (int i = 0; i < lastEventObjs.Count; i++)
            {
                Selections.instance.ShiftClickSelect(lastEventObjs[i]);
            }
        }

        public void Undo()
        {
            lastEventObjs = eventObjs;
            for (int i = 0; i < eventObjs.Count; i++)
            {
                Selections.instance.ShiftClickSelect(eventObjs[i]);
            }
        }
    }

    // I spent 7 hours trying to fix this instead of sleeping, which would've probably worked better.
    // I'll go fuck myself later I'm just glad it works
    // I give massive props to people who code undo/redo systems
    // -- Starpelly

    public class Move : IAction
    {
        public List<Pos> pos = new List<Pos>();

        public class Pos
        {
            public TimelineEventObj eventObj;

            public Vector2 lastPos_;
            public Vector3 previousPos;
        }

        public Move(List<TimelineEventObj> eventObjs)
        {
            pos.Clear();

            for (int i = 0; i < eventObjs.Count; i++)
            {
                Pos p = new Pos();
                p.eventObj = eventObjs[i];
                p.lastPos_ = eventObjs[i].moveStartPos;
                p.previousPos = eventObjs[i].transform.localPosition;
                this.pos.Add(p);
            }
        }

        public void Execute()
        {
        }

        public void Redo()
        {
            for (int i = 0; i < pos.Count; i++)
            {
                EnsureEventObj(i);
                pos[i].eventObj.transform.localPosition = pos[i].previousPos;
                pos[i].eventObj.entity.beat = pos[i].eventObj.transform.localPosition.x;
            }
        }

        public void Undo()
        {
            for (int i = 0; i < pos.Count; i++)
            {
                EnsureEventObj(i);
                pos[i].eventObj.transform.localPosition = pos[i].lastPos_;
                pos[i].eventObj.entity.beat = pos[i].eventObj.transform.localPosition.x;
            }
        }

        private void EnsureEventObj(int id)
        {
            if (pos[id].eventObj == null)
            {
                pos[id].eventObj = GameManager.instance.Beatmap.entities.Find(c => c.eventObj.eventObjID == pos[id].eventObj.eventObjID).eventObj;
            }
        }
    }

    public class Place : IAction
    {
        TimelineEventObj eventObj;
        TimelineEventObj deletedObj;

        public Place(TimelineEventObj eventObj)
        {
            this.eventObj = eventObj;
        }

        public void Execute()
        {
        }

        public void Redo()
        {
            deletedObj = Timeline.instance.AddEventObject(deletedObj.entity.datamodel, false, new Vector3(deletedObj.entity.beat, -deletedObj.entity.track * Timeline.instance.LayerHeight()), deletedObj.entity, true, deletedObj.entity.eventObj.eventObjID);
        }

        public void Undo()
        {
            deletedObj = eventObj;
            Selections.instance.Deselect(eventObj);
            Timeline.instance.DestroyEventObject(eventObj.entity);
            // DynamicBeatmap.DynamicEntity e = deletedObjs[i].entity;
            // Timeline.instance.AddEventObject(e.datamodel, false, new Vector3(e.beat, -e.track * Timeline.instance.LayerHeight()), e, true, e.eventObj.eventObjID);
        }
    }

    public class Deletion : IAction
    {
        List<TimelineEventObj> eventObjs;
        List<TimelineEventObj> deletedObjs;

        public Deletion(List<TimelineEventObj> eventObjs)
        {
            this.eventObjs = eventObjs;
        }

        public void Execute()
        {
            deletedObjs = eventObjs;
            for (int i = 0; i < eventObjs.Count; i++)
            {
                Selections.instance.Deselect(eventObjs[i]);
                Timeline.instance.DestroyEventObject(eventObjs[i].entity);
            }
        }

        public void Redo()
        {
            deletedObjs = eventObjs;
            for (int i = 0; i < eventObjs.Count; i++)
            {
                Selections.instance.Deselect(eventObjs[i]);
                Timeline.instance.DestroyEventObject(eventObjs[i].entity);
            }
        }

        public void Undo()
        {
            for (int i = 0; i < deletedObjs.Count; i++)
            {
                DynamicBeatmap.DynamicEntity e = deletedObjs[i].entity;
                eventObjs[i] = Timeline.instance.AddEventObject(e.datamodel, false, new Vector3(e.beat, -e.track * Timeline.instance.LayerHeight()), e, true, e.eventObj.eventObjID);
            }
        }
    }

    public class Duplicate : IAction
    {
        List<TimelineEventObj> eventObjs;
        List<TimelineEventObj> copiedObjs;

        public Duplicate(List<TimelineEventObj> eventObjs)
        {
            this.eventObjs = eventObjs;
        }

        public void Execute()
        {
        }

        public void Redo()
        {
            for (int i = 0; i < copiedObjs.Count; i++)
            {
                DynamicBeatmap.DynamicEntity e = copiedObjs[i].entity;
                eventObjs[i] = Timeline.instance.AddEventObject(e.datamodel, false, new Vector3(e.beat, -e.track * Timeline.instance.LayerHeight()), e, true, e.eventObj.eventObjID);
            }
        }

        public void Undo()
        {
            copiedObjs = eventObjs;
            for (int i = 0; i < eventObjs.Count; i++)
            {
                Selections.instance.Deselect(eventObjs[i]);
                Timeline.instance.DestroyEventObject(eventObjs[i].entity);
            }
        }
    }
}