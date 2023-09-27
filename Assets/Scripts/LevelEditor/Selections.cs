using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HeavenStudio.Editor.Track;

namespace HeavenStudio.Editor
{
    public class Selections : MonoBehaviour
    {
        public List<TimelineEventObj> eventsSelected = new List<TimelineEventObj>();

        public static Selections instance { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            // Should never happen to begin with.
            /*
            var buggedSelections = eventsSelected.FindAll(c => c == null);
            if (buggedSelections.Count > 0)
            {
                for (int i = 0; i < buggedSelections.Count; i++)
                    Deselect(buggedSelections[i]);
            }
            */
            if (Editor.instance.isShortcutsEnabled)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                    if (Input.GetKeyDown(KeyCode.A))
                        SelectAll();
            }
        }

        public void ClickSelect(TimelineEventObj eventToAdd)
        {
            DeselectAll();
            eventsSelected.Add(eventToAdd);
            eventToAdd.selected = true;
            eventToAdd.OnSelect();

            TimelineBlockManager.Instance.SortMarkers();

            // CommandManager.instance.Execute(new Commands.Selection(new List<TimelineEventObj>() { eventToAdd } ));
        }

        public void ShiftClickSelect(TimelineEventObj eventToAdd)
        {
            if (!eventsSelected.Contains(eventToAdd))
            {
                eventToAdd.selected = true;
                eventToAdd.OnSelect();
                eventsSelected.Add(eventToAdd);
            }
            else
            {
                eventToAdd.selected = false;
                eventToAdd.OnDeselect();
                eventsSelected.Remove(eventToAdd);
            }
            TimelineBlockManager.Instance.SortMarkers();
        }

        public void DragSelect(TimelineEventObj eventToAdd)
        {
            if (!eventsSelected.Contains(eventToAdd))
            {
                eventToAdd.selected = true;
                eventsSelected.Add(eventToAdd);
                eventToAdd.OnSelect();
            }
        }

        public void SelectAll()
        {
            return;
            /*
            DeselectAll();
            var eventObjs = Timeline.instance.eventObjs;
            for (int i = 0; i < eventObjs.Count; i++)
            {
                eventObjs[i].selected = true;
                eventObjs[i].OnSelect();

                eventsSelected.Add(eventObjs[i]);
            }
            TimelineBlockManager.Instance.SortMarkers();
            */
        }

        public void DeselectAll()
        {
            foreach (var @event in eventsSelected)
            {
                @event.selected = false;
                @event.OnSelect();
            }
            eventsSelected.Clear();
            TimelineBlockManager.Instance.SortMarkers();
        }

        public void Deselect(TimelineEventObj eventToDeselect)
        {
            if (eventsSelected.Contains(eventToDeselect))
            {
                eventToDeselect.selected = false;
                eventToDeselect.OnDeselect();
                eventsSelected.Remove(eventToDeselect);
            }
            TimelineBlockManager.Instance.SortMarkers();
        }
    }
}