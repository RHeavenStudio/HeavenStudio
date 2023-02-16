using HeavenStudio.Util;
using System;
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
                    preFunction = delegate { var e = eventCaller.currentEntity; TapTroupe.Stepping(e.beat, e.length, e["startTap"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("startTap", false, "Start Tap Voice Line", "Whether or not it should say -Tap!- on the first step.")
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
        private static List<float> queuedInputs = new List<float>();
        private int stepSound = 1;

        public static TapTroupe instance;

        void OnDestroy()
        {
            if (queuedInputs.Count > 0) queuedInputs.Clear();
        }

        void Awake()
        {
            instance = this;
        }

        public static void Stepping(float beat, float length, bool startTap)
        {
            if (GameManager.instance.currentGame == "tapTroupe")
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
            else
            {
                for (int i = 0; i < length; i++)
                {
                    queuedInputs.Add(beat + i);
                }
            }
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
