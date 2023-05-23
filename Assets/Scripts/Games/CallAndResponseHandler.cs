using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            public float beat;
            public float relativeBeat; // this beat is relative to the intervalStartBeat
            public Dictionary<string, dynamic> DynamicData; //if you need more properties for your queued event

            public CallAndResponseEvent(float beat, float relativeBeat)
            {
                this.beat = beat;
                this.relativeBeat = relativeBeat;
                DynamicData = new Dictionary<string, dynamic>();
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

        private float intervalStartBeat = -1; // the first beat of the interval
        private float intervalLength = -1; // the duration of the interval in beats

        public float defaultIntervalLength; // when an event is queued and the interval has not started yet, it will use this as the interval length.

        public CallAndResponseHandler(float defaultIntervalLength)
        {
            this.defaultIntervalLength = defaultIntervalLength;
        }

        private List<CallAndResponseEvent> queuedEvents = new List<CallAndResponseEvent>();

        /// <summary>
        /// Returns the normalized progress of the interval
        /// </summary>
        public float GetIntervalProgress()
        {
            return Conductor.instance.GetPositionFromBeat(intervalStartBeat, intervalLength);
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
        public void StartInterval(float beat, float length)
        {
            intervalStartBeat = beat;
            intervalLength = length;
            defaultIntervalLength = length;
        }
        /// <summary>
        /// Adds an event to the queued events list.
        /// </summary>
        /// <param name="beat">The current beat.</param>
        /// <param name="crParams">Extra properties to add to the event.</param>
        /// <param name="ignoreInterval">If true, this function will not start a new interval if the interval isn't active.</param>
        /// <param name="overrideInterval">If true, overrides the current interval.</param>
        public void AddEvent(float beat, List<CallAndResponseEventParam> crParams = null, bool ignoreInterval = false, bool overrideInterval = false)
        {
            if ((!IntervalIsActive() && !ignoreInterval) || overrideInterval)
            {
                StartInterval(beat, defaultIntervalLength);
            }
            CallAndResponseEvent addedEvent = new CallAndResponseEvent(beat, beat - intervalStartBeat);
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
        /// Passes the turn and returns a list of all events gathered in the interval.
        /// </summary>
        /// <returns></returns>
        public List<CallAndResponseEvent> PassTurn()
        {
            List<CallAndResponseEvent> eventsToReturn = queuedEvents;
            queuedEvents.Clear();

            //Cuts the interval short
            intervalLength = -1;
            intervalStartBeat = -1;
            return eventsToReturn;
        }

        /// <summary>
        /// Check if an event exists at beat.
        /// </summary>
        /// <param name="beat">The beat to check.</param>
        public bool EventExistsAtBeat(float beat)
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
        /// <param name="beat">The beat to check.</param>
        public bool EventExistsAtRelativetBeat(float relativeBeat)
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
