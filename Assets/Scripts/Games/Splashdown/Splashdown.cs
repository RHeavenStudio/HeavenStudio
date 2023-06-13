using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class NtrSplashdownLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("splashdown", "Splashdown", "327BF5", false, false, new List<GameAction>()
            {

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
            float startPos = -Mathf.Floor(amount / 2) * synchretteDistance;

            for (int i = 0; i < amount; i++)
            {
                NtrSynchrette spawnedSynchrette = Instantiate(synchrettePrefab, synchretteHolder);
                spawnedSynchrette.transform.localPosition = new Vector3(startPos + (synchretteDistance * i), 0, 0);
                if (i < amount - 1) currentSynchrettes.Add(spawnedSynchrette);
                else player = spawnedSynchrette;
            }
        }
    }
}

