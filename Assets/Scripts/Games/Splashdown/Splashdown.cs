using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class NtrSplashdownLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("splashdown", "Splashdown", "327BF5", false, false, new List<GameAction>()
            {
                new GameAction("dive", "Dive")
                {
                    function = delegate { var e = eventCaller.currentEntity; Splashdown.instance.GoDown(e.beat, e.length); },
                    resizable = true
                },
                new GameAction("appear", "Appear")
                {
                    function = delegate { var e = eventCaller.currentEntity; Splashdown.instance.GoUp(e.beat, e.length, e["type"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", new EntityTypes.Integer(1, 3, 1), "Type")
                    }
                },
                new GameAction("amount", "Synchrette Amount")
                {
                    function = delegate { Splashdown.instance.SpawnSynchrettes(eventCaller.currentEntity["amount"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("amount", new EntityTypes.Integer(3, 5, 3), "Amount")
                    }
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_Splashdown;

    public class Splashdown : Minigame
    {
        public static Splashdown instance;
        [Header("References")]
        [SerializeField] private Transform synchretteHolder;
        [SerializeField] private NtrSynchrette synchrettePrefab;
        [Header("Properties")]
        [SerializeField] private float synchretteDistance;

        private List<NtrSynchrette> currentSynchrettes = new List<NtrSynchrette>();
        private NtrSynchrette player;

        [NonSerialized] public int currentAppearType = 1;

        private void Awake()
        {
            instance = this;
            SpawnSynchrettes(3);
        }

        public void SpawnSynchrettes(int amount)
        {
            if (currentSynchrettes.Count > 0)
            {
                foreach (var synchrette in currentSynchrettes)
                {
                    Destroy(synchrette.gameObject);
                }
                currentSynchrettes.Clear();
            }
            if (player != null) Destroy(player.gameObject);
            float startPos = -((amount / 2) * synchretteDistance) + ((amount % 2 == 0) ? synchretteDistance / 2 : 0);

            for (int i = 0; i < amount; i++)
            {
                NtrSynchrette spawnedSynchrette = Instantiate(synchrettePrefab, synchretteHolder);
                spawnedSynchrette.transform.localPosition = new Vector3(startPos + (synchretteDistance * i), 0, 0);
                if (i < amount - 1) currentSynchrettes.Add(spawnedSynchrette);
                else player = spawnedSynchrette;
            }
        }

        public void GoDown(double beat, float length)
        {
            List<BeatAction.Action> actions = new List<BeatAction.Action>();
            for (int i = 0; i < currentSynchrettes.Count; i++)
            {
                NtrSynchrette synchretteToDive = currentSynchrettes[i];
                double diveBeat = beat + (i * length);
                actions.Add(new BeatAction.Action(diveBeat, delegate
                {
                    synchretteToDive.GoDown();
                }));
                SoundByte.PlayOneShotGame("splashdown/whistle", diveBeat);
                SoundByte.PlayOneShotGame("splashdown/downOthers", diveBeat);
            }
            BeatAction.New(instance.gameObject, actions);
            SoundByte.PlayOneShotGame("splashdown/whistle", beat + (currentSynchrettes.Count * length));
            ScheduleInput(beat, currentSynchrettes.Count * length, InputType.STANDARD_DOWN, JustDown, Out, Out);
        }

        public void GoUp(double beat, float length, int appearType)
        {
            currentAppearType = appearType;
            List<BeatAction.Action> actions = new List<BeatAction.Action>();
            for (int i = 0; i < currentSynchrettes.Count; i++)
            {
                NtrSynchrette synchretteToDive = currentSynchrettes[i];
                double diveBeat = beat + (i * length);
                actions.Add(new BeatAction.Action(diveBeat, delegate
                {
                    synchretteToDive.Appear();
                }));
                SoundByte.PlayOneShotGame("splashdown/whistle", diveBeat);
                SoundByte.PlayOneShotGame("splashdown/upOthers", diveBeat);
            }
            BeatAction.New(instance.gameObject, actions);
            SoundByte.PlayOneShotGame("splashdown/whistle", beat + (currentSynchrettes.Count * length));
            ScheduleInput(beat, currentSynchrettes.Count * length, InputType.STANDARD_UP, JustUp, Out, Out);
        }

        private void JustDown(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("splashdown/downPlayer");
            player.GoDown();
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
            }
        }

        private void JustUp(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("splashdown/upPlayer");
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                player.Appear(true);
                return;
            }
            player.Appear();
        }

        private void Out(PlayerActionEvent caller) { }
    }
}

