using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Util
{
    public class BeatAction
    {
        private int index;
        private List<Action> actions = new List<Action>();
        private Coroutine coroutine;
        private MonoBehaviour behaviour;

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

        public static BeatAction New(MonoBehaviour behaviour, List<Action> actions)
        {
            if (behaviour == null)
            {
                Debug.LogWarning("Starting a BeatAction with no assigned behaviour. The Conductor will be used instead.");
                behaviour = Conductor.instance;
            }
            BeatAction beatAction = new BeatAction();
            beatAction.actions = actions;
            beatAction.behaviour = behaviour;
            beatAction.coroutine = behaviour.StartCoroutine(beatAction.BeatActionRoutine());

            return beatAction;
        }

        IEnumerator BeatActionRoutine()
        {
            int idx = 0;
            WaitUntil waitUntil = new WaitUntil(() => Conductor.instance.songPositionInBeatsAsDouble >= actions[idx].beat || !Conductor.instance.isPlaying);
            while (idx < actions.Count)
            {
                yield return waitUntil;

                if (!Conductor.instance.isPlaying)
                    yield break;

                actions[idx].function.Invoke();
                idx++;
            }
            this.actions = null;
            yield break;
        }

        public void Delete()
        {
            behaviour.StopCoroutine(coroutine);
            this.actions = null;
        }
    }
}