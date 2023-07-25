using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starpelly;

namespace HeavenStudio.Games
{
    public class CallAndResponseHandler
    {
        public struct CallAndResponseEventParam
        {
            public string propertyName;
            public dynamic value;
            public CallAndResponseEventParam(string propertyName, dynamic value)
            {
                this.propertyName = propertyName;
                this.value = value;
            }
        }
        public class CallAndResponseEvent
        {
            public double beat;
            public float length;
            public double relativeBeat; // this beat is relative to the intervalStartBeat
            public Dictionary<string, dynamic> DynamicData; //if you need more properties for your queued event
            public string tag;

            public CallAndResponseEvent(double beat, double relativeBeat, string tag, float length = 0)
            {
                this.beat = beat;
                this.length = length;
                this.relativeBeat = relativeBeat;
                DynamicData = new Dictionary<string, dynamic>();
                this.tag = tag;
                this.length = length;
            }

            public void CreateProperty(string name, dynamic defaultValue)
            {
                if (!DynamicData.ContainsKey(name))
                {
                    DynamicData.Add(name, defaultValue);
                }
            }

            public dynamic this[string propertyName]
            {
                get
                {
                    if (DynamicData.ContainsKey(propertyName))
                    {
                        return DynamicData[propertyName];
                    }
                    else
                    {
                        Debug.LogWarning("This property does not exist on this callAndResponse event.");
                        return null;
                    }
                }
                set
                {
                    if (DynamicData.ContainsKey(propertyName))
                    {
                        DynamicData[propertyName] = value;
                    }
                    else
                    {
                        Debug.LogError($"This callAndRespone event does not have a property named {propertyName}! Attempted to insert value of type {value.GetType()}");
                    }


                }
            }
        }

        public double intervalStartBeat = -1; // the first beat of the interval
        public float intervalLength = -1; // the duration of the interval in beats

        public List<CallAndResponseEvent> queuedEvents = new List<CallAndResponseEvent>();

        /// <summary>
        /// Returns the normalized progress of the interval
        /// </summary>
        public float GetIntervalProgress(float lengthOffset = 0)
        {
            return Conductor.instance.GetPositionFromBeat(intervalStartBeat, intervalLength - lengthOffset);
        }

        public float GetIntervalProgressFromBeat(double beat, float lengthOffset = 0)
        {
            return (float)((beat - intervalStartBeat) / Mathf.Max(1, intervalLength - lengthOffset));
        }

        /// <summary>
        /// Is the interval currently on-going?
        /// </summary>
        public bool IntervalIsActive()
        {
            float progress = GetIntervalProgress();
            return progress >= 0 && progress <= 1;
        }

        /// <summary>
        /// Starts the interval.
        /// </summary>
        /// <param name="beat">The interval start beat.</param>
        /// <param name="length">The length of the interval.</param>
        public void StartInterval(double beat, float length)
        {
            if (queuedEvents.Count > 0) queuedEvents.Clear();
            intervalStartBeat = beat;
            intervalLength = length;
        }
        /// <summary>
        /// Adds an event to the queued events list.
        /// </summary>
        /// <param name="beat">The current beat.</param>
        /// <param name="length">The length of the event.</param>>
        /// <param name="tag">The tag of the event.</param>
        /// <param name="crParams">Extra properties to add to the event.</param>
        public void AddEvent(double beat, float length = 0, string tag = "", List<CallAndResponseEventParam> crParams = null)
        {
            CallAndResponseEvent addedEvent = new(beat, beat - intervalStartBeat, tag, length);
            if (crParams != null && crParams.Count > 0)
            {
                foreach (var param in crParams)
                {
                    addedEvent.CreateProperty(param.propertyName, param.value);
                }
            }
            queuedEvents.Add(addedEvent);
        }

        /// <summary>
        /// Check if an event exists at beat.
        /// </summary>
        /// <param name="beat">The beat to check.</param>
        public bool EventExistsAtBeat(double beat)
        {
            if (queuedEvents.Count == 0)
            {
                return false;
            }
            CallAndResponseEvent foundEvent = queuedEvents.Find(x => x.beat == beat);
            return foundEvent != null;
        }

        /// <summary>
        /// Check if an event exists at relativeBeat.
        /// </summary>
        /// <param name="relativeBeat">The relativeBeat to check.</param>
        public bool EventExistsAtRelativetBeat(double relativeBeat)
        {
            if (queuedEvents.Count == 0)
            {
                return false;
            }
            CallAndResponseEvent foundEvent = queuedEvents.Find(x => x.relativeBeat == relativeBeat);
            return foundEvent != null;
        }
    }

}
