using HeavenStudio.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlTapTroupeLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tapTroupe", "Tap Troupe \n<color=#eb5454>[WIP]</color>", "TAPTAP", false, false, new List<GameAction>()
            {
                new GameAction("stepping", "Stepping")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TapTroupe.PreStepping(e.beat, e.length, e["startTap"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("startTap", false, "Start Tap Voice Line", "Whether or not it should say -Tap!- on the first step.")
                    }
                },
                new GameAction("tapping", "Tapping")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; TapTroupe.PreTapping(e.beat, e.length, e["okay"]); },
                    defaultLength = 3f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("okay", true, "Okay Voice Line", "Whether or not the tappers should say -Okay!- after successfully tapping.")
                    }
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate {TapTroupe.instance.Bop(); },
                    defaultLength = 1f
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_TapTroupe;
    public class TapTroupe : Minigame
    {
        [Header("Components")]
        [SerializeField] TapTroupeTapper playerTapper;
        [SerializeField] TapTroupeCorner playerCorner;
        [SerializeField] List<TapTroupeTapper> npcTappers = new List<TapTroupeTapper>();
        [SerializeField] List<TapTroupeCorner> npcCorners = new List<TapTroupeCorner>();
        [Header("Properties")]
        private static List<QueuedSteps> queuedSteps = new List<QueuedSteps>();
        private static List<QueuedTaps> queuedTaps = new List<QueuedTaps>();
        public struct QueuedSteps
        {
            public float beat;
            public float length;
            public bool startTap;
        }
        public struct QueuedTaps
        {
            public float beat;
            public float length;
            public bool okay;
        }
        private int stepSound = 1;

        public static TapTroupe instance;

        void OnDestroy()
        {
            if (queuedSteps.Count > 0) queuedSteps.Clear();
            if (queuedTaps.Count > 0) queuedTaps.Clear();
        }

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedSteps.Count > 0)
                {
                    foreach (var step in queuedSteps)
                    {
                        Stepping(step.beat, step.length, step.startTap);
                    }
                    queuedSteps.Clear();
                }
                if (queuedTaps.Count > 0)
                {
                    foreach (var tap in queuedTaps)
                    {
                        Tapping(tap.beat, tap.length, tap.okay);
                    }
                    queuedTaps.Clear();
                }
            }
        }

        public static void PreStepping(float beat, float length, bool startTap)
        {
            if (GameManager.instance.currentGame == "tapTroupe")
            {
                TapTroupe.instance.Stepping(beat, length, startTap);

            }
            else
            {
                queuedSteps.Add(new QueuedSteps { beat = beat, length = length, startTap = startTap });
            }
        }

        public void Stepping(float beat, float length, bool startTap)
        {
            for (int i = 0; i < length; i++)
            {
                TapTroupe.instance.ScheduleInput(beat - 1, 1 + i, InputType.STANDARD_DOWN, TapTroupe.instance.JustStep, TapTroupe.instance.MissStep, TapTroupe.instance.Nothing);
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + i, delegate
                    {
                        TapTroupe.instance.NPCStep();
                    })
                });
            }
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1, delegate
                {
                    TapTroupe.instance.NPCStep(false, false);
                    TapTroupe.instance.playerTapper.Step(false, false);
                    TapTroupe.instance.playerCorner.Bop();
                }),
                new BeatAction.Action(beat, delegate { if (startTap) Jukebox.PlayOneShotGame("tapTroupe/startTap"); })
            });
        }

        public static void PreTapping(float beat, float length, bool okay)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTroupe/tapReady1", beat - 2f),
                new MultiSound.Sound("tapTroupe/tapReady2", beat - 1f),
            }, forcePlay: true);
            if (GameManager.instance.currentGame == "tapTroupe")
            {
                TapTroupe.instance.Tapping(beat, length, okay);
            }
            else
            {
                queuedTaps.Add(new QueuedTaps { beat = beat, length = length, okay = okay });
            }
        }

        public void Tapping(float beat, float length, bool okay)
        {
            float actualLength = length - 0.5f;
            actualLength -= actualLength % 0.75f;
            bool secondBam = false;
            if (actualLength < 2.25f) actualLength = 2.25f;
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>
            {
                new MultiSound.Sound("tapTroupe/tapAnd", beat)
            };
            for (float i = 0; i < actualLength; i += 0.75f)
            {
                string soundToPlay = "bamvoice1";
                float beatToSpawn = beat + i + 0.5f;
                if (i + 0.75f >= actualLength)
                {
                    soundToPlay = "startTap";
                    beatToSpawn = Mathf.Ceil(beat + i);
                }
                else if (i + 1.5f >= actualLength)
                {
                    soundToPlay = "tapvoice2";
                }
                else if (i + 2.25f >= actualLength)
                {
                    soundToPlay = "tapvoice1";
                }
                else if (secondBam)
                {
                    soundToPlay = "bamvoice2";
                }
                soundsToPlay.Add(new MultiSound.Sound($"tapTroupe/{soundToPlay}", beatToSpawn));
                secondBam = !secondBam;
            }
            MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);
        }

        public void Bop()
        {
            playerTapper.Bop();
            playerCorner.Bop();
            foreach (var tapper in npcTappers)
            {
                tapper.Bop();
            }
            foreach (var corner in npcCorners)
            {
                corner.Bop();
            }
        }

        public void NPCStep(bool hit = true, bool switchFeet = true)
        {
            foreach (var tapper in npcTappers)
            {
                tapper.Step(hit, switchFeet);
            }
            foreach (var corner in npcCorners)
            {
                corner.Bop();
            }
        }

        void JustStep(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                playerTapper.Step(false);
                playerCorner.Bop();
                return;
            }
            SuccessStep();
        }

        void SuccessStep()
        {
            playerTapper.Step();
            playerCorner.Bop();
            Jukebox.PlayOneShotGame($"tapTroupe/step{stepSound}");
            if (stepSound == 1)
            {
                stepSound = 2;
            }
            else
            {
                stepSound = 1;
            }
        }

        void MissStep(PlayerActionEvent caller)
        {

        }

        void Nothing(PlayerActionEvent caller) { }
    }
}
