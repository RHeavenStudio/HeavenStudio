using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrFillbotsLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("fillbots", "Fillbots", "FFFFFF", false, false, new List<GameAction>()
            {
                new GameAction("medium", "Medium Bot")
                {
                    preFunction = delegate { Fillbots.PreSpawnFillbot(eventCaller.currentEntity.beat, 4, Scripts_Fillbots.BotSize.Medium, Scripts_Fillbots.BotVariant.Normal); },
                    defaultLength = 8f
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_Fillbots;
    public class Fillbots : Minigame
    {
        private struct QueuedFillbot
        {
            public double beat;
            public double holdLength;
            public BotSize size;
            public BotVariant variant;
        }
        private static List<QueuedFillbot> queuedBots = new List<QueuedFillbot>();

        [Header("Components")]
        [SerializeField] private NtrFillbot mediumBot;

        public static Fillbots instance;

        private void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            if (queuedBots.Count > 0) queuedBots.Clear();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedBots.Count > 0)
                {
                    foreach (var queuedBot in queuedBots)
                    {
                        SpawnFillbot(queuedBot.beat, queuedBot.holdLength, queuedBot.size, queuedBot.variant);
                    }
                    queuedBots.Clear();
                }
            }
        }

        public static void PreSpawnFillbot(double beat, double holdLength, BotSize size, BotVariant variant)
        {
            if (GameManager.instance.currentGame == "fillbots")
            {
                instance.SpawnFillbot(beat, holdLength, size, variant);
            }
            else
            {
                queuedBots.Add(new QueuedFillbot
                {
                    beat = beat,
                    holdLength = holdLength,
                    size = size,
                    variant = variant
                });
            }
        }

        private void SpawnFillbot(double beat, double holdLength, BotSize size, BotVariant variant)
        {
            NtrFillbot spawnedBot = Instantiate(mediumBot, transform);
            spawnedBot.holdLength = holdLength;
            spawnedBot.Init(beat);
        }
    }
}

