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
                    preFunction = delegate { Fillbots.PreSpawnFillbot(eventCaller.currentEntity.beat, 3, Scripts_Fillbots.BotSize.Medium, Scripts_Fillbots.BotVariant.Normal); },
                    defaultLength = 8f
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_Fillbots;
    using System;

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
        public Animator filler;
        [SerializeField] private Transform[] gears;
        [SerializeField] private Animator conveyerBelt;

        public static Fillbots instance;

        [NonSerialized] public List<NtrFillbot> currentBots = new List<NtrFillbot>();

        [NonSerialized] public double conveyerStartBeat = -1;

        [NonSerialized] public float conveyerNormalizedOffset;

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
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    filler.DoScaledAnimationAsync("Hold", 0.5f);
                    SoundByte.PlayOneShotGame("fillbots/armExtension");
                }
                if (PlayerInput.PressedUp() && !IsExpectingInputNow(InputType.STANDARD_UP))
                {
                    filler.DoScaledAnimationAsync("ReleaseWhiff", 0.5f);
                    SoundByte.PlayOneShotGame("fillbots/armRetractionWhiff");
                }

                if (conveyerStartBeat >= 0)
                {
                    float normalizedBeat = cond.GetPositionFromBeat(conveyerStartBeat, 1);

                    if (normalizedBeat >= 0)
                    {
                        for (int i = 0; i < currentBots.Count; i++)
                        {
                            var bot = currentBots[i];
                            bot.MoveConveyer(normalizedBeat);
                        }
                        conveyerBelt.Play("Move", -1, ((normalizedBeat + conveyerNormalizedOffset) % 1) / 4);
                        foreach (var gear in gears)
                        {
                            gear.localEulerAngles = new Vector3(0, 0, Mathf.LerpUnclamped(0, 90, normalizedBeat + conveyerNormalizedOffset));
                        }
                    }
                    else
                    {
                        foreach (var bot in currentBots)
                        {
                            bot.StopConveyer();
                        }
                        conveyerBelt.Play("Move", -1, (conveyerNormalizedOffset % 1) / 4);
                        foreach (var gear in gears)
                        {
                            gear.localEulerAngles = new Vector3(0, 0, Mathf.LerpUnclamped(0, 90, conveyerNormalizedOffset));
                        }
                    }
                }
                else
                {
                    foreach (var bot in currentBots)
                    {
                        bot.StopConveyer();
                    }
                    conveyerBelt.Play("Move", -1, (conveyerNormalizedOffset % 1) / 4);
                    foreach (var gear in gears)
                    {
                        gear.localEulerAngles = new Vector3(0, 0, Mathf.LerpUnclamped(0, 90, conveyerNormalizedOffset));
                    }
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
            if (holdLength > 0)
            {
                spawnedBot.holdLength = holdLength;
            }
            spawnedBot.Init(beat);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    if (conveyerStartBeat != -1) conveyerNormalizedOffset = Conductor.instance.GetPositionFromBeat(conveyerStartBeat, 1);
                    conveyerStartBeat = -2;
                }),
                new BeatAction.Action(beat + 3, delegate
                {
                    if (!PlayerInput.Pressing()) filler.DoScaledAnimationAsync("FillerPrepare", 0.5f);
                    conveyerStartBeat = beat + 3;
                })
            });
        }
    }
}

