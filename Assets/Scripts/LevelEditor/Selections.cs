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
            var buggedSelections = eventsSelected.FindAll(c => c == null);
            if (buggedSelections.Count > 0)
            {
                for (int i = 0; i < buggedSelections.Count; i++)
                Deselect(buggedSelections[i]);
            }
            if (Input.GetKey(KeyCode.LeftControl))
                if (Input.GetKeyDown(KeyCode.A))
                    SelectAll();
        }

        public void ClickSelect(TimelineEventObj eventToAdd)
        {
            DeselectAll();
            eventsSelected.Add(eventToAdd);

            // CommandManager.instance.Execute(new Commands.Selection(new List<TimelineEventObj>() { eventToAdd } ));
        }

        public void ShiftClickSelect(TimelineEventObj eventToAdd)
        {
            if (!eventsSelected.Contains(eventToAdd))
            {
                eventsSelected.Add(eventToAdd);
            }
            else
            {
                eventsSelected.Remove(eventToAdd);
            }
        }

        public void DragSelect(TimelineEventObj eventToAdd)
        {
            if (!eventsSelected.Contains(eventToAdd))
            {
                eventsSelected.Add(eventToAdd);
            }
        }

        public void SelectAll()
        {
            DeselectAll();
            var eventObjs = Timeline.instance.eventObjs;
            for (int i = 0; i < eventObjs.Count; i++)
                eventsSelected.Add(eventObjs[i]);
        }

        public void DeselectAll()
        {
            eventsSelected.Clear();
        }

        public void Deselect(TimelineEventObj eventToDeselect)
        {
            if (eventsSelected.Contains(eventToDeselect))
            {
                eventsSelected.Remove(eventToDeselect);
            }
        }
    }
}