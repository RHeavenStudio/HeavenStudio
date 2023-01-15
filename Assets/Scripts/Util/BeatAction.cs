using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Util
{
    public class BeatAction : MonoBehaviour
    {
        private int index;
        private List<Action> actions = new List<Action>();

        public delegate void EventCallback();

        public class Action
        {
            public double beat { get; set; }
            public EventCallback function { get; set; }

            public Action(double beat, EventCallback function)
            {
                this.beat = beat;
                this.function = function;
            }
        }

        public static BeatAction New(GameObject prefab, List<Action> actions)
        {
            BeatAction beatAction = prefab.AddComponent<BeatAction>();
            beatAction.actions = actions;

            return beatAction;
        }

        private void Update()
        {
            double songPositionInBeats = Conductor.instance.songPositionInBeatsAsDouble;

            for (int i = 0; i < actions.Count; i++)
            {
                if (songPositionInBeats >= actions[i].beat && index == i)
                {
                    actions[i].function.Invoke();
                    index++;
                }
            }
        }

        public void Delete()
        {
            Destroy(this);
        }
    }

}