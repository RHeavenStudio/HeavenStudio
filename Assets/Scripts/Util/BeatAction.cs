using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace HeavenStudio.Util
{
    public class BeatAction
    {
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

        public static CancellationTokenSource New(MonoBehaviour behaviour, List<Action> actions)
        {
            if (behaviour == null)
            {
                Debug.LogWarning("Starting a BeatAction with no assigned behaviour. The Game Manager will be used instead.");
                behaviour = GameManager.instance;
            }
            CancellationTokenSource cancelToken = new CancellationTokenSource();
            RunAsync(behaviour, actions, cancelToken.Token).Forget();

            return cancelToken;
        }

        static async UniTask RunAsync(MonoBehaviour behaviour, List<Action> actions, CancellationToken token)
        {
            try
            {
                await BeatActionAsync(behaviour, actions, token);
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("BeatAction cancelled.");
            }
        }

        static async UniTask BeatActionAsync(MonoBehaviour behaviour, List<Action> actions, CancellationToken token)
        {
            int idx = 0;
            while (idx < actions.Count)
            {
                await UniTask.WaitUntil(() => Conductor.instance.songPositionInBeatsAsDouble >= actions[idx].beat || !(Conductor.instance.isPlaying || Conductor.instance.isPaused) || behaviour == null, cancellationToken: token);

                if (behaviour == null || !(Conductor.instance.isPlaying || Conductor.instance.isPaused))
                    return;

                actions[idx].function.Invoke();
                idx++;
            }
        }
    }
}